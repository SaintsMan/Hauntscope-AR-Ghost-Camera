using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    // The jack-o'-lantern (GDD 5.42): it sits still among a patch of empty pumpkins, and the radar hears all of them. Only
    // the lens tells them apart: a decoy sputters out, spending a bite of battery; the real one wakes with a flare and the
    // hunt goes on as with any ghost. Decoys and ghost share the lens's range and angle, so it is the eyes that decide.
    public sealed class PumpkinPatchAbility : IGhostAbility
    {
        private const int Attempts = 30;
        private const float FullCircle = Mathf.PI * 2f;
        private const float Jitter = 0.1f;
        private const float FullTurn = 360f;

        private readonly int _decoysMin;
        private readonly int _decoysMax;
        private readonly float _minSpacing;
        private readonly float _minCameraDistance;
        private readonly float _revealIn;
        private readonly float _revealOut;
        private readonly float _dudDuration;
        private readonly float _spentBite;
        private readonly float _wakeStagger;
        private readonly Func<Vector3, IGhostView> _createView;
        private readonly List<Decoy> _decoys = new List<Decoy>();
        private readonly List<Vector3> _sources = new List<Vector3>();

        private bool _isPlanted;
        private bool _isAwake;
        private float _elapsed;

        public PumpkinPatchAbility(int decoysMin, int decoysMax, float minSpacing, float minCameraDistance, float revealIn, float revealOut,
            float dudDuration, float spentBite, float wakeStagger, Func<Vector3, IGhostView> createView)
        {
            _decoysMin = decoysMin;
            _decoysMax = Mathf.Max(decoysMin, decoysMax);
            _minSpacing = minSpacing;
            _minCameraDistance = minCameraDistance;
            _revealIn = revealIn;
            _revealOut = revealOut;
            _dudDuration = dudDuration;
            _spentBite = spentBite;
            _wakeStagger = wakeStagger;
            _createView = createView;
        }

        public bool IsAwake => _isAwake;

        // Decoys the lens has not yet unmasked.
        public int DecoysLeft
        {
            get
            {
                var left = 0;
                for (var i = 0; i < _decoys.Count; i++)
                {
                    if (!_decoys[i].IsSpent)
                        left++;
                }

                return left;
            }
        }

        public void Tick(Ghost ghost, float deltaTime)
        {
            _elapsed += deltaTime;
            if (!_isPlanted)
                Plant(ghost);

            if (!_isAwake)
            {
                // Held, not parked by a state: the lens must still be able to wake it, and its bob goes on.
                ghost.Hold(true);
                TickDecoys(ghost, deltaTime);
                if (ghost.Reveal >= ghost.Context.Config.AlertRevealThreshold)
                    Awaken(ghost);
            }

            TickSpent();
        }

        private void Plant(Ghost ghost)
        {
            _isPlanted = true;
            var context = ghost.Context;
            var count = context.Random.Range(_decoysMin, _decoysMax + 1);
            var bounds = context.Planes.RoomBounds;
            var height = ghost.Position.y;
            for (var n = 0; n < count; n++)
            {
                var found = false;
                var position = default(Vector3);
                for (var attempt = 0; attempt < Attempts && !found; attempt++)
                {
                    position = new Vector3(context.Random.Range(bounds.min.x, bounds.max.x), height + context.Random.Range(-Jitter, Jitter),
                        context.Random.Range(bounds.min.z, bounds.max.z));
                    found = IsFree(position, ghost, attempt < Attempts / 2);
                }

                // A cramped room still gets its pumpkins: the last candidate stands even when it sits close to another.
                var decoy = new Decoy(position, _createView(position), context.Random.Range(0f, FullCircle),
                    Quaternion.Euler(0f, context.Random.Range(0f, FullTurn), 0f));
                _decoys.Add(decoy);
                _sources.Add(position);
                decoy.View.SetPose(position, decoy.Rotation);
                decoy.View.SetReveal(0f);
            }

            ghost.SetExtraEmfSources(_sources);
        }

        private bool IsFree(Vector3 position, Ghost ghost, bool strict)
        {
            var spacing = strict ? _minSpacing : _minSpacing * 0.5f;
            if (strict && Horizontal(position, ghost.Context.Camera.Position) < _minCameraDistance)
                return false;
            if (Horizontal(position, ghost.Position) < spacing)
                return false;

            for (var i = 0; i < _decoys.Count; i++)
            {
                if (Horizontal(position, _decoys[i].Position) < spacing)
                    return false;
            }

            return true;
        }

        private void TickDecoys(Ghost ghost, float deltaTime)
        {
            var context = ghost.Context;
            var threshold = context.Config.AlertRevealThreshold;
            var range = context.Detection.RevealRange;
            for (var i = 0; i < _decoys.Count; i++)
            {
                var decoy = _decoys[i];
                if (decoy.IsSpent)
                    continue;

                var seen = context.Lens.Sees(decoy.Position, range);
                decoy.Reveal = Mathf.Clamp01(decoy.Reveal + (seen ? deltaTime / _revealIn : -deltaTime / _revealOut));
                decoy.View.SetReveal(decoy.Reveal);
                decoy.View.SetPose(Bobbed(decoy, context.Config.BobAmplitude, context.Config.BobFrequency), decoy.Rotation);
                if (decoy.Reveal >= threshold)
                    Spend(ghost, decoy);
            }
        }

        private void Spend(Ghost ghost, Decoy decoy)
        {
            decoy.SpentAt = _elapsed;
            _sources.Remove(decoy.Position);
            ghost.Bite(_spentBite);
            ghost.SpendDecoy(decoy.Position);
        }

        private void Awaken(Ghost ghost)
        {
            _isAwake = true;
            ghost.Hold(false);
            ghost.SetExtraEmfSources(null);
            _sources.Clear();
            ghost.Awaken();
            ghost.Stagger(_wakeStagger);
            // The rest of the patch winds down with it: nobody is left to hide among.
            for (var i = 0; i < _decoys.Count; i++)
            {
                if (!_decoys[i].IsSpent)
                    _decoys[i].SpentAt = _elapsed;
            }
        }

        // A spent pumpkin burns out over the dud's length and is then taken away.
        private void TickSpent()
        {
            for (var i = _decoys.Count - 1; i >= 0; i--)
            {
                var decoy = _decoys[i];
                if (!decoy.IsSpent)
                    continue;

                var progress = _dudDuration > 0f ? Mathf.Clamp01((_elapsed - decoy.SpentAt) / _dudDuration) : 1f;
                decoy.View.SetDissolve(progress);
                if (progress < 1f)
                    continue;

                decoy.View.Despawn();
                _decoys.RemoveAt(i);
            }
        }

        private Vector3 Bobbed(Decoy decoy, float amplitude, float frequency)
        {
            return decoy.Position + Vector3.up * (Mathf.Sin(_elapsed * frequency * FullCircle + decoy.Phase) * amplitude);
        }

        private static float Horizontal(Vector3 a, Vector3 b)
        {
            var dx = a.x - b.x;
            var dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        private sealed class Decoy
        {
            public Decoy(Vector3 position, IGhostView view, float phase, Quaternion rotation)
            {
                Position = position;
                View = view;
                Phase = phase;
                Rotation = rotation;
                SpentAt = -1f;
            }

            public Vector3 Position { get; }

            public IGhostView View { get; }

            public float Phase { get; }

            public Quaternion Rotation { get; }

            public float Reveal { get; set; }

            public float SpentAt { get; set; }

            public bool IsSpent => SpentAt >= 0f;
        }
    }
}

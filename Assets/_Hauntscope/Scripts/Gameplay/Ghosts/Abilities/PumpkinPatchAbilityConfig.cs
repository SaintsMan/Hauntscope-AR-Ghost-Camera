using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    [CreateAssetMenu(fileName = "PumpkinPatchAbility", menuName = "Hauntscope/Abilities/Pumpkin Patch")]
    public sealed class PumpkinPatchAbilityConfig : GhostAbilityConfig
    {
        [SerializeField] private GhostView _decoyPrefab;
        [SerializeField] private Color _decoyRimColor = new Color(1f, 0.55f, 0.1f, 1f);
        [SerializeField, Min(0)] private int _decoysMin = 4;
        [SerializeField, Min(0)] private int _decoysMax = 5;
        [SerializeField, Min(0.1f)] private float _minSpacing = 1.1f;
        [SerializeField, Min(0f)] private float _minCameraDistance = 1.3f;
        [SerializeField, Min(0.05f)] private float _revealIn = 0.4f;
        [SerializeField, Min(0.05f)] private float _revealOut = 0.8f;
        [SerializeField, Min(0f)] private float _dudDuration = 0.9f;
        [SerializeField, Range(0f, 0.3f)] private float _spentBite = 0.03f;
        [SerializeField, Min(0f)] private float _wakeStagger = 0.9f;

        public override IGhostAbility CreateAbility()
        {
            return new PumpkinPatchAbility(_decoysMin, _decoysMax, _minSpacing, _minCameraDistance, _revealIn, _revealOut, _dudDuration,
                _spentBite, _wakeStagger, CreateDecoyView);
        }

        private IGhostView CreateDecoyView(Vector3 position)
        {
            var view = Instantiate(_decoyPrefab, position, Quaternion.identity);
            view.name = "PumpkinDecoy";
            view.SetRimColor(_decoyRimColor);
            return view;
        }
    }
}

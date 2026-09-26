using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Observables;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using UnityEngine;

namespace Hauntscope.Gameplay.Photo
{
    // The camcorder's still shutter: a few frames of film per hunt, a score for every shot, and a flash that
    // spooks the ghost, so "photo first or catch first" is the player's call.
    public sealed class SpiritCamera
    {
        private readonly HuntSession _session;
        private readonly PhotoScorer _scorer;
        private readonly IPhotoCapture _capture;
        private readonly PhotoAlbum _album;
        private readonly PhotoAlbumRepository _repository;
        private readonly PhotoConfig _config;
        private readonly IClock _clock;
        private readonly ObservableValue<int> _film = new ObservableValue<int>();
        private readonly ObservableValue<bool> _canShoot = new ObservableValue<bool>();

        private float _cooldown;
        private bool _isShooting;

        public SpiritCamera(
            HuntSession session,
            PhotoScorer scorer,
            IPhotoCapture capture,
            PhotoAlbum album,
            PhotoAlbumRepository repository,
            PhotoConfig config,
            IClock clock)
        {
            _session = session;
            _scorer = scorer;
            _capture = capture;
            _album = album;
            _repository = repository;
            _config = config;
            _clock = clock;
        }

        public event Action ShutterReleased;

        public event Action<PhotoShot> PhotoTaken;

        public IReadOnlyObservableValue<int> Film => _film;

        public IReadOnlyObservableValue<bool> CanShoot => _canShoot;

        public PhotoShot BestShot { get; private set; }

        // Shots of this hunt good enough to count as research evidence for the ghost's file.
        public int EvidenceShots { get; private set; }

        public int Reward => BestShot != null ? _config.RewardFor(BestShot.Score.Stars) : 0;

        public void BeginHunt()
        {
            _film.Value = _config.FilmPerHunt;
            _cooldown = 0f;
            BestShot = null;
            EvidenceShots = 0;
            _canShoot.Value = false;
        }

        public void Tick(float deltaTime)
        {
            _cooldown = Mathf.Max(0f, _cooldown - deltaTime);
            _canShoot.Value = _film.Value > 0 && !_isShooting && _cooldown <= 0f && _scorer.IsInFrame(_session.Ghost.Value);
        }

        public async UniTask<bool> ShootAsync(CancellationToken cancellationToken)
        {
            var ghost = _session.Ghost.Value;
            var data = _session.GhostData;
            if (!_canShoot.Value || ghost == null || data == null)
                return false;

            // Scored at the press, not after the capture frame, so the stars match what the player aimed at.
            var score = _scorer.Score(ghost);
            _isShooting = true;
            _canShoot.Value = false;
            _film.Value--;
            ShutterReleased?.Invoke();
            var taken = _clock.UtcNow;
            try
            {
                var fileName = await _capture.CaptureAsync(new PhotoCaption(data, score.Stars, taken.ToLocalTime()), cancellationToken);
                ghost.Spook();
                var record = new PhotoRecord(fileName, data.Id, score.Stars, new DateTimeOffset(taken).ToUnixTimeSeconds());
                _album.Add(record);
                _repository.Save(_album);
                Keep(new PhotoShot(record, score));
                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                // A failed write must not cost the player their film.
                Debug.LogWarning($"Hauntscope: photo failed: {exception.Message}");
                _film.Value++;
                return false;
            }
            finally
            {
                _isShooting = false;
                _cooldown = _config.ShutterCooldown;
            }
        }

        private void Keep(PhotoShot shot)
        {
            if (shot.Score.Stars >= _config.EvidenceMinStars)
                EvidenceShots++;
            if (BestShot == null || shot.Score.Stars > BestShot.Score.Stars)
                BestShot = shot;
            PhotoTaken?.Invoke(shot);
        }
    }
}

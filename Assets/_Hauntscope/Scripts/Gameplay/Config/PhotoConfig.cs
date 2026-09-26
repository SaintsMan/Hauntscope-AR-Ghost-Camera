using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class PhotoConfig
    {
        [SerializeField, Min(0)] private int _filmPerHunt = 3;
        [SerializeField, Min(0f)] private float _shutterCooldown = 1f;
        [SerializeField, Range(0f, 1f)] private float _revealThreshold = 0.5f;
        [SerializeField, Range(0.01f, 0.5f)] private float _centerRadius = 0.2f;
        [SerializeField, Range(0.05f, 1f)] private float _minFrameFill = 0.3f;
        [SerializeField, Range(0f, 0.2f)] private float _frameMargin = 0.03f;
        [SerializeField] private int[] _starRewards = { 2, 4, 8 };
        [SerializeField, Range(1, 3)] private int _evidenceMinStars = 2;
        [SerializeField, Min(1)] private int _albumLimit = 40;
        [SerializeField, Min(256)] private int _width = 1080;
        [SerializeField, Range(1, 100)] private int _jpegQuality = 85;
        [SerializeField] private string _storeUrl = "https://play.google.com/store/apps/details?id=com.pavko.hauntscope";

        public PhotoConfig()
        {
        }

        public PhotoConfig(
            int filmPerHunt,
            float shutterCooldown,
            float revealThreshold,
            float centerRadius,
            float minFrameFill,
            float frameMargin,
            int[] starRewards,
            int evidenceMinStars,
            int albumLimit)
        {
            _filmPerHunt = filmPerHunt;
            _shutterCooldown = shutterCooldown;
            _revealThreshold = revealThreshold;
            _centerRadius = centerRadius;
            _minFrameFill = minFrameFill;
            _frameMargin = frameMargin;
            _starRewards = starRewards;
            _evidenceMinStars = evidenceMinStars;
            _albumLimit = albumLimit;
        }

        public int FilmPerHunt => _filmPerHunt;

        public float ShutterCooldown => _shutterCooldown;

        // The ghost must be at least this revealed in the lens for the shutter to arm.
        public float RevealThreshold => _revealThreshold;

        // Fraction of the screen width around the centre that counts as a well-framed shot.
        public float CenterRadius => _centerRadius;

        // The whole ghost must fit inside the frame (with this margin) and fill at least this share of its height:
        // a ghost pressed against the lens is a white blur, one across the room is a speck.
        public float MinFrameFill => _minFrameFill;

        public float FrameMargin => _frameMargin;

        public int EvidenceMinStars => _evidenceMinStars;

        public int AlbumLimit => _albumLimit;

        public int Width => _width;

        public int JpegQuality => _jpegQuality;

        public string StoreUrl => _storeUrl;

        public int RewardFor(int stars)
        {
            return stars <= 0 || _starRewards.Length == 0 ? 0 : _starRewards[Mathf.Min(stars, _starRewards.Length) - 1];
        }
    }
}

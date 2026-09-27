using Hauntscope.Gameplay.Config;
using UnityEngine;

namespace Hauntscope.Gameplay.Feedback
{
    // How loud each layer of the hunt's music should be at a given moment (GDD 5.34): the drone under the search, the
    // pulse rising with the EMF, the heart when the ghost is close, the arpeggio once it is revealed, the drums while
    // the beam fights it. Everything drops under a pause and under a running EVP tape.
    public sealed class HuntMusicMix
    {
        private readonly MusicConfig _config;

        public HuntMusicMix(MusicConfig config)
        {
            _config = config;
        }

        public void Fill(in HuntMusicMoment moment, float[] volumes)
        {
            for (var i = 0; i < volumes.Length; i++)
                volumes[i] = 0f;

            if (moment.IsPrank)
            {
                Set(volumes, HuntMusicLayer.Drone, 1f);
                Set(volumes, HuntMusicLayer.Pulse, _config.PrankPulse);
                Set(volumes, HuntMusicLayer.Arp, _config.PrankArp);
            }
            else if (moment.HasResult)
            {
                Set(volumes, HuntMusicLayer.Drone, _config.ResultDrone);
            }
            else if (!moment.IsHunting)
            {
                Set(volumes, HuntMusicLayer.Drone, _config.ScanDrone);
            }
            else
            {
                Set(volumes, HuntMusicLayer.Drone, 1f);
                Set(volumes, HuntMusicLayer.Pulse, Mathf.Lerp(_config.SearchPulse, 1f, Mathf.Clamp01(moment.Closeness)));
                Set(volumes, HuntMusicLayer.Heart, moment.Closeness >= _config.HeartFrom ? 1f : 0f);
                Set(volumes, HuntMusicLayer.Arp, moment.IsRevealed ? 1f : 0f);
                Set(volumes, HuntMusicLayer.Drums, moment.IsCapturing || moment.IsSurging ? 1f : 0f);
            }

            var duck = 1f;
            if (moment.IsPaused)
                duck = _config.PauseDuck;
            if (moment.IsRecording)
                duck = Mathf.Min(duck, _config.RecordingDuck);
            for (var i = 0; i < volumes.Length; i++)
                volumes[i] *= duck;
        }

        private static void Set(float[] volumes, HuntMusicLayer layer, float volume)
        {
            var index = (int)layer;
            if (index < volumes.Length)
                volumes[index] = volume;
        }
    }
}

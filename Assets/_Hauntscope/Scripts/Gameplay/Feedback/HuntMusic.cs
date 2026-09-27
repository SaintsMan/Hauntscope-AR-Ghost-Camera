using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Photo;
using Hauntscope.Gameplay.Tools;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Feedback
{
    // Conducts the hunt's music: every frame it reads the hunt, asks HuntMusicMix how loud each layer should be and hands
    // that to the player; a surge, a catch and an escape each get their stinger.
    public sealed class HuntMusic : IStartable, ITickable, IDisposable
    {
        private readonly IMusicPlayer _player;
        private readonly MusicConfig _config;
        private readonly HuntMusicMix _mix;
        private readonly HuntSession _session;
        private readonly EmfRadar _radar;
        private readonly EmfConfig _emf;
        private readonly Toolbelt _toolbelt;
        private readonly ToolsConfig _tools;
        private readonly HuntPause _pause;
        private readonly PrankPhotoMode _prank;
        private readonly WitchingHour _witchingHour;
        private readonly float[] _volumes = new float[Enum.GetValues(typeof(HuntMusicLayer)).Length];

        private Ghost _ghost;

        public HuntMusic(
            IMusicPlayer player,
            MusicConfig config,
            HuntMusicMix mix,
            HuntSession session,
            EmfRadar radar,
            EmfConfig emf,
            Toolbelt toolbelt,
            ToolsConfig tools,
            HuntPause pause,
            PrankPhotoMode prank,
            WitchingHour witchingHour)
        {
            _player = player;
            _config = config;
            _mix = mix;
            _session = session;
            _radar = radar;
            _emf = emf;
            _toolbelt = toolbelt;
            _tools = tools;
            _pause = pause;
            _prank = prank;
            _witchingHour = witchingHour;
        }

        public void Start()
        {
            _player.Play(_config.HuntLayers, _witchingHour.IsActive ? _config.WitchingHourPitch : 1f);
            _session.Ghost.Changed += Follow;
            _session.Result.Changed += OnResult;
            Follow(_session.Ghost.Value);
        }

        public void Tick()
        {
            _mix.Fill(Moment(), _volumes);
            for (var i = 0; i < _volumes.Length; i++)
                _player.SetLayerVolume(i, _volumes[i] * _config.HuntVolume);
        }

        public void Dispose()
        {
            _session.Ghost.Changed -= Follow;
            _session.Result.Changed -= OnResult;
            Follow(null);
        }

        private HuntMusicMoment Moment()
        {
            var ghost = _session.Ghost.Value;
            var hunting = ghost != null && _session.Result.Value == null;
            return new HuntMusicMoment(
                hunting,
                _emf.MaxLevel > 0 ? (float)_radar.Level.Value / _emf.MaxLevel : 0f,
                hunting && ghost.VisibleReveal >= _tools.BeamRevealThreshold,
                hunting && _toolbelt.Beam.IsActive.Value && ghost.IsBeamed,
                hunting && ghost.IsSurging,
                _session.Result.Value != null,
                _pause.IsPaused,
                _toolbelt.Evp.IsActive.Value,
                _prank.Phase.Value != PrankPhase.Off);
        }

        private void Follow(Ghost ghost)
        {
            if (_ghost != null)
                _ghost.SurgeStarted -= OnSurge;
            _ghost = ghost;
            if (_ghost != null)
                _ghost.SurgeStarted += OnSurge;
        }

        private void OnSurge()
        {
            _player.PlayStinger(_config.SurgeStinger, _config.StingerVolume);
        }

        private void OnResult(HuntResult result)
        {
            if (result == null)
                return;

            var stinger = result.Outcome == HuntOutcome.Captured ? _config.CapturedStinger : _config.EscapedStinger;
            _player.PlayStinger(stinger, _config.StingerVolume);
        }
    }
}

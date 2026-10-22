using System;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Season;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    // The Halloween event as it runs in a test: the console says "halloween" and it is the last week of October, with
    // three skins on the shelf (a beam, a lens frame, a radar) priced 5, 8 and 12.
    public sealed class HalloweenFixture
    {
        public const string BeamId = "beam_orange";
        public const string LensId = "lens_web";
        public const string RadarId = "radar_pumpkin";
        public const int BeamPrice = 5;
        public const int LensPrice = 8;
        public const int RadarPrice = 12;

        public HalloweenFixture(bool running = true, DemoSwitches demo = null)
        {
            Remote = new FakeRemoteValues();
            Clock = new FakeClock { Today = new DateTime(2026, 10, 28) };
            Save = new FakeSaveService();
            Skins = new[]
            {
                new HalloweenSkin(BeamId, HalloweenSkinSlot.Beam, BeamPrice, Color.yellow),
                new HalloweenSkin(LensId, HalloweenSkinSlot.LensFrame, LensPrice, Color.white),
                new HalloweenSkin(RadarId, HalloweenSkinSlot.Radar, RadarPrice, Color.red)
            };
            Config = new HalloweenConfig(35, 1, Skins);
            Demo = demo ?? DemoSwitches.Off;
            Season = new SeasonEvent(new RemoteTuning(Remote), new LiveOpsConfig(), Config, Clock, Demo);
            Repository = new HalloweenRepository(Save);
            Progress = Repository.Load();
            Collection = new HalloweenCollection(Progress, Repository, Season, Config, Clock, Demo, Remote);
            if (running)
                Remote.Values[RemoteKeys.SeasonEvent] = "halloween";

            Collection.Start();
        }

        public FakeRemoteValues Remote { get; }

        public FakeClock Clock { get; }

        public FakeSaveService Save { get; }

        public HalloweenSkin[] Skins { get; }

        public HalloweenConfig Config { get; }

        public DemoSwitches Demo { get; }

        public SeasonEvent Season { get; }

        public HalloweenRepository Repository { get; }

        public HalloweenProgress Progress { get; }

        public HalloweenCollection Collection { get; }

        public HalloweenSkin Beam => Skins[0];

        public HalloweenSkin Lens => Skins[1];

        public HalloweenSkin Radar => Skins[2];

        // A second launch of the game: the same save, a fresh collection.
        public HalloweenCollection Reload()
        {
            var progress = Repository.Load();
            var collection = new HalloweenCollection(progress, Repository, Season, Config, Clock, Demo, Remote);
            collection.Start();
            return collection;
        }
    }
}

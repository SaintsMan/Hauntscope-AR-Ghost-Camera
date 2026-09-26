using System;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Contracts;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Photo;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;
using Hauntscope.Tests.EditMode.Fakes;

namespace Hauntscope.Tests.EditMode
{
    // Everything a hunt report is built from, around one hunt session and a night shift fixture.
    public sealed class ReportFixture : IDisposable
    {
        public const float StaggerHitSeconds = 0.5f;

        public ReportFixture(HuntSession session, GhostFixture ghost, ShiftFixture shift)
        {
            Shift = shift;
            Battery = shift.Battery;
            Beam = TestConfigs.Beam(session, ghost.Camera, TestConfigs.Tools(), shift.Store.Modifiers);
            Loadout = shift.Store.CreateLoadout();
            Spares = new SpareBatteries(shift.Store.Inventory, shift.Store.InventoryRepository, shift.Store.Config, Battery);
            Loot = new HuntLoot();
            var photo = TestConfigs.Photo();
            var storage = new FakePhotoStorage();
            Camera = new SpiritCamera(session, new PhotoScorer(ghost.Camera, photo), new FakePhotoCapture(storage),
                new PhotoAlbum(storage, photo), new PhotoAlbumRepository(new FakeSaveService(), storage), photo, new FakeClock());
            var contracts = new ContractConfig(new[] { ContractTier.Easy }, new[] { 15 }, new[] { 0f }, 1, 1, StaggerHitSeconds,
                new ContractData[0]);
            Builder = new HuntReportBuilder(session, Beam, Battery, Loadout, Spares, Loot, Camera, shift.Shift, contracts);
            Builder.Start();
        }

        public ShiftFixture Shift { get; }

        public Battery Battery { get; }

        public CaptureBeam Beam { get; }

        public HuntLoadout Loadout { get; }

        public SpareBatteries Spares { get; }

        public HuntLoot Loot { get; }

        public SpiritCamera Camera { get; }

        public HuntReportBuilder Builder { get; }

        public void Dispose()
        {
            Builder.Dispose();
        }
    }
}

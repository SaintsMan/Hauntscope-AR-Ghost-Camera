using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class TransitionSceneLoaderTests
    {
        private FakeSceneLoader _inner;
        private FakeScreenTransition _transition;
        private TransitionSceneLoader _loader;

        [SetUp]
        public void SetUp()
        {
            _inner = new FakeSceneLoader();
            _transition = new FakeScreenTransition();
            _loader = new TransitionSceneLoader(_inner, _transition);
        }

        [TearDown]
        public void TearDown()
        {
            _loader.Dispose();
        }

        [Test]
        public void LoadAsync_CoversLoadsAndReveals()
        {
            _loader.LoadAsync(SceneId.Hunt, CancellationToken.None).Forget();

            Assert.AreEqual(1, _transition.CoverCount);
            Assert.AreEqual(SceneId.Hunt, _inner.LastScene);
            Assert.AreEqual(1, _transition.RevealCount);
            Assert.IsFalse(_transition.IsCovered);
        }

        [Test]
        public void LoadAsync_WhileCovering_WaitsBeforeLoading()
        {
            _transition.HoldCover = true;

            _loader.LoadAsync(SceneId.MainMenu, CancellationToken.None).Forget();

            Assert.AreEqual(0, _inner.LoadCount);
            Assert.AreEqual(0, _transition.RevealCount);
        }

        [Test]
        public void LoadAsync_CoverFinishes_LoadsThenReveals()
        {
            _transition.HoldCover = true;
            _loader.LoadAsync(SceneId.MainMenu, CancellationToken.None).Forget();

            _transition.CompleteCover();

            Assert.AreEqual(1, _inner.LoadCount);
            Assert.AreEqual(1, _transition.RevealCount);
        }

        [Test]
        public void LoadAsync_SecondRequestWhileSwitching_IsIgnored()
        {
            _transition.HoldCover = true;
            _loader.LoadAsync(SceneId.MainMenu, CancellationToken.None).Forget();

            _loader.LoadAsync(SceneId.Hunt, CancellationToken.None).Forget();
            _transition.CompleteCover();

            Assert.AreEqual(1, _transition.CoverCount);
            Assert.AreEqual(1, _inner.LoadCount);
            Assert.AreEqual(SceneId.MainMenu, _inner.LastScene);
        }

        [Test]
        public void LoadAsync_AfterSwitchFinished_AcceptsNextRequest()
        {
            _loader.LoadAsync(SceneId.Hunt, CancellationToken.None).Forget();

            _loader.LoadAsync(SceneId.MainMenu, CancellationToken.None).Forget();

            Assert.AreEqual(2, _inner.LoadCount);
            Assert.AreEqual(SceneId.MainMenu, _inner.LastScene);
        }

        [Test]
        public void LoadAsync_CallerCancelledMidSwitch_StillFinishesSwitch()
        {
            _transition.HoldCover = true;
            using var caller = new CancellationTokenSource();
            _loader.LoadAsync(SceneId.MainMenu, caller.Token).Forget();

            caller.Cancel();
            _transition.CompleteCover();

            Assert.AreEqual(1, _inner.LoadCount);
            Assert.AreEqual(1, _transition.RevealCount);
        }

        [Test]
        public void LoadAsync_CallerCancelled_CallerStopsWaiting()
        {
            _transition.HoldCover = true;
            using var caller = new CancellationTokenSource();
            var task = _loader.LoadAsync(SceneId.MainMenu, caller.Token);

            caller.Cancel();

            Assert.AreEqual(UniTaskStatus.Canceled, task.Status);
        }
    }
}

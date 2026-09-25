using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class HuntLauncherTests
    {
        private FakeArAvailability _ar;
        private FakeCameraPermission _camera;
        private FakeApplicationLifecycle _lifecycle;
        private FakeSaveService _save;
        private PlayerProgressRepository _repository;
        private HuntLaunchOptions _options;
        private FakeSceneLoader _sceneLoader;
        private PlayerProgress _progress;
        private HuntLauncher _launcher;
        private bool _isDisposed;

        [SetUp]
        public void SetUp()
        {
            _ar = new FakeArAvailability();
            _camera = new FakeCameraPermission();
            _lifecycle = new FakeApplicationLifecycle();
            _save = new FakeSaveService();
            _repository = new PlayerProgressRepository(_save);
            _options = new HuntLaunchOptions();
            _sceneLoader = new FakeSceneLoader();
            CreateLauncher(new PlayerProgress());
        }

        [TearDown]
        public void TearDown()
        {
            if (!_isDisposed)
                _launcher.Dispose();
        }

        [Test]
        public void Launch_ArSupportedAndCameraGranted_LoadsArWithoutRequest()
        {
            _camera.IsGranted = true;

            Launch();

            AssertLoaded(HuntEnvironment.Ar);
            Assert.AreEqual(0, _camera.RequestCount);
        }

        [Test]
        public void Launch_CameraNotGranted_ExplainsBeforeAnySystemRequest()
        {
            Launch();

            Assert.AreEqual(LaunchPrompt.CameraPermission, _launcher.Prompt.Value);
            Assert.AreEqual(0, _camera.RequestCount);
            Assert.AreEqual(0, _sceneLoader.LoadCount);
        }

        [Test]
        public void AllowCamera_GrantedOnRequest_LoadsAr()
        {
            Launch();
            _camera.NextResult = PermissionResult.Granted;

            Allow();

            Assert.AreEqual(1, _camera.RequestCount);
            AssertLoaded(HuntEnvironment.Ar);
        }

        [Test]
        public void AllowCamera_Denied_KeepsCameraPermissionPrompt()
        {
            Launch();
            _camera.NextResult = PermissionResult.Denied;

            Allow();

            Assert.AreEqual(LaunchPrompt.CameraPermission, _launcher.Prompt.Value);
            Assert.AreEqual(0, _sceneLoader.LoadCount);
        }

        [Test]
        public void Launch_ArUnsupportedFirstTime_ShowsVirtualRoomNotice()
        {
            _ar.Availability = ArAvailabilityResult.Unsupported;

            Launch();

            Assert.AreEqual(LaunchPrompt.VirtualRoomNotice, _launcher.Prompt.Value);
            Assert.AreEqual(0, _sceneLoader.LoadCount);
            Assert.AreEqual(0, _camera.RequestCount);
        }

        [Test]
        public void Launch_ArUnsupportedNoticeAlreadyShown_LoadsVirtual()
        {
            DisposeLauncher();
            CreateLauncher(new PlayerProgress(0, new Dictionary<string, int>(), 0, true));
            _ar.Availability = ArAvailabilityResult.Unsupported;

            Launch();

            AssertLoaded(HuntEnvironment.Virtual);
        }

        [Test]
        public void Launch_NeedsInstallAndInstalled_ContinuesToAr()
        {
            _ar.Availability = ArAvailabilityResult.NeedsInstall;
            _ar.InstallSucceeds = true;
            _camera.IsGranted = true;

            Launch();

            Assert.AreEqual(1, _ar.InstallCount);
            AssertLoaded(HuntEnvironment.Ar);
        }

        [Test]
        public void Launch_NeedsInstallAndDeclined_ShowsVirtualRoomNotice()
        {
            _ar.Availability = ArAvailabilityResult.NeedsInstall;
            _ar.InstallSucceeds = false;

            Launch();

            Assert.AreEqual(LaunchPrompt.VirtualRoomNotice, _launcher.Prompt.Value);
            Assert.AreEqual(0, _camera.RequestCount);
        }

        [Test]
        public void Launch_ArSupported_DoesNotInstall()
        {
            _camera.IsGranted = true;

            Launch();

            Assert.AreEqual(0, _ar.InstallCount);
        }

        [Test]
        public void AllowCamera_DeniedThenGranted_LoadsAr()
        {
            Launch();
            _camera.NextResult = PermissionResult.Denied;
            Allow();
            _camera.NextResult = PermissionResult.Granted;

            Allow();

            Assert.AreEqual(2, _camera.RequestCount);
            AssertLoaded(HuntEnvironment.Ar);
        }

        [Test]
        public void AllowCamera_DeniedPermanently_SwitchesToSettingsPrompt()
        {
            Launch();
            _camera.NextResult = PermissionResult.DeniedPermanently;

            Allow();

            Assert.AreEqual(LaunchPrompt.CameraSettings, _launcher.Prompt.Value);
        }

        [Test]
        public void AllowCamera_SettingsPrompt_OpensSettingsWithoutRequest()
        {
            ReachSettingsPrompt();

            Allow();

            Assert.AreEqual(1, _camera.OpenSettingsCount);
            Assert.AreEqual(1, _camera.RequestCount);
            Assert.AreEqual(LaunchPrompt.CameraSettings, _launcher.Prompt.Value);
        }

        [Test]
        public void AllowCamera_NoPrompt_DoesNothing()
        {
            _launcher.AllowCameraAsync(CancellationToken.None).Forget();

            Assert.AreEqual(0, _camera.RequestCount);
            Assert.AreEqual(0, _camera.OpenSettingsCount);
        }

        [Test]
        public void Resumed_SettingsPromptAndPermissionGranted_LoadsAr()
        {
            ReachSettingsPrompt();
            _camera.IsGranted = true;

            _lifecycle.Resume();

            AssertLoaded(HuntEnvironment.Ar);
        }

        [Test]
        public void Resumed_SettingsPromptAndStillDenied_KeepsPrompt()
        {
            ReachSettingsPrompt();

            _lifecycle.Resume();

            Assert.AreEqual(LaunchPrompt.CameraSettings, _launcher.Prompt.Value);
            Assert.AreEqual(0, _sceneLoader.LoadCount);
        }

        [Test]
        public void Resumed_NoPrompt_DoesNothing()
        {
            _camera.IsGranted = true;

            _lifecycle.Resume();

            Assert.AreEqual(0, _sceneLoader.LoadCount);
        }

        [Test]
        public void Resumed_WhileRequestPending_DoesNotLoadTwice()
        {
            Launch();
            _camera.HoldRequests = true;
            Allow();
            _camera.IsGranted = true;

            _lifecycle.Resume();
            _camera.CompletePending(PermissionResult.Granted);

            Assert.AreEqual(1, _sceneLoader.LoadCount);
        }

        [Test]
        public void Launch_WhileRequestPending_IsIgnored()
        {
            Launch();
            _camera.HoldRequests = true;
            Allow();

            Launch();

            Assert.AreEqual(1, _ar.CheckCount);
            Assert.AreEqual(1, _camera.RequestCount);
        }

        [Test]
        public void PlayVirtual_CameraPrompt_LoadsVirtualAndClearsPrompt()
        {
            Launch();

            _launcher.PlayVirtualAsync(CancellationToken.None).Forget();

            AssertLoaded(HuntEnvironment.Virtual);
            Assert.AreEqual(LaunchPrompt.None, _launcher.Prompt.Value);
        }

        [Test]
        public void PlayVirtual_NoPrompt_DoesNothing()
        {
            _launcher.PlayVirtualAsync(CancellationToken.None).Forget();

            Assert.AreEqual(0, _sceneLoader.LoadCount);
        }

        [Test]
        public void AcknowledgeNotice_NoticePrompt_SavesNoticeAndLoadsVirtual()
        {
            _ar.Availability = ArAvailabilityResult.Unsupported;
            Launch();

            _launcher.AcknowledgeVirtualRoomNoticeAsync(CancellationToken.None).Forget();

            AssertLoaded(HuntEnvironment.Virtual);
            Assert.IsTrue(_progress.VirtualRoomNoticeShown);
            Assert.IsTrue(_repository.Load().VirtualRoomNoticeShown);
        }

        [Test]
        public void CancelPrompt_CameraPrompt_ClearsPromptWithoutRequest()
        {
            Launch();

            var handled = _launcher.CancelPrompt();

            Assert.IsTrue(handled);
            Assert.AreEqual(LaunchPrompt.None, _launcher.Prompt.Value);
            Assert.AreEqual(0, _camera.RequestCount);
            Assert.AreEqual(0, _sceneLoader.LoadCount);
        }

        [Test]
        public void CancelPrompt_VirtualRoomNotice_LeavesNoticeUnseen()
        {
            _ar.Availability = ArAvailabilityResult.Unsupported;
            Launch();

            _launcher.CancelPrompt();

            Assert.IsFalse(_progress.VirtualRoomNoticeShown);
            Assert.AreEqual(0, _sceneLoader.LoadCount);
        }

        [Test]
        public void CancelPrompt_NoPrompt_ReturnsFalse()
        {
            Assert.IsFalse(_launcher.CancelPrompt());
        }

        [Test]
        public void CancelPrompt_WhileRequestPending_KeepsPrompt()
        {
            Launch();
            _camera.HoldRequests = true;
            Allow();

            var handled = _launcher.CancelPrompt();

            Assert.IsFalse(handled);
            Assert.AreEqual(LaunchPrompt.CameraPermission, _launcher.Prompt.Value);
        }

        [Test]
        public void Resumed_AfterCancelledSettingsPrompt_DoesNotLoad()
        {
            ReachSettingsPrompt();
            _launcher.CancelPrompt();
            _camera.IsGranted = true;

            _lifecycle.Resume();

            Assert.AreEqual(0, _sceneLoader.LoadCount);
        }

        [Test]
        public void Dispose_ThenResumed_IsIgnored()
        {
            ReachSettingsPrompt();
            _camera.IsGranted = true;
            DisposeLauncher();

            _lifecycle.Resume();

            Assert.AreEqual(0, _sceneLoader.LoadCount);
        }

        private void CreateLauncher(PlayerProgress progress)
        {
            _progress = progress;
            _launcher = new HuntLauncher(_ar, _camera, _lifecycle, _progress, _repository, _options, _sceneLoader);
            _launcher.Initialize();
            _isDisposed = false;
        }

        private void DisposeLauncher()
        {
            _launcher.Dispose();
            _isDisposed = true;
        }

        private void Allow()
        {
            _launcher.AllowCameraAsync(CancellationToken.None).Forget();
        }

        private void ReachSettingsPrompt()
        {
            Launch();
            _camera.NextResult = PermissionResult.DeniedPermanently;
            Allow();
        }

        private void Launch()
        {
            _launcher.LaunchAsync(CancellationToken.None).Forget();
        }

        private void AssertLoaded(HuntEnvironment environment)
        {
            Assert.AreEqual(1, _sceneLoader.LoadCount);
            Assert.AreEqual(SceneId.Hunt, _sceneLoader.LastScene);
            Assert.AreEqual(environment, _options.Environment);
        }
    }
}

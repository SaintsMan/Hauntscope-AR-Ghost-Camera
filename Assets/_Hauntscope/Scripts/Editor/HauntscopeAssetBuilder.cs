using Hauntscope.Gameplay.Config;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Editor
{
    public static class HauntscopeAssetBuilder
    {
        private const string Root = "Assets/_Hauntscope";
        internal const string GameConfigPath = Root + "/Data/Config/GameConfig.asset";
        private const string SfxFolder = Root + "/Audio/SFX";
        private const string AmbientFolder = Root + "/Audio/Ambient";

        [MenuItem("Hauntscope/Build Assets")]
        public static void Build()
        {
            UiSpriteGenerator.BuildAll();
            TutorialCueGenerator.BuildAll();
            SfxGenerator.BuildAll();
            FontAssetGenerator.BuildAll();
            CreditsExporter.Export();
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(GameConfigPath);
            VfxGenerator.BuildAll(config.Ghost.CaptureDuration);
            SplashAnimationGenerator.Build(config.Boot);
            GhostAssetBuilder.Build();
            PickupAssetBuilder.Build();
            AppIconGenerator.Build();
            StoreAssetBuilder.Build();
            ShiftAssetBuilder.Build();
            ContractAssetBuilder.Build();
            LoginAssetBuilder.Build();
            WireConfig(config);
        }

        private static void WireConfig(GameConfig config)
        {
            var serialized = new SerializedObject(config);
            SetClip(serialized, "_emf._beepClip", SfxFolder, "EmfBeep");
            SetClip(serialized, "_hud._lowBatteryClip", SfxFolder, "BatteryLow");
            SetClip(serialized, "_audio._ambientDrone", AmbientFolder, "AmbientDrone");
            SetClip(serialized, "_audio._ambientStatic", AmbientFolder, "AmbientStatic");
            SetClip(serialized, "_audio._lensOn", SfxFolder, "LensOn");
            SetClip(serialized, "_audio._lensOff", SfxFolder, "LensOff");
            SetClip(serialized, "_audio._beamLoop", SfxFolder, "BeamLoop");
            SetClip(serialized, "_audio._captureSuccess", SfxFolder, "CaptureSuccess");
            SetClip(serialized, "_audio._ghostEscape", SfxFolder, "GhostEscape");
            SetClip(serialized, "_audio._scareSting", SfxFolder, "ScareSting");
            SetClip(serialized, "_audio._teleportWhoosh", SfxFolder, "TeleportWhoosh");
            SetClip(serialized, "_audio._dashWhoosh", SfxFolder, "DashWhoosh");
            SetClip(serialized, "_audio._shriek", SfxFolder, "BansheeShriek");
            SetClip(serialized, "_audio._scanComplete", SfxFolder, "ScanComplete");
            SetClip(serialized, "_audio._uiClick", SfxFolder, "UiClick");
            SetClip(serialized, "_audio._uiBack", SfxFolder, "UiBack");
            SetClip(serialized, "_audio._ghostReveal", SfxFolder, "GhostReveal");
            SetClip(serialized, "_audio._beamLock", SfxFolder, "BeamLock");
            SetClip(serialized, "_audio._splashBoot", SfxFolder, "SplashBoot");
            SetClip(serialized, "_audio._splashOff", SfxFolder, "SplashOff");
            SetClip(serialized, "_audio._bootTick", SfxFolder, "BootTick");
            SetClip(serialized, "_audio._screenOn", SfxFolder, "ScreenOn");
            SetClip(serialized, "_audio._purchase", SfxFolder, "Purchase");
            SetClip(serialized, "_audio._denied", SfxFolder, "Denied");
            SetClip(serialized, "_audio._batteryInsert", SfxFolder, "BatteryInsert");
            SetClip(serialized, "_audio._pickupEcto", SfxFolder, "PickupEcto");
            SetClip(serialized, "_audio._pickupCase", SfxFolder, "PickupCase");
            SetClip(serialized, "_audio._pickupCell", SfxFolder, "PickupCell");
            SetClip(serialized, "_audio._cellBeacon", SfxFolder, "CellBeacon");
            SetClip(serialized, "_audio._emergencyAlarm", SfxFolder, "EmergencyAlarm");
            SetClip(serialized, "_audio._rewardGranted", SfxFolder, "RewardGranted");
            SetClip(serialized, "_audio._ghostStagger", SfxFolder, "GhostStagger");
            SetClip(serialized, "_audio._ghostSurge", SfxFolder, "GhostSurge");
            SetClip(serialized, "_audio._photoShutter", SfxFolder, "PhotoShutter");
            SetClip(serialized, "_audio._lurkerCreak", SfxFolder, "LurkerCreak");
            SetClip(serialized, "_audio._catMeow", SfxFolder, "CatMeow");
            SetClip(serialized, "_audio._catPurr", SfxFolder, "CatPurr");
            serialized.FindProperty("_vfx._captureSpiral").objectReferenceValue = VfxGenerator.CaptureSpiral;
            serialized.FindProperty("_vfx._teleportFlash").objectReferenceValue = VfxGenerator.TeleportFlash;
            serialized.FindProperty("_vfx._revealPulse").objectReferenceValue = VfxGenerator.RevealPulse;
            serialized.FindProperty("_vfx._pickupBurst").objectReferenceValue = VfxGenerator.PickupBurst;
            serialized.FindProperty("_vfx._staggerSparks").objectReferenceValue = VfxGenerator.StaggerSparks;
            serialized.FindProperty("_vfx._coldSpot").objectReferenceValue = VfxGenerator.ColdSpot;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        private static void SetClip(SerializedObject serialized, string property, string folder, string name)
        {
            serialized.FindProperty(property).objectReferenceValue = LoadClip(folder, name);
        }

        private static AudioClip LoadClip(string folder, string name)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{folder}/{name}.wav");
            if (clip == null)
                Debug.LogError($"Hauntscope: missing audio clip {folder}/{name}.wav");
            return clip;
        }
    }
}

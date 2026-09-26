using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Hauntscope.Editor
{
    // Release bundles are signed with the upload key described in a local, git-ignored file, so the open
    // repository never holds a keystore or its passwords. Signing is applied only for the build and cleared
    // afterwards, which also keeps ProjectSettings free of machine-specific paths.
    public static class ReleaseBuilder
    {
        public const string DefaultSigningPath = "Assets/_Hauntscope/Secrets/android-signing.json";
        public const string DefaultAdMobPath = "Assets/_Hauntscope/Secrets/admob.json";

        private const string OutputFolder = "Build";

        [MenuItem("Hauntscope/Build Release AAB")]
        public static void BuildFromMenu()
        {
            Build(DefaultSigningPath);
        }

        [MenuItem("Hauntscope/Bump Version Code")]
        public static void BumpVersionCode()
        {
            // Google Play rejects an upload whose version code isn't higher than every earlier one.
            PlayerSettings.Android.bundleVersionCode++;
            AssetDatabase.SaveAssets();
            Debug.Log($"Hauntscope: version code is now {PlayerSettings.Android.bundleVersionCode}.");
        }

        public static BuildResult Build(string signingPath)
        {
            var signing = LoadSigning(signingPath);
            var output = $"{OutputFolder}/Hauntscope-{PlayerSettings.bundleVersion}-{PlayerSettings.Android.bundleVersionCode}.aab";

            var useCustomKeystore = PlayerSettings.Android.useCustomKeystore;
            var keystoreName = PlayerSettings.Android.keystoreName;
            var keyaliasName = PlayerSettings.Android.keyaliasName;
            var buildAppBundle = EditorUserBuildSettings.buildAppBundle;
            var testAdUnits = SwapInAdUnits(DefaultAdMobPath);
            try
            {
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = Path.GetFullPath(signing.Keystore);
                PlayerSettings.Android.keystorePass = signing.KeystorePassword;
                PlayerSettings.Android.keyaliasName = signing.Alias;
                PlayerSettings.Android.keyaliasPass = signing.AliasPassword;
                EditorUserBuildSettings.buildAppBundle = true;

                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = EnabledScenes(),
                    locationPathName = output,
                    target = BuildTarget.Android,
                    targetGroup = BuildTargetGroup.Android,
                    options = BuildOptions.None
                });

                var summary = report.summary;
                var size = File.Exists(output) ? new FileInfo(output).Length / (1024f * 1024f) : 0f;
                Debug.Log($"Hauntscope: release build {summary.result}, {summary.totalErrors} errors, {summary.totalWarnings} warnings, " +
                          $"{size:F1} MB -> {output}");
                return summary.result;
            }
            finally
            {
                PlayerSettings.Android.keystorePass = string.Empty;
                PlayerSettings.Android.keyaliasPass = string.Empty;
                PlayerSettings.Android.useCustomKeystore = useCustomKeystore;
                PlayerSettings.Android.keystoreName = keystoreName;
                PlayerSettings.Android.keyaliasName = keyaliasName;
                EditorUserBuildSettings.buildAppBundle = buildAppBundle;
                RestoreAdUnits(testAdUnits);
                // The build saves ProjectSettings with the signing applied; saving again writes the restored values.
                AssetDatabase.SaveAssets();
            }
        }

        // Real AdMob IDs live only in the git-ignored Secrets folder: they are written into GameConfig for the build
        // and the test IDs go back afterwards, like the signing passwords. Returns the IDs to restore, or null.
        private static AdUnits SwapInAdUnits(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Hauntscope: '{path}' not found, the release build serves Google's TEST ads.");
                return null;
            }

            var real = JsonUtility.FromJson<AdUnits>(File.ReadAllText(path));
            if (real == null || string.IsNullOrEmpty(real.AndroidAppId) || string.IsNullOrEmpty(real.RewardedUnitId)
                || string.IsNullOrEmpty(real.InterstitialUnitId))
                throw new InvalidOperationException($"'{path}' must define _androidAppId, _rewardedUnitId and _interstitialUnitId.");

            var test = ReadAdUnits();
            WriteAdUnits(real);
            return test;
        }

        private static void RestoreAdUnits(AdUnits test)
        {
            if (test != null)
                WriteAdUnits(test);
        }

        private static AdUnits ReadAdUnits()
        {
            var serialized = new SerializedObject(AssetDatabase.LoadAssetAtPath<ScriptableObject>(HauntscopeAssetBuilder.GameConfigPath));
            return new AdUnits(serialized.FindProperty("_adUnits._androidAppId").stringValue,
                serialized.FindProperty("_adUnits._rewardedUnitId").stringValue,
                serialized.FindProperty("_adUnits._interstitialUnitId").stringValue);
        }

        private static void WriteAdUnits(AdUnits units)
        {
            var config = AssetDatabase.LoadAssetAtPath<ScriptableObject>(HauntscopeAssetBuilder.GameConfigPath);
            var serialized = new SerializedObject(config);
            serialized.FindProperty("_adUnits._androidAppId").stringValue = units.AndroidAppId;
            serialized.FindProperty("_adUnits._rewardedUnitId").stringValue = units.RewardedUnitId;
            serialized.FindProperty("_adUnits._interstitialUnitId").stringValue = units.InterstitialUnitId;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        private static AndroidSigning LoadSigning(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Release signing file not found at '{path}'. See the README section on release builds.", path);

            var signing = JsonUtility.FromJson<AndroidSigning>(File.ReadAllText(path));
            if (signing == null || string.IsNullOrEmpty(signing.Keystore) || string.IsNullOrEmpty(signing.Alias))
                throw new InvalidOperationException($"'{path}' must define _keystore, _keystorePassword, _alias and _aliasPassword.");
            if (!File.Exists(signing.Keystore))
                throw new FileNotFoundException($"Keystore '{signing.Keystore}' from '{path}' does not exist.", signing.Keystore);

            return signing;
        }

        private static string[] EnabledScenes()
        {
            var scenes = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    scenes.Add(scene.path);
            }

            return scenes.ToArray();
        }

        // Same key convention as the signing file: _androidAppId, _rewardedUnitId, _interstitialUnitId.
        [Serializable]
        private sealed class AdUnits
        {
            [SerializeField] private string _androidAppId;
            [SerializeField] private string _rewardedUnitId;
            [SerializeField] private string _interstitialUnitId;

            public AdUnits()
            {
            }

            public AdUnits(string androidAppId, string rewardedUnitId, string interstitialUnitId)
            {
                _androidAppId = androidAppId;
                _rewardedUnitId = rewardedUnitId;
                _interstitialUnitId = interstitialUnitId;
            }

            public string AndroidAppId => _androidAppId;

            public string RewardedUnitId => _rewardedUnitId;

            public string InterstitialUnitId => _interstitialUnitId;
        }

        // JsonUtility maps field names to keys, so the file uses them as-is: _keystore, _keystorePassword, _alias, _aliasPassword.
        [Serializable]
        private sealed class AndroidSigning
        {
            [SerializeField] private string _keystore;
            [SerializeField] private string _keystorePassword;
            [SerializeField] private string _alias;
            [SerializeField] private string _aliasPassword;

            public string Keystore => _keystore;

            public string KeystorePassword => _keystorePassword;

            public string Alias => _alias;

            public string AliasPassword => _aliasPassword;
        }
    }
}

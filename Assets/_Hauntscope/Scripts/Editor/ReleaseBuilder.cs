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
                // The build saves ProjectSettings with the signing applied; saving again writes the restored values.
                AssetDatabase.SaveAssets();
            }
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

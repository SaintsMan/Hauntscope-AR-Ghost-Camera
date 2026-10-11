using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Unity does not keep the Android keystore passwords between Editor sessions. This fills both
// (keystore and key alias) from D:\Keys\android_keystore_password.txt when the Editor loads and before
// every Android build, so the password never sits in the project or in git. Added by LiveOps, 2026-10-10.
// Does nothing when the project signs with the debug keystore (Publishing Settings → Custom Keystore off).
[InitializeOnLoad]
public class LiveOpsKeystorePasswords : IPreprocessBuildWithReport
{
    const string PasswordFile = @"D:\Keys\android_keystore_password.txt";

    public int callbackOrder => -1000;

    static LiveOpsKeystorePasswords()
    {
        EditorApplication.delayCall += () => Apply(false);
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.Android)
            Apply(true);
    }

    [MenuItem("Tools/LiveOps/Fill Keystore Passwords")]
    static void ApplyFromMenu() => Apply(true);

    static void Apply(bool verbose)
    {
        if (!PlayerSettings.Android.useCustomKeystore)
        {
            if (verbose) Debug.Log("[LiveOps] Custom keystore is off — nothing to fill.");
            return;
        }
        if (!File.Exists(PasswordFile))
        {
            Debug.LogWarning($"[LiveOps] {PasswordFile} not found — enter the keystore passwords in Publishing Settings.");
            return;
        }

        string password = File.ReadAllText(PasswordFile).Trim();
        if (password.Length == 0) return;

        if (PlayerSettings.Android.keystorePass != password) PlayerSettings.Android.keystorePass = password;
        if (PlayerSettings.Android.keyaliasPass != password) PlayerSettings.Android.keyaliasPass = password;
        if (verbose) Debug.Log("[LiveOps] Keystore passwords filled from D:\\Keys.");
    }
}

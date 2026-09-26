using System.IO;
using System.Xml;
using Hauntscope.Gameplay.Config;
using UnityEditor;
using UnityEditor.Android;

namespace Hauntscope.Editor
{
    public sealed class AndroidManifestPostprocessor : IPostGenerateGradleAndroidProject
    {
        private const string AndroidNamespace = "http://schemas.android.com/apk/res/android";
        private const string VibratePermission = "android.permission.VIBRATE";
        private const string AdMobAppIdKey = "com.google.android.gms.ads.APPLICATION_ID";

        public int callbackOrder => 0;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            var manifestPath = Path.Combine(path, "src", "main", "AndroidManifest.xml");
            var document = new XmlDocument();
            document.Load(manifestPath);

            AddVibratePermission(document);
            AddAdMobAppId(document);
            document.Save(manifestPath);
        }

        // Unity adds VIBRATE only when Handheld.Vibrate is referenced; AndroidHaptics calls the Vibrator service via JNI.
        private static void AddVibratePermission(XmlDocument document)
        {
            var manifest = document.DocumentElement;
            foreach (XmlElement permission in manifest.GetElementsByTagName("uses-permission"))
            {
                if (permission.GetAttribute("name", AndroidNamespace) == VibratePermission)
                    return;
            }

            var element = document.CreateElement("uses-permission");
            element.SetAttribute("name", AndroidNamespace, VibratePermission);
            manifest.AppendChild(element);
        }

        // The Mobile Ads SDK refuses to start without its app ID in the manifest. It comes from GameConfig, which
        // holds Google's test ID unless ReleaseBuilder swapped in the real one for a release build.
        private static void AddAdMobAppId(XmlDocument document)
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(HauntscopeAssetBuilder.GameConfigPath);
            var application = (XmlElement)document.DocumentElement.GetElementsByTagName("application")[0];
            XmlElement entry = null;
            foreach (XmlElement meta in application.GetElementsByTagName("meta-data"))
            {
                if (meta.GetAttribute("name", AndroidNamespace) == AdMobAppIdKey)
                    entry = meta;
            }

            if (entry == null)
            {
                entry = document.CreateElement("meta-data");
                entry.SetAttribute("name", AndroidNamespace, AdMobAppIdKey);
                application.AppendChild(entry);
            }

            entry.SetAttribute("value", AndroidNamespace, config.AdUnits.AndroidAppId);
        }
    }
}

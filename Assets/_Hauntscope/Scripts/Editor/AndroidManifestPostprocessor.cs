using System.IO;
using System.Xml;
using UnityEditor.Android;

namespace Hauntscope.Editor
{
    public sealed class AndroidManifestPostprocessor : IPostGenerateGradleAndroidProject
    {
        private const string AndroidNamespace = "http://schemas.android.com/apk/res/android";
        private const string VibratePermission = "android.permission.VIBRATE";

        public int callbackOrder => 0;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            var manifestPath = Path.Combine(path, "src", "main", "AndroidManifest.xml");
            var document = new XmlDocument();
            document.Load(manifestPath);

            var manifest = document.DocumentElement;
            foreach (XmlElement permission in manifest.GetElementsByTagName("uses-permission"))
            {
                if (permission.GetAttribute("name", AndroidNamespace) == VibratePermission)
                    return;
            }

            // Unity adds VIBRATE only when Handheld.Vibrate is referenced; AndroidHaptics calls the Vibrator service via JNI.
            var element = document.CreateElement("uses-permission");
            element.SetAttribute("name", AndroidNamespace, VibratePermission);
            manifest.AppendChild(element);
            document.Save(manifestPath);
        }
    }
}

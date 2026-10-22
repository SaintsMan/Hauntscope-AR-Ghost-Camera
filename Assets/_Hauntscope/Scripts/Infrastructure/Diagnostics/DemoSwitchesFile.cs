using System.IO;
using Hauntscope.Gameplay.Season;
using UnityEngine;

namespace Hauntscope.Infrastructure.Diagnostics
{
    // Reads the developer switches (DemoSwitches) from demo.json next to the save files. Players never have the file; on a
    // device it is pushed with adb into the app's files folder. Anything wrong with it means every switch off.
    public static class DemoSwitchesFile
    {
        public const string FileName = "demo.json";

        public static DemoSwitches Read(string folder)
        {
            try
            {
                var path = Path.Combine(folder, FileName);
                if (!File.Exists(path))
                    return DemoSwitches.Off;

                return JsonUtility.FromJson<DemoSwitches>(File.ReadAllText(path)) ?? DemoSwitches.Off;
            }
            catch (System.Exception)
            {
                return DemoSwitches.Off;
            }
        }
    }
}

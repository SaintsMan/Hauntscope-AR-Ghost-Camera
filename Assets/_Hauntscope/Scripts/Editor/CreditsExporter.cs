using System.IO;
using System.Text;
using UnityEditor;

namespace Hauntscope.Editor
{
    // The in-game Credits screen shows CREDITS.md; the markdown table is flattened into readable lines.
    public static class CreditsExporter
    {
        private const string SourcePath = "CREDITS.md";
        private const string TargetPath = "Assets/_Hauntscope/Data/Credits.txt";

        public static void Export()
        {
            var builder = new StringBuilder();
            builder.AppendLine("HAUNTSCOPE");
            builder.AppendLine();
            builder.AppendLine("Game, code, art and sound: Pavko");
            builder.AppendLine("Code: MIT License");
            builder.AppendLine("Art and audio: © 2026 Pavko, all rights reserved");
            builder.AppendLine("Ghosts, UI sprites and sound effects are generated in code.");
            builder.AppendLine();
            builder.AppendLine("THIRD-PARTY");

            foreach (var line in File.ReadAllLines(SourcePath))
            {
                if (!line.StartsWith("|") || line.StartsWith("|---") || line.StartsWith("| Asset"))
                    continue;

                var cells = line.Trim('|').Split('|');
                if (cells.Length < 4)
                    continue;

                builder.AppendLine();
                builder.AppendLine(cells[0].Trim());
                builder.AppendLine(cells[1].Trim());
                builder.AppendLine(cells[3].Split('(')[0].Trim());
            }

            File.WriteAllText(TargetPath, builder.ToString());
            AssetDatabase.ImportAsset(TargetPath);
        }
    }
}

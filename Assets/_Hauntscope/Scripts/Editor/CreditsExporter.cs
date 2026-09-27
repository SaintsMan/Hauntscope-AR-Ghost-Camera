using System.IO;
using System.Text;
using UnityEditor;

namespace Hauntscope.Editor
{
    // The in-game Credits screen shows CREDITS.md; the markdown table is flattened into readable lines, styled like a
    // case file: cyan "// SECTION" headings, the name of each work bright, its author and license dim.
    public static class CreditsExporter
    {
        private const string SourcePath = "CREDITS.md";
        private const string TargetPath = "Assets/_Hauntscope/Data/Credits.txt";
        private const string HeadingColor = "#4FF5E6";
        private const string DimColor = "#7D8B99";

        public static void Export()
        {
            var builder = new StringBuilder();
            Heading(builder, "HAUNTSCOPE");
            Entry(builder, "Game, code, art and sound: Pavko",
                "Code: MIT License",
                "Art and audio: © 2026 Pavko, all rights reserved",
                "Ghosts, UI sprites and sound effects are generated in code.");
            builder.AppendLine();
            Heading(builder, "THIRD-PARTY");

            foreach (var line in File.ReadAllLines(SourcePath))
            {
                if (!line.StartsWith("|") || line.StartsWith("|---") || line.StartsWith("| Asset"))
                    continue;

                // Markdown code spans read as stray backticks on screen.
                var cells = line.Replace("`", string.Empty).Trim('|').Split('|');
                if (cells.Length < 4)
                    continue;

                builder.AppendLine();
                Entry(builder, cells[0].Trim(), cells[1].Trim(), cells[3].Split('(')[0].Trim());
            }

            File.WriteAllText(TargetPath, builder.ToString());
            AssetDatabase.ImportAsset(TargetPath);
        }

        private static void Heading(StringBuilder builder, string title)
        {
            builder.AppendLine($"<color={HeadingColor}><b>// {title}</b></color>");
        }

        private static void Entry(StringBuilder builder, string title, params string[] details)
        {
            builder.AppendLine($"<b>{title}</b>");
            builder.AppendLine($"<color={DimColor}>{string.Join("\n", details)}</color>");
        }
    }
}

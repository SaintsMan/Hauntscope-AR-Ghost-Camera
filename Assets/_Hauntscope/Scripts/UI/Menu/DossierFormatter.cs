using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Hauntscope.UI.Menu
{
    // Lays a case file out as TMP rich text: "// SECTION" headers in the ghost's colour, body text, and locked
    // sections blacked out like a redacted document, with a note on what declassifies them.
    public sealed class DossierFormatter
    {
        // Word lengths of the redaction bars; fixed so a file looks the same every time it is opened.
        private static readonly int[][] RedactedLines =
        {
            new[] { 7, 4, 9, 3, 6 },
            new[] { 5, 8, 4, 7 }
        };

        private readonly StringBuilder _builder = new StringBuilder();
        private readonly string _lockedColor;
        private readonly string _redactionColor;
        private readonly string _bodyColor;

        public DossierFormatter(Color lockedColor, Color redactionColor, Color bodyColor)
        {
            _lockedColor = Hex(lockedColor);
            _redactionColor = Hex(redactionColor);
            _bodyColor = Hex(bodyColor);
        }

        public string Format(IReadOnlyList<DossierSection> sections, Color accent)
        {
            var accentColor = Hex(accent);
            _builder.Clear();
            for (var i = 0; i < sections.Count; i++)
            {
                var section = sections[i];
                if (i > 0)
                    _builder.Append("\n\n");

                _builder.Append("<size=78%><b><color=#").Append(section.IsLocked ? _lockedColor : accentColor).Append(">// ")
                    .Append(section.Title).Append("</color></b></size>\n");

                if (section.IsLocked)
                    AppendRedacted(section.LockHint);
                else
                    _builder.Append("<color=#").Append(_bodyColor).Append('>').Append(section.Body).Append("</color>");
            }

            return _builder.ToString();
        }

        private void AppendRedacted(string hint)
        {
            foreach (var line in RedactedLines)
            {
                for (var w = 0; w < line.Length; w++)
                {
                    if (w > 0)
                        _builder.Append(' ');
                    // Transparent letters under a solid mark: a blacked-out word the width of a real one.
                    _builder.Append("<mark=#").Append(_redactionColor).Append("><color=#00000000>")
                        .Append('x', line[w]).Append("</color></mark>");
                }

                _builder.Append('\n');
            }

            _builder.Append("<size=74%><i><color=#").Append(_lockedColor).Append('>').Append(hint).Append("</color></i></size>");
        }

        private static string Hex(Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }
    }
}

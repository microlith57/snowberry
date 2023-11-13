using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Snowberry;

public partial class FormattedText {
    private readonly (char?, Color)[] characters;

    public FormattedText(string expr) {
        var colors = new Stack<Color>();
        var characters = new List<(char?, Color)>();

        var esc = false;
        for (var i = 0; i < expr.Length; i++) {
            var c = expr[i];
            if (c == '\\') {
                esc = true;
                continue;
            }

            if (!esc) {
                var cmdMatch = CommandRegex().Match(expr.Substring(i));
                if (cmdMatch.Success) {
                    var cmd = cmdMatch.Groups[1].Value;

                    if (ColourCommandRegex().IsMatch(cmd))
                        colors.Push(Calc.HexToColor(cmd.Trim()));
                    else if (ColourPopCommandRegex().IsMatch(cmd)) {
                        if (colors.Count != 0)
                            colors.Pop();
                    } else
                        characters.Add((null, colors.Count == 0 ? Color.White : colors.Peek()));

                    i += cmdMatch.Length - 1;
                    continue;
                }
            }

            esc = false;

            characters.Add((c, colors.Count == 0 ? Color.White : colors.Peek()));
        }

        this.characters = characters.ToArray();
    }

    public string Format(out Color[] colors, params object[] values) {
        var formatted = "";
        var colorList = new List<Color>();
        var v = 0;
        foreach (var pair in characters) {
            if (pair.Item1 == null) {
                if (v < values.Length) {
                    var insert = values[v]?.ToString() ?? "null";
                    formatted += insert;
                    colorList.AddRange(Enumerable.Repeat(pair.Item2, insert.Length));
                    v++;
                }
            } else {
                formatted += pair.Item1;
                colorList.Add(pair.Item2);
            }
        }

        colors = colorList.ToArray();
        return formatted;
    }

    [GeneratedRegex("^{((?:[^{}\\n])*)}")]
    private static partial Regex CommandRegex();
    [GeneratedRegex(@"^\s*#?[a-fA-F0-9]{6}\s*")]
    private static partial Regex ColourCommandRegex();
    [GeneratedRegex(@"^\s*#<<\s*")]
    private static partial Regex ColourPopCommandRegex();
}
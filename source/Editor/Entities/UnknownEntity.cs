using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

// Any entity that doesn't have its own plugin.
public class UnknownEntity : Entity {
    private static readonly Color TriggerColor = Calc.HexToColor("0c5f7a");

    public readonly Dictionary<string, object> Attrs = new();
    public bool LoadedFromTrigger = false;

    private string triggerText = null;

    public override bool IsTrigger => LoadedFromTrigger;

    public override void Set(string option, object value) {
        Attrs[option] = value;
    }

    public override object Get(string option) {
        return Attrs[option];
    }

    public override void Render() {
        base.Render();

        if (LoadedFromTrigger) {
            Rectangle rect = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
            Draw.Rect(rect, TriggerColor * 0.3f);
            Draw.HollowRect(rect, TriggerColor);

            triggerText ??= string.Join(" ", Regex.Split(char.ToUpper(Name[0]) + Name.Substring(1), @"(?=[A-Z])")).Trim();

            Fonts.Pico8.Draw(triggerText, new Vector2(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f), Vector2.One, Vector2.One * 0.5f, Color.Black);
        } else {
            var rect = new Rectangle(Width < 6 ? X - 3 : X, Height < 6 ? Y - 3 : Y, Width < 6 ? 6 : Width, Height < 6 ? 6 : Height);
            Draw.Rect(rect, Color.Red * 0.5f);
        }
    }
}
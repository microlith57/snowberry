using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("water")]
public class Plugin_Water : Entity {

    public static readonly Color WaterColour = Color.LightSkyBlue;

    [Option("hasBottom")] public bool HasBottom = false;

    public override int MinWidth => 8;
    public override int MinHeight => 8;

    public Plugin_Water() {
        Tracked = true;
    }

    public override void Render() {
        base.Render();

        Draw.Rect(Position, Width, Height, WaterColour * 0.3f);
        if (Editor.FancyRender) {
            WobbleLine(Position - Vector2.UnitY, Width, true);
            if (HasBottom)
                WobbleLine(Position + Vector2.UnitY * (Height + 1), Width, false);
        } else {
            Draw.HollowRect(Position, Width, Height, WaterColour * 0.8f);
        }
    }

    public static void WobbleLine(Vector2 position, float width, bool fillDown) {
        const float amplitude = 1;
        Vector2 last = position;
        int lastIdx = (int)(width + 2);
        for (int i = 1; i < lastIdx; i++) {
            var offset = (float)Math.Sin(last.X / 10) * amplitude;
            Vector2 next = position + new Vector2(i, offset);
            if (i != lastIdx - 1)
                Draw.Line(last, next, WaterColour * 0.8f);
            Draw.Line(last, position + new Vector2(i - 1, fillDown ? amplitude : -amplitude), WaterColour * 0.3f);
            last = next;
        }
    }

    public static void AddPlacements() {
        Placements.Create("Water", "water");
    }
}
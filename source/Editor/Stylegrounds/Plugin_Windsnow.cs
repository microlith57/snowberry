using System;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.WindController;

namespace Snowberry.Editor.Stylegrounds;

[Plugin("windsnow")]
public class Plugin_Windsnow : Styleground {

    public const float LoopWidth = 640;
    public const float LoopHeight = 360;

    public override void Render(Room room) {
        base.Render(room);

        bool vertical = room.WindPattern == Patterns.Up;
        int count = vertical ? 144 : 240;
        float rotation = vertical ? -MathHelper.PiOver2 : 0;
        Vector2 scale = PreviewStrength(room.WindPattern).Abs() / 100f;
        scale.X = Math.Max(1, scale.X);
        scale.Y = Math.Max(1, scale.Y);

        Calc.PushRandom(room.Name.GetHashCode());
        for (int i = 0; i < count; i++) {
            var pos = Calc.Random.Range(Vector2.Zero, room.Size * 8);
            GFX.Game["particles/snow"].DrawCentered(pos + room.Position * 8, Color.White, scale, rotation);
        }
        Calc.PopRandom();
    }

    public static Vector2 PreviewStrength(Patterns pattern) {
        return pattern switch {
            Patterns.Left => Vector2.UnitX * -400,
            Patterns.Right => Vector2.UnitX * 400,
            Patterns.LeftStrong => Vector2.UnitX * -800,
            Patterns.RightStrong => Vector2.UnitX * 800,
            Patterns.RightCrazy => Vector2.UnitX * 1200,
            Patterns.Down => Vector2.UnitY * 300,
            Patterns.Up => Vector2.UnitY * -400,
            Patterns.Space => Vector2.UnitY * -600,
            _ => Vector2.Zero
        };
    }
}
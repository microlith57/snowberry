using System;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.WindController;

namespace Snowberry.Editor.Stylegrounds;

[Plugin("windsnow")]
public class Plugin_Windsnow : Styleground {

    // room area: 40x23 = 920
    // 240/920 = ~0.260 wind specks per tile
    // or 0.6x as many with upwind

    public override void Render(Room room) {
        base.Render(room);

        var previewStrength = PreviewStrength(room.WindPattern);
        bool vertical = previewStrength.Y != 0;
        int count = (int)Math.Ceiling(0.260f * room.Width * room.Height * (vertical ? 0.6f : 1));
        float rotation = vertical ? -MathHelper.PiOver2 : 0;
        var scStrength = previewStrength.Abs() / 100;
        Vector2 scale = vertical ? new Vector2(Math.Max(scStrength.Y, 1), 1) : new Vector2(Math.Max(scStrength.X, 1), 1);

        Calc.PushRandom(room.Name.GetHashCode());
        for (int i = 0; i < count; i++) {
            var pos = Calc.Random.Range(Vector2.Zero, room.Size * 8);
            GFX.Game["particles/snow"].DrawCentered(pos + room.Position * 8, Color.White * 0.75f, scale, rotation);
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
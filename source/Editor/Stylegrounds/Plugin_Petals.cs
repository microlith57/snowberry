using System;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Stylegrounds;

[Plugin("petals")]
public class Plugin_Petals : Styleground{
    // wrapping room area: 44x26.5 = 1166
    // 40/1166 = ~0.03431 petals per tile

    public override void Render(Room room){
        base.Render(room);

        Calc.PushRandom((room.Name + "petals").GetHashCode());
        int count = (int)Math.Ceiling((40 / 1166f) * room.Width * room.Height);
        for (int i = 0; i < count; i++) {
            var pos = Calc.Random.Range(Vector2.Zero, room.Size * 8);
            float angleRadians = (float)(Calc.QuarterCircle + Math.Sin(Calc.Random.NextFloat(24) * Calc.Random.Range(0.3f, 0.7f)));
            GFX.Game["particles/petal"].DrawCentered(pos + room.Position * 8, Calc.HexToColor("ff3aa3"), scale: 1, rotation: angleRadians);
        }

        Calc.PopRandom();
    }
}
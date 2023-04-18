using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Stylegrounds;

[Plugin("planets")]
internal class Plugin_Planets : Styleground {
    // Stored as a float and rounded when loading
    [Option("count")] public float Count = 32;
    [Option("size")] public string Size = "small";

    public override void Render(Room room) {
        base.Render(room);

        // room area: 40x23 = 920
        int count = (int)(Count * (room.Width * room.Height) / 920f);
        List<MTexture> textures = GFX.Game.GetAtlasSubtextures("bgs/10/" + Size);

        Calc.PushRandom((room.Name + "planets").GetHashCode());
        for (int i = 0; i < count; i++) {
            var pos = Calc.Random.Range(Vector2.Zero, room.Size * 8);
            Calc.Random.Choose(textures).DrawCentered(pos + room.Position * 8, Color);
        }
        Calc.PopRandom();
    }
}
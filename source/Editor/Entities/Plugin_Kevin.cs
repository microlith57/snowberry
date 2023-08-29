using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Snowberry.Editor.Entities.Util;

namespace Snowberry.Editor.Entities;

[Plugin("crushBlock")]
public class Plugin_Kevin : Entity {

    public static readonly Color kevinColor = Calc.HexToColor("62222b");
    public static readonly EditorNinePatch[] patches = {
        new(GFX.Game["objects/crushblock/block03"]),
        new(GFX.Game["objects/crushblock/block01"]),
        new(GFX.Game["objects/crushblock/block02"])
    };

    [Option("axes")] public CrushBlock.Axes Axes = CrushBlock.Axes.Both;
    [Option("chillout")] public bool Chillout = false;

    public override int MinWidth => 24;
    public override int MinHeight => 24;

    public override void Render() {
        base.Render();

        Draw.Rect(new(Position.X + 4, Position.Y + 4), Width - 4, Height - 4, kevinColor);
        MTexture faceTex = GFX.Game[(Chillout && Width >= 48 && Height >= 48) ? "objects/crushblock/giant_block00" : "objects/crushblock/idle_face"];
        faceTex.DrawCentered(new(Position.X + (Width / 2), Position.Y + (Height / 2)));
        patches[(int)Axes].Draw(Position, Width, Height, Color.White);
    }

    public static void AddPlacements() {
        Placements.Create("Kevin", "crushBlock");
    }
}
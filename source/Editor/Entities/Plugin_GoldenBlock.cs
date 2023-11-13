using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Snowberry.Editor.Entities.Util;

namespace Snowberry.Editor.Entities;

[Plugin("goldenBlock")]
public class Plugin_GoldBlock : Entity {

    public static readonly EditorNinePatch blockTex = new(GFX.Game["objects/goldblock"]);

    public override int MinWidth => 16;
    public override int MinHeight => 16;

    public override void Render() {
        base.Render();

        blockTex.Draw(Position, Width, Height, Color.White);
        MTexture centerTex = GFX.Game["collectables/goldberry/idle00"];
        centerTex.DrawCentered(new(Position.X + (Width / 2), Position.Y + (Height / 2)));
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Golden Block", "goldenBlock");
    }
}
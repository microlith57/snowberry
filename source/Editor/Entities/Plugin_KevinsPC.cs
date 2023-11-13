using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("kevins_pc")]
public class Plugin_KevinsPC : Entity {

    public override void Render() {
        base.Render();
        MTexture compTex = GFX.Game["objects/kevinspc/pc"];
        MTexture specTex = GFX.Game["objects/kevinspc/spectogram"];
        MTexture modSpecTex = specTex.GetSubtexture(39, 0, 32, 18);

        compTex.DrawCentered(Position);
        modSpecTex.DrawJustified(Position + new Vector2(0, 2), new(0.5f, 0.5f));
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(96, 64), justify: new(0.5f, 0.5f));
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Kevin's PC", "kevins_pc");
    }
}
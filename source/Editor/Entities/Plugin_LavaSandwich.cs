using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("sandwichLava")]
public class Plugin_LavaSandwich : Entity {

    public override void Render() {
        base.Render();

        MTexture icon = GFX.Game["plugins/Snowberry/lava_sandwich"];
        icon.DrawCentered(Position);
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(24), justify: new(0.5f));
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Sandwich Lava", "sandwichLava");
    }
}
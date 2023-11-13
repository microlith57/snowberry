using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Celeste;

namespace Snowberry.Editor.Entities;

[Plugin("theoCrystalPedestal")]
public class Plugin_TheoCrystalPedestal : Entity {

    public override void Render() {
        base.Render();

        GFX.Game["characters/theoCrystal/pedestal"].DrawCentered(Position, Color.White, new Vector2(1, 1));
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(42, 44), position: new(0, -8), justify: new(0.5f, 0)); // shoutout to theo crystals for being exactly 21 pixels across
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Theo Crystal Pedestal", "theoCrystalPedestal");
    }
}
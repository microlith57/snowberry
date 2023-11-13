using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Celeste;

namespace Snowberry.Editor.Entities;

[Plugin("theoCrystal")]
public class Plugin_TheoCrystal : Entity {

    public override void Render() {
        base.Render();

        GFX.Game["characters/theoCrystal/idle00"].DrawCentered(Position, Color.White, new Vector2(1, 1));
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(21, 22), position: new(-0.5f, 0), justify: new(0.5f, 0.5f)); // shoutout to theo crystals for being exactly 21 pixels across
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Theo Crystal", "theoCrystal");
    }
}
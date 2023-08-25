using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities;

[Plugin("bigSpinner")]
public class Plugin_Bumper : Entity {
    
    public override void Render() {
        base.Render();

        FromSprite("bumper", "idle")?.DrawCentered(Position, Color.White, new Vector2(1, 1));
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(22, 22), justify: new(0.5f, 0.5f));
    }

    public static void AddPlacements() {
        Placements.Create("Bumper", "bigSpinner");
    }
}
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities;

[Plugin("player")]
public class Plugin_Player : Entity {
    public override void Render() {
        base.Render();

        FromSprite("player", "sitDown")?.DrawCentered(Position - Vector2.UnitY * 16);
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(13, 17), position: new(-1, 0), justify: new(0.5f, 1));
    }

    public static void AddPlacements() {
        Placements.Create("Spawn Point", "player");
    }
}
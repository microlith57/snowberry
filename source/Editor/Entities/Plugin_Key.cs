using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities;

[Plugin("key")]
public class Plugin_Key : Entity {
    public override void Render() {
        base.Render();
        FromSprite("key", "idle")?.DrawCentered(Position);
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(14), justify: new(0.5f));
    }

    public static void AddPlacements() {
        Placements.Create("Key", "key");
    }
}
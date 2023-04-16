using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities;

[Plugin("picoconsole")]
public class Plugin_PicoConsole : Entity {

    public override void Render() {
        base.Render();

        GFX.Game["objects/pico8Console"].DrawJustified(Position, new(0.5f, 1));
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(42, 16), justify: new(0.5f, 1));
    }

    public static void AddPlacements() {
        Placements.Create("Pico8 Console", "picoconsole");
    }
}
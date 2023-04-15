using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities;

[Plugin("touchSwitch")]
public class Plugin_TouchSwitch : Entity {
    public override void Render() {
        base.Render();
        GFX.Game["objects/touchswitch/container"].DrawCentered(Position);
        GFX.Game["objects/touchswitch/icon00"].DrawCentered(Position);
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(14), justify: new(0.5f));
    }

    public static void AddPlacements() {
        Placements.Create("Touch Switch", "touchSwitch");
    }
}
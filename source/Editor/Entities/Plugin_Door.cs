using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities;

[Plugin("door")]
public class Plugin_Door : Entity {

    [Option("type")]
    public DoorType Type = DoorType.wood;

    public override void Render() {
        base.Render();

        FromSprite((Type == DoorType.wood ? "" : Type) + "door", "idle").DrawJustified(Position, new(0.5f, 1));
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(8, 24), justify: new(0.5f, 1));
    }

    public static void AddPlacements() {
        Placements.Create("Door (Wood)", "door", new(){ ["type"] = "wood" });
        Placements.Create("Door (Metal)", "door", new(){ ["type"] = "metal" });
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")] // must exactly match
    public enum DoorType {
        wood, metal
    }
}
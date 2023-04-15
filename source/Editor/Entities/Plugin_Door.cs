namespace Snowberry.Editor.Entities;

[Plugin("door")]
public class Plugin_Door : Entity {

    // any string is allowed, TODO: suggestions
    [Option("type")]
    public string Type = "wood";

    public override void Render() {
        base.Render();

        FromSprite((Type == "wood" ? "" : Type) + "door", "idle").DrawJustified(Position, new(0.5f, 1));
    }

    public static void AddPlacements() {
        Placements.Create("Door (Wood)", "door", new(){ ["type"] = "wood" });
        Placements.Create("Door (Metal)", "door", new(){ ["type"] = "metal" });
    }
}
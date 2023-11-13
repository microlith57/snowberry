namespace Snowberry.Editor.Entities;

[Plugin("friendlyGhost")]
public class Plugin_FriendlyGhost : Entity {

    public override void Render() {
        base.Render();

        FromSprite("oshiro_boss", "idle").DrawCentered(Position);
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Oshiro Boss", "friendlyGhost");
    }
}
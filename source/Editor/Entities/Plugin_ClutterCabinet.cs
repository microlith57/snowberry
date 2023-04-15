using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities;

[Plugin("clutterCabinet")]
public class Plugin_ClutterCabinet : Entity {

    public override void Render() {
        base.Render();

        FromSprite("clutterCabinet", "idle").DrawCentered(Position + new Vector2(8, 8));
    }

    public static void AddPlacements() {
        Placements.Create("Clutter Cabinet", "clutterCabinet");
    }
}
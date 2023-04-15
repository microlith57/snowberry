using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities;

[Plugin("trapdoor")]
public class Plugin_Trapdoor : Entity {

    public override void Render() {
        base.Render();

        FromSprite("trapdoor", "idle").Draw(Position + new Vector2(6, 0));
    }

    public static void AddPlacements() {
        Placements.Create("Trapdoor", "trapdoor");
    }
}
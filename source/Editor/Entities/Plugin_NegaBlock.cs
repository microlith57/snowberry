using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("negaBlock")]
public class Plugin_NegaBlock : Entity {

    public override int MinWidth => 8;
    public override int MinHeight => 8;

    public override void Render() {
        base.Render();

        Draw.Rect(Position, Width, Height, Color.Red);
    }

    public static void AddPlacements() {
        Placements.Create("Nega Block", "negaBlock");
    }
}
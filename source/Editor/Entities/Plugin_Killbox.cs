using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("killbox")]
public class Plugin_Killbox : Entity {

    public static readonly Color killboxColour = Calc.HexToColor("CC6666");

    public override int MinWidth => 8;
    public override int MinHeight => 32;

    public override void Render() {
        base.Render();

        Draw.Rect(Position, Width, Height, killboxColour * 0.8f);
    }

    public static void AddPlacements() {
        Placements.Create("Killbox", "killbox");
    }
}
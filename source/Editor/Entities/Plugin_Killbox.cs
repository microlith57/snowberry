using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("killbox")]
public class Plugin_Killbox : Entity {

    public static readonly Color killboxColour = Calc.HexToColor("CC6666");

    public override int MinWidth => 8;

    public override void Render() {
        base.Render();

        Draw.Rect(Position, Width, 32, killboxColour * 0.8f);
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(Width, 32));
    }

    public static void AddPlacements() {
        Placements.Create("Killbox", "killbox");
    }
}
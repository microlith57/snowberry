using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("invisibleBarrier")]
public class Plugin_InvisibleBarrier : Entity {

    public static readonly Color BarrierColour = Calc.HexToColor("666666");

    public override int MinWidth => 8;
    public override int MinHeight => 8;

    public override void Render() {
        base.Render();

        Draw.Rect(Position, Width, Height, BarrierColour * 0.8f);
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Invisible Barrier", "invisibleBarrier");
    }
}
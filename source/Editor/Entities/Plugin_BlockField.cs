using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("blockField")]
public class Plugin_BlockField : Entity {

    public static readonly Color BlockfieldColour = Calc.HexToColor("6666ff");

    public override int MinWidth => 8;
    public override int MinHeight => 8;

    public override void Render() {
        base.Render();

        Draw.Rect(Position, Width, Height, BlockfieldColour * 0.4f);
        Draw.HollowRect(Position, Width, Height, BlockfieldColour);
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Strawberry Blockfield", "blockField");
    }
}
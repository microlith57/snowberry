using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("iceBlock")]
public class Plugin_IceBlock : Entity {

    private static readonly Color BorderColor = Calc.HexToColor("6CD6EB");
    private static readonly Color InnerColor = Calc.HexToColor("4CA8D6");

    public override int MinWidth => 8;
    public override int MinHeight => 8;

    public override void Render() {
        base.Render();

        Draw.HollowRect(Position, Width, Height, BorderColor);
        Draw.Rect(new(Position.X + 1, Position.Y + 1), Width - 2, Height - 2, InnerColor * 0.4f);
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Ice Block", "iceBlock");
    }
}
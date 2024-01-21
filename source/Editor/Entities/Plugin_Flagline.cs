using Microsoft.Xna.Framework;
using Monocle;
using Snowberry.Editor.Entities.Util;

namespace Snowberry.Editor.Entities;

[Plugin("cliffflag")]
public class Plugin_Flagline : Entity {

    public static readonly Color[] Colours = [
        Calc.HexToColor("d85f2f"),
        Calc.HexToColor("d82f63"),
        Calc.HexToColor("2fd8a2"),
        Calc.HexToColor("d8d62f")
    ];
    public static readonly Color LineColour = Color.Lerp(Color.Gray, Color.DarkBlue, 0.25f);
    public static readonly Color PinColour = Color.Gray;

    public override int MinNodes => 1;
    public override int MaxNodes => 1;

    private readonly EditorFlagline flagline;

    public Plugin_Flagline() {
        flagline = new EditorFlagline(Position, Position, LineColour, PinColour, Colours, 10, 10, 10, 10, 2, 8) {
            ClothDroopAmount = 0.2f
        };
    }

    public override void Render() {
        base.Render();
        flagline.From = Position;
        flagline.To = Nodes[0];
        flagline.Render();
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Flagline", "cliffflag");
    }
}
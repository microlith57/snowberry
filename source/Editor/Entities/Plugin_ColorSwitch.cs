using Celeste;
using Microsoft.Xna.Framework;
using static Celeste.ClutterBlock;

namespace Snowberry.Editor.Entities;

[Plugin("colorSwitch")]
public class Plugin_ColorSwitch : Entity {

    [Option("type")]
    public Colors Colour = Colors.Green;

    public override void Render() {
        base.Render();

        FromSprite("clutterSwitch", "idle").DrawJustified(Position + new Vector2(16, 16), new(0.5f, 1));
        GFX.Game["objects/resortclutter/icon_" + Colour].DrawCentered(Position + new Vector2(16, 8));
    }

    public static void AddPlacements() {
        foreach (var colour in new[]{ Colors.Red, Colors.Green, Colors.Yellow, Colors.Lightning })
            Placements.Create($"Clutter Switch ({colour})", "colorSwitch", new() { ["type"] = colour.ToString() });
    }
}
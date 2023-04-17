using Celeste;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Triggers;

[Plugin("windTrigger")]
public class Plugin_WindTrigger : Trigger {

    [Option("pattern")] public WindController.Patterns Pattern = WindController.Patterns.None;

    public override void Render() {
        base.Render();
        Fonts.Pico8.Draw($"({Pattern})", Center + Vector2.UnitY * 6, Vector2.One, new(0.5f), Color.Black);
    }

    public new static void AddPlacements() {
        Placements.Create("Wind Trigger", "windTrigger");
    }
}
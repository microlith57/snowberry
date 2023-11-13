using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Triggers;

[Plugin("moonGlitchBackgroundTrigger")]
public class Plugin_MoonGlitchBackgroundTrigger : Trigger {

    [Option("duration")] public Duration duration = Duration.Short;
    [Option("stay")] public bool Stay = false;
    [Option("glitch")] public bool Glitch = true;

    public override void Render() {
        base.Render();

        Fonts.Pico8.Draw($"({duration})", Center + new Vector2(0, 6), Vector2.One, new(0.5f), Color.Black);
    }

    public new static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Moon Glitch Background Trigger", "moonGlitchBackgroundTrigger", trigger: true);
    }

    public enum Duration{
        Short,
        Medium,
        Long
    }
}
using Microsoft.Xna.Framework;
using static Celeste.Trigger;

namespace Snowberry.Editor.Triggers;

[Plugin("bloomFadeTrigger")]
public class Plugin_BloomFadeTrigger : Trigger {
    [Option("positionMode")] public PositionModes PositionMode = PositionModes.NoEffect;
    [Option("bloomAddFrom")] public float From = 0;
    [Option("bloomAddTo")] public float To = 0;

    public override void Render() {
        base.Render();
        var str = (PositionMode == PositionModes.NoEffect || From == To) ? $"(bloom = {To})" : $"(bloom: {From} -> {To})";
        Fonts.Pico8.Draw(str, Center + Vector2.UnitY * 6, Vector2.One, new(0.5f), Color.Black);
    }

    public new static void AddPlacements() {
        Placements.Create("Bloom Fade Trigger", "bloomFadeTrigger");
    }
}
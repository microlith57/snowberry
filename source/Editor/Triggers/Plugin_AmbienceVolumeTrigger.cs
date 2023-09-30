using Microsoft.Xna.Framework;
using static Celeste.Trigger;

namespace Snowberry.Editor.Triggers;

[Plugin("everest/AmbienceVolumeTrigger")]
public class Plugin_AmbienceVolumeTrigger : Trigger {
    [Option("direction")] public PositionModes PositionMode = PositionModes.NoEffect;
    [Option("from")] public float From = 0;
    [Option("to")] public float To = 0;

    public override void Render() {
        base.Render();

        var str = (PositionMode == PositionModes.NoEffect || From == To) ? $"(vol = {To})" : $"(vol: {From} -> {To})";
        Fonts.Pico8.Draw(str, Center + Vector2.UnitY * 6, Vector2.One, new(0.5f), Color.Black);
    }

    public new static void AddPlacements() {
        Placements.Create("Ambience Volume Trigger (Everest)", "everest/AmbienceVolumeTrigger", trigger: true);
    }
}
using Microsoft.Xna.Framework;
using static Celeste.Trigger;

namespace Snowberry.Editor.Triggers;

[Plugin("ambienceParamTrigger")]
public class Plugin_AmbienceParameterTrigger : Trigger {
    [Option("direction")] public PositionModes Direction = PositionModes.NoEffect;
    [Option("parameter")] public string Parameter = "";
    [Option("from")] public float From = 0;
    [Option("to")] public float To = 0;

    public override void Render() {
        base.Render();
        var str = Direction == PositionModes.NoEffect ? $"(\"{Parameter}\" = {To})" : $"(\"{Parameter}\": {From} -> {To})";
        Fonts.Pico8.Draw(str, Center + Vector2.UnitY * 6, Vector2.One, new(0.5f), Color.Black);
    }

    public new static void AddPlacements() {
        Placements.Create("Ambience Parameter Trigger", "ambienceParamTrigger");
    }
}
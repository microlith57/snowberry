using Microsoft.Xna.Framework;
using static Celeste.Session;

namespace Snowberry.Editor.Triggers;

[Plugin("everest/coreModeTrigger")]
public class Plugin_CoreModeTrigger : Trigger {
    [Option("mode")] public CoreModes CoreMode = CoreModes.None;
    [Option("playEffects")] public bool PlayEffects = true;

    public override void Render() {
        base.Render();
        var str = $"({CoreMode})";
        Fonts.Pico8.Draw(str, Center + Vector2.UnitY * 6, Vector2.One, new(0.5f), Color.Black);
    }

    public new static void AddPlacements() {
        Placements.Create("Core Mode Trigger (Everest)", "everest/coreModeTrigger", trigger: true);
    }
}
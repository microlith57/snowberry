using Microsoft.Xna.Framework;
using static Celeste.Mod.Entities.CrystalShatterTrigger;

namespace Snowberry.Editor.Triggers;

[Plugin("everest/crystalShatterTrigger")]
public class Plugin_CrystalShatterTrigger : Trigger {
    [Option("mode")] public Modes Mode = Modes.All;

    public override void Render() {
        base.Render();
        var str = $"({Mode})";
        Fonts.Pico8.Draw(str, Center + Vector2.UnitY * 6, Vector2.One, new(0.5f), Color.Black);
    }

    public new static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Crystal Shatter Trigger (Everest)", "everest/crystalShatterTrigger", trigger: true);
    }
}
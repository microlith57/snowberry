using System.Collections.Generic;

namespace Snowberry.Editor.Entities;

[Plugin("booster")]
public class Plugin_Booster : Entity {
    [Option("red")] public bool Red;
    [Option("ch9_hub_booster")] public bool Ch9Hub;

    public override void Render() {
        base.Render();

        FromSprite(Red ? "boosterRed" : "booster", "loop")?.DrawOutlineCentered(Position);
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Booster (Green)", "booster", new Dictionary<string, object>() { { "red", false } });
        Placements.EntityPlacementProvider.Create("Booster (Red)", "booster", new Dictionary<string, object>() { { "red", true } });
    }
}
using Celeste;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("foregroundDebris")]
public class Plugin_ForegroundDebris : Entity {
    private MTexture[] debris;

    public override void Initialize() {
        base.Initialize();
        if (Calc.Random.Next(2) == 1)
            debris = [
                GFX.Game["scenery/fgdebris/rock_a00"],
                GFX.Game["scenery/fgdebris/rock_a01"],
                GFX.Game["scenery/fgdebris/rock_a02"]
            ];
        else
            debris = [
                GFX.Game["scenery/fgdebris/rock_b00"],
                GFX.Game["scenery/fgdebris/rock_b01"]
            ];
    }

    public override void Render() {
        base.Render();
        foreach (MTexture t in debris)
            t.DrawCentered(Position);
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Foreground Debris", "foregroundDebris");
    }
}
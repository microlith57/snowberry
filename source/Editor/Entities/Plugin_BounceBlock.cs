using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Snowberry.Editor.Entities.Util;

namespace Snowberry.Editor.Entities;

[Plugin("bounceBlock")]
public class Plugin_BounceBlock : Entity {

    public override int MinWidth => 24;
    public override int MinHeight => 24;

    private static EditorNinePatch hotPatch = new(GFX.Game["objects/bumpblocknew/fire00"]);
    private static EditorNinePatch coldPatch = new(GFX.Game["objects/bumpblocknew/ice00"]);
    private MTexture hotCrystal;
    private MTexture coldCrystal;

    [Option("notCoreMode")] public bool NotCoreMode = false;

    public Plugin_BounceBlock() {
        hotCrystal = GFX.Game["objects/bumpblocknew/fire_center00"];
        coldCrystal = GFX.Game["objects/bumpblocknew/ice_center00"];
    }

    public override void Render() {
        base.Render();
        if (NotCoreMode) {
            coldPatch.Draw(Position, Width, Height, Color.White);
            coldCrystal.DrawCentered(Position + new Vector2(Width, Height) / 2);
        } else {
            hotPatch.Draw(Position, Width, Height, Color.White);
            hotCrystal.DrawCentered(Position + new Vector2(Width, Height) / 2);
        }
    }

    public static void AddPlacements() {
        Placements.Create("Bounce Block", "bounceBlock");
    }
}
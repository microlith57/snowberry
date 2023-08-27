using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Snowberry.Editor.Entities.Util;

namespace Snowberry.Editor.Entities;

[Plugin("bounceBlock")]
public class Plugin_BounceBlock : Entity {

    public override int MinWidth => 24;
    public override int MinHeight => 24;

    private EditorNinePatch hotPatch;
    private EditorNinePatch coldPatch;
    private MTexture hotCrystal;
    private MTexture coldCrystal;

    [Option("notCoreMode")] public bool notCoreMode;

    public Plugin_BounceBlock() {
        hotPatch = new EditorNinePatch(GFX.Game["objects/bumpblocknew/fire00"]);
        coldPatch = new EditorNinePatch(GFX.Game["objects/bumpblocknew/ice00"]);
        hotCrystal = GFX.Game["objects/bumpblocknew/fire_center00"];
        coldCrystal = GFX.Game["objects/bumpblocknew/ice_center00"];
    }

    public override void RenderBefore() {
        base.RenderBefore();
    }
    public override void Render() {
        base.Render();
        if (notCoreMode) {
            coldPatch.Draw(Position, Width, Height, Color.White);
            coldCrystal.DrawCentered(Position + new Vector2(Width, Height) / 2);
        } else {
            hotPatch.Draw(Position, Width, Height, Color.White);
            hotCrystal.DrawCentered(Position + new Vector2(Width, Height) / 2);
        }
    }

    public static void AddPlacements() {
        Placements.Create("Core Block", "BounceBlock", new() { ["notCoreMode"] = false });
    }
}
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("waveDashTutorialMachine")]
public class Plugin_InternetCafe : Entity {

    public static readonly MTexture BackTex = GFX.Game["objects/wavedashtutorial/building_back"];
    public static readonly MTexture LeftTex = GFX.Game["objects/wavedashtutorial/building_front_left"];
    public static readonly MTexture RightTex = GFX.Game["objects/wavedashtutorial/building_front_right"];

    public override void Render() {
        base.Render();

        BackTex.DrawJustified(Position, new(0.5f, 1.0f));
        LeftTex.DrawJustified(Position, new(0.5f, 1.0f));
        RightTex.DrawJustified(Position, new(0.5f, 1.0f));
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(120, 79), justify: new(0.5f, 1));
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Internet Cafe", "waveDashTutorialMachine");
    }
}
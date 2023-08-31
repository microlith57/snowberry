using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("summitCloud")]
public class Plugin_SummitCloud : Entity {

    public static readonly MTexture[] sprites = {
        GFX.Game["scenery/summitclouds/cloud00"],
        GFX.Game["scenery/summitclouds/cloud01"],
        GFX.Game["scenery/summitclouds/cloud03"]
    };

    public override void Render() {
        base.Render();

        MTexture cloudTex = Calc.Random.Choose(sprites);
        cloudTex.DrawCentered(Position);
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(128, 64), justify: new(0.5f, 0.5f));
    }

    public static void AddPlacements() {
        Placements.Create("Summit Cloud", "summitCloud");
    }
}
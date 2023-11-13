using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities;

[Plugin("summitcheckpoint")]
public class Plugin_SummitCheckpoint : Entity{

    [Option("number")] public int Number = 1;

    public override void Render(){
        base.Render();

        var tex = GFX.Game["scenery/summitcheckpoints/base00"];
        tex.Draw(Position - new Vector2(tex.Width / 2 + 1, tex.Height / 2));

        var numberString = Number.ToString("D2");
        var digits = GFX.Game.GetAtlasSubtextures("scenery/summitcheckpoints/numberbg");
        digits[numberString[0] - 48].DrawJustified(Position + new Vector2(-1, 1), new Vector2(1, 0));
        digits[numberString[1] - 48].DrawJustified(Position + new Vector2(0, 1), new Vector2(0, 0));
    }

    protected override IEnumerable<Rectangle> Select(){
        var tex = GFX.Game["scenery/summitcheckpoints/base00"];
        yield return RectOnRelative(new(11, 21), position: -new Vector2(tex.Width / 2, tex.Height / 2 - 1));
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Summit Checkpoint", "summitcheckpoint");
    }
}
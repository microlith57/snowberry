using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("everest/starClimbGraphicsController")]
public class Plugin_StarClimbGraphicsController : Entity {

    [Option("bgColor")] public Color BgColor = Calc.HexToColor("A3FFFF");
    [Option("fgColor")] public Color FgColor = Calc.HexToColor("293E4B");

    public override void Render() {
        base.Render();

        MTexture icon = GFX.Game["plugins/Snowberry/northern_lights"];
        icon.DrawCentered(Position);
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(24), justify: new(0.5f));
    }

    public static void AddPlacements() {
        Placements.Create("Star Climb Graphics Controller (Everest)", "everest/starClimbGraphicsController");
    }
}
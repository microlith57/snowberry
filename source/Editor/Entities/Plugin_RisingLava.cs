using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("risingLava")]
public class Plugin_RisingLava : Entity {

    [Option("intro")] public bool Intro = false;

    public override void Render() {
        base.Render();

        MTexture icon = GFX.Game["plugins/Snowberry/rising_lava"];
        icon.DrawCentered(Position);
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(24), justify: new(0.5f));
    }

    public static void AddPlacements() {
        Placements.Create("Rising Lava", "risingLava");
    }
}
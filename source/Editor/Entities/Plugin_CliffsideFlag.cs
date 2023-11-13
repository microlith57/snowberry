using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("cliffside_flag")]
public class Plugin_CliffsideFlag : Entity {

    // TODO: bound in 0-10
    [Option("index")] public int Index = 0;

    public override void Render() {
        base.Render();

        GetTexture().Draw(Position);
    }

    protected override IEnumerable<Rectangle> Select() {
        var tex = GetTexture();
        yield return RectOnRelative(new(tex.Width, tex.Height));
    }

    public MTexture GetTexture() => GFX.Game.GetAtlasSubtexturesAt("scenery/cliffside/flag", Index);

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Cliffside Flag", "cliffside_flag");
    }
}
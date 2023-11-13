using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("bigSpinner")]
public class Plugin_Bumper : Entity {

    public override int MaxNodes => 1;

    public override void Render() {
        base.Render();

        MTexture sprite = GFX.Game["objects/Bumper/Idle30"];
        sprite?.DrawCentered(Position);

        if(Nodes.Count != 0)
            sprite?.DrawCentered(Nodes[0], Color.White * 0.5f);
    }

    public override void HQRender() {
        base.HQRender();

        if (Nodes.Count != 0)
            DrawUtil.DottedLine(Center, Nodes[0] + new Vector2(Width, Height) / 2f, Color.White, 4, 2);
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(22, 22), justify: new(0.5f, 0.5f));
        foreach (var node in Nodes)
            yield return RectOnAbsolute(new(22, 22), position: node, justify: new(0.5f, 0.5f));
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Bumper", "bigSpinner");
    }
}
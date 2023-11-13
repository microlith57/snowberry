using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("tentacles")]
public class Plugin_Tentacles : Entity {
    public override int MaxNodes => -1;

    public override void Render() {
        base.Render();

        MTexture icon = GFX.Game["plugins/Snowberry/tentacles"];
        icon.DrawCentered(Position);

        Vector2 prev = Position;
        foreach (Vector2 node in Nodes) {
            icon.DrawCentered(node);
            DrawUtil.DottedLine(prev, node, Color.Red * 0.5f, 8, 4);
            prev = node;
        }
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(24), justify: new(0.5f));
        foreach (var node in Nodes)
            yield return RectOnAbsolute(new(24), position: node, justify: new(0.5f));
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Tentacles", "tentacles");
    }
}
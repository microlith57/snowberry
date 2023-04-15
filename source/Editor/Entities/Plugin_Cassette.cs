using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("cassette")]
public class Plugin_Cassette : Entity {
    public override int MinNodes => 2;
    public override int MaxNodes => 2;

    public override void Render() {
        base.Render();
        var sprite = FromSprite("cassette", "idle");
        sprite?.DrawCentered(Position);
        foreach (var node in Nodes) {
            sprite?.DrawCentered(node, Color.White * 0.5f);
        }
    }

    public override void HQRender() {
        base.HQRender();
        new SimpleCurve(Position, Nodes[1], Nodes[0]).Render(Color.DarkCyan * 0.75f, 32, 2);
    }

    protected override IEnumerable<Rectangle> Select() {
        var sprite = FromSprite("cassette", "idle");
        yield return RectOnRelative(sprite, justify: new(0.5f));
        foreach (var node in Nodes)
            yield return RectOnAbsolute(sprite, position: node, justify: new(0.5f));
    }

    public static void AddPlacements() {
        Placements.Create("Cassette", "cassette");
    }
}
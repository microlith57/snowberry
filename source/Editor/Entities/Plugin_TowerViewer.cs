using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("towerviewer")]
public class Plugin_Watchtower : Entity{
    [Option("onlyY")] public bool OnlyY = false;
    [Option("summit")] public bool Summit = false;

    public override int MaxNodes => -1;

    public override void Render(){
        base.Render();

        MTexture tower = FromSprite("lookout", "idle");
        tower?.DrawJustified(Position, new Vector2(0.5f, 1.0f));

        Vector2 prev = Position;
        foreach (Vector2 node in Nodes) {
            tower?.DrawJustified(node, new Vector2(0.5f, 1.0f));
            Draw.Line(prev, node, Color.White * 0.5f);
            prev = node;
        }
    }

    protected override IEnumerable<Rectangle> Select(){
        yield return RectOnRelative(new(13, 16), position: new(-1, 0), justify: new(0.5f, 1));
    }

    public static void AddPlacements(){
        Placements.Create("Watchtower", "towerviewer");
    }
}
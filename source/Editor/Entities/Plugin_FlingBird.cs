using Microsoft.Xna.Framework;
using Celeste;
using System.Collections.Generic;

namespace Snowberry.Editor.Entities; 

// So I am the greatest coder alive, which is why my enemies (the compiler) thwarted me when I tried to make birds showing what bird is next in the sequence with a dotted line work.
// Relevant code is commented out below. If you feel like taking a look, please do i will love you forever
[Plugin("flingBird")]
public class Plugin_FlingBird : Entity {
    [Option("waiting")] public bool Waiting = false;

    public override int MaxNodes => -1;
    public bool Selected = false;

    public Plugin_FlingBird() {
        Tracked = true;
    }

    public override void Render() {
        base.Render();

        GFX.Game["characters/bird/hover04"].DrawCentered(Position);

        if (Selected) {
            foreach (var node in Nodes) {
                GFX.Game["characters/bird/hover04"].DrawCentered(Position, Color.White * 0.5f);
            }
        }
    }

    public override void HQRender() {
        base.HQRender();

        Vector2 prev = Position;
        if (Nodes.Count != 0) { 
            foreach (Vector2 node in Nodes) {
                DrawUtil.DottedLine(prev, node, Color.White * 0.5f, 8, 4);
                prev = node;
            }
        }
        /**
        // Draw a line from either the bird's position or the last node to the next bird in the sequence.
        if (Room.TrackedEntities[typeof(Plugin_FlingBird)].Count > 1) {
            Vector2 startPos = (Nodes.Count == 0) ? Position : Nodes[Nodes.Count - 1];
            DrawUtil.DottedLine(startPos, NextBirdPos(), Color.White, 8, 4);
        } **/
    }

    protected override IEnumerable<Rectangle> Select() {
        Selected = true;
        yield return RectOnRelative(new(18, 12), position: new(-1f, 2.5f), justify: new(0.5f, 0.5f));
        foreach (var node in Nodes)
            yield return RectOnAbsolute(new(18, 12), position: node, justify: new(0.5f, 0.5f));
    }

    public static void AddPlacements() {
        Placements.Create("Fling Bird", "flingBird");
    }

    /**
    // Calculate the position of the bird closest to the right of the current bird.
    protected Vector2 NextBirdPos() {
        if (Room == null || Room.TrackedEntities[typeof(Plugin_FlingBird)].Count == 0) return Position;
        float? mindx = null;
        Vector2 closestPos = new Vector2(-1, -1);

        for (int i = 0; i < Room.TrackedEntities[typeof(Plugin_FlingBird)].Count; i++) {
            float dx = Room.TrackedEntities[typeof(Plugin_FlingBird)][i].Position.X - this.Position.X;
            if (mindx == null || (dx < mindx && dx > 0)) {
                closestPos = Room.TrackedEntities[typeof(Plugin_FlingBird)][i].Position;
                mindx = dx;
            }
        }

        if (mindx > 0) {
            return closestPos;
        } else return Position;
    } **/
}
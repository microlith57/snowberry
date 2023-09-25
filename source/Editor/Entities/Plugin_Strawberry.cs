using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("strawberry")]
public class Plugin_Strawberry : Entity {
    [Option("winged")] public bool Winged = false;
    [Option("moon")] public bool Moon = false;
    [Option("order")] public int Order = -1;
    [Option("checkpointID")] public int CheckpointID = -1;

    public override int MaxNodes => -1;

    public override void Render() {
        base.Render();

        GetTexture()?.DrawCentered(Position);

        foreach (Vector2 node in Nodes)
            FromSprite("strawberrySeed", "idle")?.DrawCentered(node);
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(10, 13), justify: new(.5f));
        foreach (Vector2 node in Nodes)
            yield return RectOnAbsolute(new(7, 10), position: node, justify: new(.5f));
    }

    private MTexture GetTexture() {
        bool seeded = Nodes.Count != 0;
        if (Moon) {
            string anim = seeded || Winged ? "moonghostberry" : "moonberry";
            return FromSprite(anim, "idle");
        } else {
            string dir = seeded ? "ghostberry" : "strawberry";
            string anim = Winged ? "flap" : "idle";
            return FromSprite(dir, anim);
        }
    }

    public static void AddPlacements() {
        Placements.Create("Strawberry", "strawberry");
        Placements.Create("Strawberry (Winged)", "strawberry", new Dictionary<string, object>() { { "winged", true } });
        Placements.Create("Moon Berry", "strawberry", new Dictionary<string, object>() { { "moon", true } });
    }
}
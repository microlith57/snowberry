using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("fireBall")]
public class Plugin_FireBall : Entity {
    [Option("amount")] public int Amount = 3;
    [Option("offset")] public float Offset = 0.0f;
    [Option("speed")] public float Speed = 1.0f;
    [Option("notCoreMode")] public bool NotCoreMode = false;

    public override int MinNodes => 1;
    public override int MaxNodes => -1;

    public override void Render() {
        base.Render();

        Vector2 start = Position;
        Vector2? end = Nodes.Count > 0 ? Nodes[0] : null;

        MTexture orb = FromSprite("fireball", NotCoreMode ? "ice" : "hot");

        if (end == null || Amount == 0 || start == end) {
            orb?.DrawCentered(Position);
        } else {
            Draw.Line(start, end.Value, Color.Teal);
            Vector2 d = end.Value - start;
            float step = 1f / Amount;
            for (float f = 0f; f < 1f; f += step)
                orb?.DrawCentered(Position + d * ((f + Offset) % 1f));
        }
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Fireball", "fireBall");
    }
}
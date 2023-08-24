using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities;

[Plugin("eyebomb")]
public class Plugin_Puffer : Entity {
    [Option("right")] public bool Right;

    public override void Render() {
        base.Render();

        FromSprite("pufferFish", "idle")?.DrawCentered(Position, Color.White, new Vector2(Right ? 1 : -1, 1));
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(16, 13), position: new Vector2(Right ? -1 : 1, 0), justify: new(0.5f));
    }

    public static void AddPlacements() {
        Placements.Create("Puffer", "eyebomb");
    }
}
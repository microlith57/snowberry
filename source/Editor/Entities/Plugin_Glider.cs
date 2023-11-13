using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Snowberry.Editor.Entities;

[Plugin("glider")]
public class Plugin_Glider : Entity {
    [Option("bubble")] public bool Bubble = false;
    [Option("tutorial")] public bool Tutorial = false;

    public override void Render() {
        base.Render();
        FromSprite("glider", "idle")?.DrawCentered(Position);
        if (Bubble) {
            for (int i = 0; i < 24; i++) {
                Draw.Point(Position + PlatformAdd(i), PlatformColor(i));
            }
        }
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(28, 17), position: new(1, 3), justify: new(0.5f, 1));
    }

    private Vector2 PlatformAdd(int num) {
        return new Vector2(-12 + num, -5 + (int)Math.Round(Math.Sin(3 + num * 0.2f) * 1.8));
    }

    private Color PlatformColor(int num) {
        if (num <= 1 || num >= 22) {
            return Color.White * 0.4f;
        }

        return Color.White * 0.8f;
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Jellyfish", "glider");
    }
}
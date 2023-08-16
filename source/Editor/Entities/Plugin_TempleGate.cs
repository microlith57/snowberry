using System;
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("templeGate")]
public class Plugin_TempleGate : Entity {

    [Option("type")] public TempleGate.Types Type = TempleGate.Types.NearestSwitch;
    [Option("sprite")] public string Sprite = "default";

    public override int MinHeight => 16;

    public override void Render() {
        base.Render();

        Draw.Rect(X - 2, Y - 8, 14, 10, Color.Black);
        MTexture sprite = FromSprite($"templegate_{Sprite}", "idle");
        sprite?.GetSubtexture(new Rectangle(0, sprite.Height - Height, sprite.Width, Height))
               .DrawJustified(Position + new Vector2(4, 0), new(0.5f, 0));
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(17, Height), position: new(5, 0), justify: new(0.5f, 0));
    }

    public static void AddPlacements() {
        Placements.Create("Temple Gate", "templeGate");
    }
}
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("dreammirror")]
public class Plugin_DreamMirror : Entity {
    private MTexture frame, mirror;

    public override void Initialize() {
        base.Initialize();
        mirror = GFX.Game["objects/mirror/glassbreak00"];
        frame = GFX.Game["objects/mirror/frame"];
    }

    public override void Render() {
        base.Render();
        mirror.DrawJustified(Position, new Vector2(0.5f, 1.0f));
        frame.DrawJustified(Position, new Vector2(0.5f, 1.0f));
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(62, 37), justify: new(0.5f, 1));
    }

    public static void AddPlacements() {
        Placements.Create("Dream Mirror", "dreammirror");
    }
}
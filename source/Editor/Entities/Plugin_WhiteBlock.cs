using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("whiteblock")]
public class Plugin_WhiteBlock : Entity {
    private MTexture sprite;

    public override void Initialize() {
        base.Initialize();
        sprite = GFX.Game["objects/whiteblock"];
    }

    public override void Render() {
        base.Render();
        sprite.Draw(Position);
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(48, 24));
    }

    public static void AddPlacements() {
        Placements.Create("White Block", "whiteblock");
    }
}
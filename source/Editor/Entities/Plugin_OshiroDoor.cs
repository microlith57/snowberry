using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities;

[Plugin("oshirodoor")]
public class Plugin_OshiroDoor : Entity {

    public override void Render() {
        base.Render();

        FromSprite("ghost_door", "idle").DrawCentered(Position + new Vector2(16, 16));
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(32));
    }
}
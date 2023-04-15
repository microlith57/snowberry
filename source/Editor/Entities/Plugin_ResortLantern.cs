using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("resortLantern")]
public class Plugin_ResortLantern : Entity {

    public override void Render() {
        base.Render();

        bool flipped = Room.GetFgTile(Position + Vector2.UnitX * 8f) is not '0' and not ' ';
        MTexture holder = GFX.Game["objects/resortLantern/holder"], lantern = GFX.Game["objects/resortLantern/lantern00"];
        var scale = new Vector2(flipped ? -1 : 1, 1);
        holder.Draw(Position, holder.Center, Color.White, scale);
        // can't use FromSprite since the sprite is made in code
        lantern.Draw(Position + new Vector2(-1 + flipped.Bit() * 2, -5), new Vector2(7, 7), Color.White, scale);
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(12, 16), position: new(0, -5), justify: new(0.5f, 0));
    }

    public static void AddPlacements() {
        Placements.Create("Resort Lantern", "resortLantern");
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("waterfall")]
public class Plugin_Waterfall : Entity {

    public override void Render() {
        base.Render();

        int height = 0;
        if(Room != null) {
            int maxHeight = Room.Height - Y / 8;
            for (int i = 0; i < maxHeight; i++) {
                height = i;

                Vector2 point = Position + new Vector2(4, 4 + i * 8);
                if (Room.GetFgTile(point) is not ' ' and not '0')
                    break;
                if (Room.TrackedEntities.TryGetValue(typeof(Plugin_Water), out var waters)
                    && waters.Any(w => w.Bounds.Contains((int)point.X, (int)point.Y)))
                    break;
            }
        } else
            height = 10;

        height = Math.Max(height, 1);

        Draw.Rect(X + 1f, Y, 6f, height * 8, Plugin_Water.WaterColour * 0.3f);
        Draw.Rect(X - 1f, Y, 2f, height * 8, Plugin_Water.WaterColour * 0.8f);
        Draw.Rect(X + 7f, Y, 2f, height * 8, Plugin_Water.WaterColour * 0.8f);
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(8, 16));
    }

    public static void AddPlacements() {
        Placements.Create("Waterfall", "waterfall");
    }
}
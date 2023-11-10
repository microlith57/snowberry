using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("introCar")]
public class Plugin_IntroCar : Entity {

    [Option("hasRoadAndBarriers")] public bool HasRoadAndBarriers;

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(47, 18), position: new(1, 0), justify: new(0.5f, 1));
    }

    public override void Render() {
        base.Render();

        GFX.Game["scenery/car/wheels"].DrawJustified(Position, new(0.5f, 1));
        GFX.Game["scenery/car/body"].DrawJustified(Position, new(0.5f, 1));

        if (HasRoadAndBarriers) {
            GFX.Game["scenery/car/barrier"].DrawJustified(Position + new Vector2(32, 0), new(0, 1));
            GFX.Game["scenery/car/barrier"].DrawJustified(Position + new Vector2(41, 0), new(0, 1), Color.DarkGray);

            if (Room != null) {
                Vector2 basePos = new Vector2(Room.X * 8, Y);
                int columns = (X - Room.X * 8 - 48) / 8;
                for (int idx = 0; idx < columns; ++idx) {
                    int num = idx >= columns - 2 ? (idx != columns - 2 ? 3 : 2) : Calc.Random.Next(0, 2);
                    GFX.Game["scenery/car/pavement"].GetSubtexture(num * 8, 0, 8, 8).Draw(basePos + new Vector2(idx * 8, 0));
                }
            }
        }
    }

    public static void AddPlacements() {
        Placements.Create("Intro Car", "introCar");
    }
}
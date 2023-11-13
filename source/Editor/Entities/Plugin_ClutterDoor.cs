using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using static Celeste.ClutterBlock;

namespace Snowberry.Editor.Entities;

[Plugin("clutterDoor")]
public class Plugin_ClutterDoor : Entity {

    [Option("type")] public Colors Colour = Colors.Green;

    public override void Render() {
        base.Render();

        FromSprite("ghost_door", "idle").DrawCentered(Position + new Vector2(16, 16));
        GFX.Game["objects/resortclutter/icon_" + Colour].DrawCentered(Position + new Vector2(16, 16));
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(32));
    }

    public static void AddPlacements() {
        foreach (var colour in new[]{ Colors.Red, Colors.Green, Colors.Yellow, Colors.Lightning })
            Placements.EntityPlacementProvider.Create($"Clutter Door ({colour})", "clutterDoor", new() { ["type"] = colour.ToString() });
    }
}
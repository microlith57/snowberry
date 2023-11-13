using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities;

[Plugin("templeEye")]
public class Plugin_TempleEye : Entity {

    // TODO: change depth (unimplemented) to render on top of FG tiles when present

    public override void Render() {
        base.Render();

        string pfix = (Room == null || Room.GetFgTile(Position) != '0') ? "fg" : "bg";
        GFX.Game[$"scenery/temple/eye/{pfix}_eye"].DrawCentered(Position);
        GFX.Game[$"scenery/temple/eye/{pfix}_pupil"].DrawCentered(Position);
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(12, 13), justify: new(0.5f));
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Temple Eye", "templeEye");
    }
}
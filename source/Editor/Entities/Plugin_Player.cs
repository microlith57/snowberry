using System.Collections.Generic;
using System.Linq;
using Celeste;
using Microsoft.Xna.Framework;
using Snowberry.Editor.Triggers;

namespace Snowberry.Editor.Entities;

[Plugin("player")]
public class Plugin_Player : Entity{

    public Plugin_Player() {
        Tracked = true;
    }

    public override void Render(){
        base.Render();
        Facings? facing = GetSpawnFacing();
        FromSprite("player", "sitDown")?.DrawCentered(Position + new Vector2(facing == Facings.Left ? -3 : 0, -16), Color.White, new Vector2(facing == Facings.Left ? -1 : 1, 1));
    }

    protected override IEnumerable<Rectangle> Select(){
        yield return RectOnRelative(new(13, 17), position: new(-2, 0), justify: new(0.5f, 1));
    }

    public static void AddPlacements(){
        Placements.EntityPlacementProvider.Create("Spawn Point", "player");
    }

    protected Facings? GetSpawnFacing(){
        return Room != null && Room.TrackedEntities.ContainsKey(typeof(Plugin_SpawnFacingTrigger))
                ? Room.TrackedEntities[typeof(Plugin_SpawnFacingTrigger)]
                    .OfType<Plugin_SpawnFacingTrigger>()
                    .FirstOrDefault(e => e.Bounds.Intersects(Select().First()))?.Facing
                : null;
    }
}
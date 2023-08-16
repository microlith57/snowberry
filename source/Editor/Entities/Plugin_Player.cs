using System.Collections.Generic;
using System.Linq;
using Celeste;
using Microsoft.Xna.Framework;
using Snowberry.Editor.Triggers;

namespace Snowberry.Editor.Entities;

[Plugin("player")]
public class Plugin_Player : Entity{

    public override void Render(){
        base.Render();
        int xScale = GetSpawnFacing() == Facings.Left ? -1 : 1;
        FromSprite("player", "sitDown")?.DrawCentered(Position - Vector2.UnitY * 16, Color.White, new Vector2(xScale, 1));
    }

    protected override IEnumerable<Rectangle> Select(){
        yield return RectOnRelative(new(13, 17), position: new(-1, 0), justify: new(0.5f, 1));
    }

    public static void AddPlacements(){
        Placements.Create("Spawn Point", "player");
    }

    protected Facings? GetSpawnFacing(){
        return Room != null && Room.TrackedEntities.ContainsKey(typeof(Plugin_SpawnFacingTrigger))
                ? Room.TrackedEntities[typeof(Plugin_SpawnFacingTrigger)]
                    .OfType<Plugin_SpawnFacingTrigger>()
                    .FirstOrDefault(e => e.Bounds.Intersects(Select().First()))?.Facing
                : null;
    }
}
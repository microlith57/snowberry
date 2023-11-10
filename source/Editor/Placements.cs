using System.Collections.Generic;
using Snowberry.Editor.Tools;

namespace Snowberry.Editor;

public static class Placements {

    public interface Placement {

        public string Name { get; }
        public string ModName { get; }

        public Placeable Build(Room room);
    }

    public class EntityPlacement : Placement {

        public string Name { get; }
        public string ModName { get; }

        public readonly string EntityName;

        public readonly Dictionary<string, object> Defaults;

        public readonly bool IsTrigger;

        public EntityPlacement(string name, string entityName, Dictionary<string, object> defaults, bool isTrigger) {
            Name = name;
            EntityName = entityName;
            Defaults = defaults;
            IsTrigger = isTrigger;

            var split = name.Split('/');
            ModName = split.Length > 1 ? split[0] : "Celeste";
        }

        public Placeable /* Entity */ Build(Room room) {
            Entity e = Entity.Create(EntityName, room);
            foreach (var item in Defaults)
                e.Set(item.Key, item.Value);
            return e;
        }
    }

    public class DecalPlacement : Placement {

        public string Name { get; }
        public string ModName { get; }
        public string DecalPath { get; }

        public DecalPlacement(string name, string modName, string decalPath) {
            Name = name;
            ModName = modName;
            DecalPath = decalPath;
        }

        public Placeable Build(Room room) => new Decal(room, DecalPath) {
            Fg = PlacementTool.DecalsAreFg
        };
    }

    public static readonly List<Placement> All = new();

    public static void Create(string placementName, string entityName, Dictionary<string, object> defaults = null, bool trigger = false) {
        All.Add(new EntityPlacement(placementName, entityName, defaults ?? new(), trigger));
    }
}
using System.Collections.Generic;

namespace Snowberry.Editor;

public static class Placements {

    public class Placement {
        public readonly string Name, EntityName;

        public readonly Dictionary<string, object> Defaults;

        public readonly bool IsTrigger;

        public Placement(string name, string entityName, Dictionary<string, object> defaults, bool isTrigger) {
            Name = name;
            EntityName = entityName;
            Defaults = defaults;
            IsTrigger = isTrigger;
        }

        public Entity Build(Room room) {
            Entity e = Entity.Create(EntityName, room);
            foreach (var item in Defaults)
                e.Set(item.Key, item.Value);
            return e;
        }
    }

    public static readonly List<Placement> All = new();

    public static void Create(string placementName, string entityName, Dictionary<string, object> defaults = null, bool trigger = false) {
        All.Add(new Placement(placementName, entityName, defaults ?? new Dictionary<string, object>(), trigger));
    }
}
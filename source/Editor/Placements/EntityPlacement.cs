using System.Collections.Generic;
using Snowberry.UI.Layout;

namespace Snowberry.Editor.Placements;

public class EntityPlacementProvider : PlacementProvider {

    public static readonly List<EntityPlacement> All = new();

    public IEnumerable<Placement> Placements() => All;

    public UITree BuildTree() {
        return null;
    }

    public static void Create(string placementName, string entityName, Dictionary<string, object> defaults = null, bool trigger = false)
        => All.Add(new EntityPlacement(placementName, entityName, defaults ?? new(), trigger));
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
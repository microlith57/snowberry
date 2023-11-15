using System.Collections.Generic;
using System.Linq;
using Snowberry.Editor.Tools;
using Snowberry.UI;
using Snowberry.UI.Layout;

namespace Snowberry.Editor.Placements;

public class EntityPlacementProvider : PlacementProvider {

    public static readonly List<EntityPlacement> All = new();

    public IEnumerable<Placement> Placements() => All;

    public IEnumerable<UITree> BuildTree(int width) {
        yield return CreateEntitiesTree(width);
        yield return CreateEntitiesTree(width, true);
    }

    private static UITree CreateEntitiesTree(int width, bool triggers = false) {
        UITree entities = new(new UILabel(triggers ? "triggers" : "entities")) {
            NoKb = true,
            PadUp = 2,
            PadDown = 2
        };
        foreach (var group in EntityPlacementProvider.All.Where(x => x.IsTrigger == triggers).OrderBy(x => x.Name).GroupBy(x => x.EntityName)) {
            if (group.Count() == 1)
                entities.Add(PlacementTool.CreatePlacementButton(group.First(), width - entities.PadLeft));
            else {
                UITree subtree = new UITree(PlacementTool.CreatePlacementButton(group.First(), width - entities.PadLeft * 2 - 20), new(), new(5, 2), collapsed: true) {
                    PadUp = 2,
                    PadDown = 2
                };
                foreach (EntityPlacement p in group.Skip(1))
                    subtree.Add(PlacementTool.CreatePlacementButton(p, width - entities.PadLeft * 2));
                subtree.Layout();
                entities.Add(subtree);
            }
        }
        entities.Layout();
        return entities;
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

        var split = entityName.Split('/');
        ModName = split.Length > 1 ? split[0] : "Celeste";
    }

    public Placeable /* Entity */ Build(Room room) {
        Entity e = Entity.Create(EntityName, room);
        foreach (var item in Defaults)
            e.Set(item.Key, item.Value);
        return e;
    }
}
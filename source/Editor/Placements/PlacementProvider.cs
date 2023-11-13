using System.Collections.Generic;
using Snowberry.UI.Layout;

namespace Snowberry.Editor.Placements;

public interface PlacementProvider {

    public IEnumerable<Placement> Placements();

    // where all placement buttons must have their corresponding placement as their tag
    public UITree BuildTree();
}

public static class PlacementProviders {

    public static readonly List<PlacementProvider> All = new() {
        new EntityPlacementProvider(),
        new DecalPlacementProvider()
    };
}
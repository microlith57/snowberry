using System.Collections.Generic;
using Snowberry.UI.Layout;

namespace Snowberry.Editor.Placements;

public interface PlacementProvider {

    public static readonly List<PlacementProvider> All = [
        new EntityPlacementProvider(),
        new DecalPlacementProvider()
    ];

    public IEnumerable<Placement> Placements();

    // where all placement buttons must have their corresponding placement as their tag
    public IEnumerable<UITree> BuildTree(int width);
}
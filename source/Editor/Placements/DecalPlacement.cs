using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Snowberry.Editor.Tools;
using Snowberry.UI;
using Snowberry.UI.Layout;

namespace Snowberry.Editor.Placements;

public class DecalPlacementProvider : PlacementProvider {

    private static Tree<string> Spine = null;

    public static void Reload() {
        List<List<string>> decalPaths = GFX.Game.Textures.Keys
            .Where(x => x.StartsWith("decals/", StringComparison.Ordinal))
            .Select(x => Decal.Sanitize(x, true))
            .Select(x => x.Split('/').ToList())
            .ToList();
        Spine = Tree<string>.FromPrefixes(decalPaths, "").Children[0];
        Spine.Parent = null; // get rid of the empty parent for AggregateUp
    }

    public IEnumerable<Placement> Placements() {
        return null;
    }

    public UITree BuildTree() {
        List<List<string>> decalPaths = GFX.Game.Textures.Keys
            .Where(x => x.StartsWith("decals/", StringComparison.Ordinal))
            .Select(x => Decal.Sanitize(x, true))
            .Select(x => x.Split('/').ToList())
            .ToList();
        Tree<string> decalTree = Tree<string>.FromPrefixes(decalPaths, "").Children[0];
        decalTree.Parent = null; // get rid of the empty parent for AggregateUp

        UITree RenderPart(Tree<string> part, float maxWidth){
            UITree tree = new(new UILabel(part.Value + "/", Fonts.Regular)){
                NoKb = true,
                PadUp = 2,
                PadDown = 2
            };
            foreach(Tree<string> c in part.Children.OrderBy(x => x.Value))
                if(c.Children.Any())
                    tree.Add(RenderPart(c, maxWidth - tree.PadLeft));
                else {
                    UIElement group = new();
                    string path = c.AggregateUp((s, s1) => s + "/" + s1);
                    Placement fake = new DecalPlacement(c.Value, "weh", path.Substring("decals/".Length));
                    group.Add(/*CreatePlacementButton(fake, maxWidth)*/ null);
                    group.AddRight(new UIImage(GFX.Game.GetAtlasSubtextures(path)[0]).ScaleToFit(new(24, 24)), new(3, 0));
                    group.CalculateBounds();
                    tree.Add(group);
                }

            tree.Layout();
            return tree;
        }

        return RenderPart(decalTree, /*width*/ -1);
    }
}

public class DecalPlacement(string name, string modName, string decalPath) : Placement {

    public string Name { get; } = name;
    public string ModName { get; } = modName;
    public string DecalPath { get; } = decalPath;

    public Placeable Build(Room room) => new Decal(room, DecalPath) {
        Fg = PlacementTool.DecalsAreFg
    };
}
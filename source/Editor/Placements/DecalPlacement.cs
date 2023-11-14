using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Monocle;
using Snowberry.Editor.Tools;
using Snowberry.UI;
using Snowberry.UI.Layout;

namespace Snowberry.Editor.Placements;

public class DecalPlacementProvider : PlacementProvider {

    private static Tree<string> Spine = null;
    private static Dictionary<string, Placement> All = new();

    public static void Reload() {
        List<(string mod, string path)> decalPaths = new(GFX.Game.Textures.Count);

        foreach ((string path, MTexture tex) in GFX.Game.Textures)
            if (path.StartsWith("decals/", StringComparison.Ordinal))
                decalPaths.Add((mod: tex.Metadata?.Source?.Name ?? "Celeste", path: Decal.Sanitize(path, true)));

        List<List<string>> splitPaths = decalPaths
            .Select(x => x.path.Split('/').ToList())
            .ToList();
        Spine = Tree<string>.FromPrefixes(splitPaths, "").Children[0];
        Spine.Parent = null; // get rid of the empty parent for AggregateUp

        foreach ((string mod, string path) in decalPaths)
            All[path] = new DecalPlacement(path.Split('/')[^1], mod, path["decals/".Length..]);
    }

    public IEnumerable<Placement> Placements() => All.Values;

    public IEnumerable<UITree> BuildTree(int width) {
        Tree<string> decalTree = Spine;

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
                    group.Add(new UIImage(GFX.Game.GetAtlasSubtextures(path)[0]).ScaleToFit(new(24, 24)));
                    group.AddRight(PlacementTool.CreatePlacementButton(All[path], maxWidth), new(3, 0));
                    group.CalculateBounds();
                    tree.Add(group);
                }

            tree.Layout();
            return tree;
        }

        yield return RenderPart(decalTree, width);
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
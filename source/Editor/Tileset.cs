using System.Collections.Generic;
using System.Linq;
using Celeste;
using Monocle;
using MonoMod.Utils;

namespace Snowberry.Editor;

public class Tileset {

    public char Key;
    public string Path;
    public bool Bg;

    public TileGrid Tile, Square;

    public string Name => Path.Split('/').Last();

    public Tileset(char key, string path, bool bg) {
        Key = key;
        Path = path;
        Bg = bg;
        Autotiler autotiler = Bg ? GFX.BGAutotiler : GFX.FGAutotiler;
        Tile = autotiler.GenerateBox(Key, 1, 1).TileGrid;
        Square = autotiler.GenerateBox(Key, 3, 3).TileGrid;
    }

    public static List<Tileset> GetTilesets(bool bg) {
        // todo: cleanup?
        // TODO: cache
        DynamicData autotilerData = new DynamicData(typeof(Autotiler), bg ? GFX.BGAutotiler : GFX.FGAutotiler);
        DynamicData lookupData = new DynamicData(autotilerData.Get("lookup"));
        ICollection<char> keys = (ICollection<char>)lookupData.Get("Keys");
        System.Collections.IEnumerable entries = (System.Collections.IEnumerable)lookupData.Get("Values");
        var paths = new List<string>();
        var chars = keys.ToList();
        int i = 0;
        foreach (var item in entries) {
            var itemData = new DynamicData(item);
            var tilesData = new DynamicData(itemData.Get("Center"));
            string path = GFX.Game.Textures.FirstOrDefault(t => t.Value.Equals(tilesData.Get<List<MTexture>>("Textures")[0].GetParent())).Key ?? "Tileset of " + chars[i];
            paths.Add(path);
            i++;
        }

        List<Tileset> ret = new List<Tileset> {
            // not a "real" tileset
            new('0', "air", bg)
        };
        ret.AddRange(chars.Select((item, i1) => new Tileset(item, paths[i1], bg)));

        return ret;
    }

    public static Tileset ByKey(char key, bool bg) {
        return GetTilesets(bg).FirstOrDefault(ts => ts.Key == key) ?? new('0', "air", bg);
    }
}
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using NLua;

namespace Snowberry.Editor.Entities;

public class LuaEntity : Entity {
    private readonly LuaTable plugin;
    private bool initialized = false;

    // updated on modification
    Color? color, fillColor, borderColor;
    private string texture;
    private List<SpriteWithPos> sprites;
    private Vector2 justify = Vector2.One * 0.5f;

    public Dictionary<string, object> Values = new();

    public LuaEntity(string name, LoennPluginInfo info, LuaTable plugin, bool isTrigger) {
        Name = name;
        Info = info;
        this.plugin = plugin;
        IsTrigger = isTrigger;
    }

    public override bool IsTrigger { get; }

    public override void Render() {
        base.Render();

        if (!initialized || Dirty) {
            color = CallOrGet<LuaTable>("color") is LuaTable c ? TableColor(c) : null;
            fillColor = CallOrGet<LuaTable>("fillColor") is LuaTable f ? TableColor(f) : null;
            borderColor = CallOrGet<LuaTable>("borderColor") is LuaTable b ? TableColor(b) : null;

            texture = CallOrGet<string>("texture");

            var justifyTable = CallOrGet<LuaTable>("justify");
            if (justifyTable != null)
                justify = new Vector2(Float(justifyTable, 1, 0.5f), Float(justifyTable, 2, 0.5f));

            sprites = Sprites();

            initialized = true;
        }

        if(texture != null)
            GFX.Game[texture].DrawJustified(Center, justify);

        if(fillColor is Color fill) {
            Draw.Rect(Position, Width, Height, fill);
            if(borderColor is Color border)
                Draw.HollowRect(Position, Width, Height, border);
        } else if(color is Color c) {
            Draw.Rect(Position, Width, Height, c);
        }

        foreach(var sprite in sprites)
            sprite.texture.DrawJustified(Center + sprite.pos, justify, sprite.color, sprite.scale);
    }

    private List<SpriteWithPos> Sprites() {
        List<SpriteWithPos> ret = new();
        if(CallOrGetAll("sprite") is object[] sprites)
            foreach(var item in sprites) {
                if(item is LuaTable sprite) {
                    foreach(var k in sprite.Keys) {
                        if(sprite[k] is LuaTable sp && sp["meta"] is LuaTable meta && meta["image"] is string image && meta["atlas"] is string atlasName) {
                            Atlas atlas = atlasName.ToLowerInvariant().Equals("gui") ? GFX.Gui : atlasName.ToLowerInvariant().Equals("misc") ? GFX.Misc : GFX.Game;
                            MTexture tex = atlas[image];
                            int x = X, y = Y;
                            float sX = Float(sp, "scaleX"), sY = Float(sp, "scaleY");
                            Color color = Color.White;

                            if(sp["x"] is int spX) { x = spX; }
                            if(sp["y"] is int spY) { x = spY; }
                            if(sp["color"] is LuaTable ct) { color = TableColor(ct); }

                            ret.Add(new SpriteWithPos(tex, new Vector2(x, y) - Center, new Vector2(sX, sY), color));
                            sp.Dispose();
                        }
                    }
                    sprite.Dispose();
                }
            }
        return ret;
    }

    private T CallOrGet<T>(string name, T orElse = default) where T : class {
        return CallOrGetAll(name, orElse).FirstOrDefault() as T;
    }

    private object[] CallOrGetAll(string name, object orElse = default) {
        using LuaTable entity = WrapEntity();
        using LuaTable empty = EmptyTable();
        if (entity == null)
            return new[] { orElse };
        switch (plugin[name]) {
            case LuaFunction f:
                try {
                    return f.Call(empty, entity, empty) ?? new[] { orElse };
                } catch {
                    return new[] { orElse };
                }
            case object s:
                return new[] { s };
            default:
                return new[] { orElse };
        }
    }

    private static LuaTable EmptyTable() {
        Everest.LuaLoader.Context.NewTable("snowberry_it");
        return Everest.LuaLoader.Context.DoString("return snowberry_it").FirstOrDefault() as LuaTable;
    }

    private static LuaTable WrapTable(IDictionary<string, object> dict) {
        var table = EmptyTable();
        if (table != null)
            foreach (var pair in dict)
                table[pair.Key] = pair.Value;
        return table;
    }

    private LuaTable WrapEntity() {
        LuaTable table = WrapTable(Values);

        if (table != null) {
            table["name"] = Name;
            table["width"] = Width;
            table["height"] = Height;
        }

        return table;
    }

    private static float Float<T>(LuaTable from, T index, float def = 1f) {
        if(index is int ix) // lua tables prefer longs
            return Float(from, (long)ix, def);
        if(from.Keys.OfType<T>().Any(k => k.Equals(index))) {
            object value = from[index];
            return value switch {
                float f => f,
                int i => i,
                long l => l,
                double d => (float)d,
                string s => float.Parse(s),
                _ => def
            };
        }

        return def;
    }

    private static Color TableColor(LuaTable from) {
        Color color1 = new Color(Float(from, 1), Float(from, 2), Float(from, 3)) * Float(from, 4);
        from.Dispose();
        return color1;
    }

    private record SpriteWithPos(MTexture texture, Vector2 pos, Vector2 scale, Color color) {
        // IsExternalInit my ass
        public readonly MTexture texture = texture;
        public readonly Vector2 pos = pos;
        public readonly Vector2 scale = scale;
        public readonly Color color = color;
    }
}

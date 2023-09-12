using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using NLua;

namespace Snowberry.Editor.Entities;

public class LuaEntity : Entity {
    private readonly LuaTable plugin;
    private bool initialized = false;

    private string triggerText = null;

    // updated on modification
    private Color? color, fillColor, borderColor;
    private string texture, nodeTexture;
    private List<SpriteWithPos> sprites;
    private Vector2 justify = Vector2.One * 0.5f, nodeJustify = Vector2.One * 0.5f;
    private NodeLineRenderType nodeLines = NodeLineRenderType.none;

    public Dictionary<string, object> Values = new();

    public LuaEntity(string name, LoennPluginInfo info, LuaTable plugin, bool isTrigger) {
        Name = name;
        Info = info;
        this.plugin = plugin;
        IsTrigger = isTrigger;

        if(CallOrGet<LuaTable>("nodeLimits") is LuaTable limits) {
            MinNodes = (int)Float(limits, 1, 0);
            MaxNodes = (int)Float(limits, 2, 0);
        }

        MinWidth = MinHeight = -1;

        // default minimums from properties
        if (info.HasWidth)
            MinWidth = 8;
        if (info.HasHeight)
            MinHeight = 8;

        // explicit minimum size
        if(CallOrGet<LuaTable>("minimumSize") is LuaTable minSize) {
            MinWidth = (int)Float(minSize, 1, 8);
            MinHeight = (int)Float(minSize, 2, 8);
        }

        // triggers are always resizable
        if(IsTrigger) {
            MinWidth = Math.Max(MinWidth, 8);
            MinHeight = Math.Max(MinHeight, 8);
        }
    }

    public sealed override bool IsTrigger { get; }

    // set on placement
    // TODO: should react to changes though
    public sealed override int MinWidth { get; }
    public sealed override int MinHeight { get; }
    public sealed override int MinNodes { get; }
    public sealed override int MaxNodes { get; }

    public override void Render() {
        base.Render();

        if (IsTrigger) {
            Rectangle rect = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
            Draw.Rect(rect, UnknownEntity.TriggerColor * 0.3f);
            Draw.HollowRect(rect, UnknownEntity.TriggerColor);

            triggerText ??= string.Join(" ", Regex.Split(char.ToUpper(Name[0]) + Name.Substring(1), @"(?=[A-Z])")).Trim();

            Fonts.Pico8.Draw(triggerText, new Vector2(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f), Vector2.One, Vector2.One * 0.5f, Color.Black);
            return;
        }

        if (!initialized || Dirty) {
            color = CallOrGet<LuaTable>("color") is LuaTable c ? TableColor(c) : null;
            fillColor = CallOrGet<LuaTable>("fillColor") is LuaTable f ? TableColor(f) : null;
            borderColor = CallOrGet<LuaTable>("borderColor") is LuaTable b ? TableColor(b) : null;

            texture = CallOrGet<string>("texture");
            nodeTexture = CallOrGet<string>("nodeTexture");

            var justifyTable = CallOrGet<LuaTable>("justification");
            if (justifyTable != null)
                justify = new Vector2(Float(justifyTable, 1, 0.5f), Float(justifyTable, 2, 0.5f));
            var nodeJustifyTable = CallOrGet<LuaTable>("nodeJustification");
            if (nodeJustifyTable != null)
                nodeJustify = new Vector2(Float(nodeJustifyTable, 1, 0.5f), Float(nodeJustifyTable, 2, 0.5f));

            nodeLines = Enum.TryParse<NodeLineRenderType>(CallOrGet<string>("nodeLineRenderType"), true, out var v)
                ? v
                : NodeLineRenderType.none;

            sprites = Sprites();

            initialized = true;
        }

        if(Nodes.Count > 0){
            switch (nodeLines) {
                case NodeLineRenderType.line:
                    Vector2 prev = Position;
                    foreach (Vector2 node in Nodes) {
                        DrawUtil.DottedLine(prev, node, Color.White * 0.5f, 8, 4);
                        prev = node;
                    }
                    break;
                case NodeLineRenderType.fan:
                    foreach (Vector2 node in Nodes)
                        DrawUtil.DottedLine(Position, node, Color.White * 0.5f, 8, 4);
                    break;
                case NodeLineRenderType.circle:
                    Draw.Circle(Position, Vector2.Distance(Position, Nodes[0]), Color.White * 0.5f, 20);
                    break;
                case NodeLineRenderType.none:
                default:
                    break;
            }
        }

        if (texture != null)
            GFX.Game[texture].DrawJustified(Position, justify);

        if ((nodeTexture ?? texture) != null)
            foreach (var node in Nodes)
                GFX.Game[nodeTexture ?? texture].DrawJustified(node, nodeJustify);

        if(fillColor is Color fill) {
            Draw.Rect(Position, Width, Height, fill);
            if(borderColor is Color border)
                Draw.HollowRect(Position, Width, Height, border);
        } else if(color is Color c) {
            Draw.Rect(Position, Width, Height, c);
        }

        foreach((MTexture tex, Vector2 pos, Vector2 scale, Color tint, Vector2 justification, float rotation, Vector2 texPos, Vector2 texSize) in sprites) {
            MTexture draw = tex;
            if (texPos != Vector2.Zero || texSize != -Vector2.One) {
                float tWidth = texSize.X < 0 ? tex.Width - texPos.X : texSize.X;
                float tHeight = texSize.Y < 0 ? tex.Height - texPos.Y : texSize.Y;
                draw = tex.GetSubtexture((int)texPos.X, (int)texPos.Y, (int)tWidth, (int)tHeight);
            }

            draw.DrawJustified(pos, justification, tint, scale, rotation);
        }
    }

    protected override IEnumerable<Rectangle> Select() {
        // if the entity has a custom selection function, try to use it
        List<Rectangle> ret = CallOrGetAll("selection")
            .OfType<LuaTable>()
            .Where(x => x["_type"] is "rectangle")
            .Select(t => new Rectangle(
                (int)Float(t, "x", 0),
                (int)Float(t, "y", 0),
                (int)Float(t, "width", 4),
                (int)Float(t, "height", 4)))
            .ToList();

        // fill in the gaps with selections derived from sprites
        if (ret.Count == 0 && texture != null) {
            MTexture tex = GFX.Game[texture];
            ret.Add(RectOnRelative(new(tex.Width, tex.Height), justify: justify));
        }

        if ((nodeTexture ?? texture) != null) {
            MTexture tex = GFX.Game[nodeTexture ?? texture];
            for (int i = ret.Count; i < Nodes.Count + 1; i++)
                ret.Add(RectOnAbsolute(new(tex.Width, tex.Height), position: Nodes[i - 1], justify: nodeJustify));
        }

        // fill in the rest with defaults
        if (ret.Count == 0)
            ret.Add(new(Width < 6 ? X - 3 : X, Height < 6 ? Y - 3 : Y, Width < 6 ? 6 : Width, Height < 6 ? 6 : Height));
        for (int i = ret.Count; i < Nodes.Count + 1; i++) {
            var node = Nodes[i - 1];
            ret.Add(new Rectangle((int)node.X - 3, (int)node.Y - 3, 6, 6));
        }

        return ret;
    }

    private List<SpriteWithPos> Sprites() {
        List<SpriteWithPos> ret = new();
        if(CallOrGetAll("sprite") is object[] sprites)
            foreach(var item in sprites)
                if (item is LuaTable sprite) {
                    // sprites can be returned directly
                    if (sprite["_type"] != null)
                        CreateSpriteFromDrawable(sprite, ret);
                    else
                        // ... or a table of many
                        foreach (var k in sprite.Keys)
                            if (sprite[k] is LuaTable sp)
                                CreateSpriteFromDrawable(sp, ret);
                    sprite.Dispose();
                }

        return ret;
    }

    private void CreateSpriteFromDrawable(LuaTable sp, List<SpriteWithPos> ret){
        // normalize ninepatches/lines
        if(sp["_type"] is "drawableNinePatch" or "drawableLine")
            if(sp["getDrawableSprite"] is LuaFunction h){
                sp = h.Call(sp)?.FirstOrDefault() as LuaTable;
                if(sp == null)
                    return;
                foreach(var k2 in sp.Keys)
                    CreateSpriteFromTable(sp[k2] as LuaTable, ret);
            }

        CreateSpriteFromTable(sp, ret);
    }

    private void CreateSpriteFromTable(LuaTable sp, List<SpriteWithPos> ret){
        if(sp?["meta"] is LuaTable meta && meta["image"] is string image && meta["atlas"] is string atlasName){
            Atlas atlas = atlasName.ToLowerInvariant().Equals("gui") ? GFX.Gui : atlasName.ToLowerInvariant().Equals("misc") ? GFX.Misc : GFX.Game;
            MTexture tex = atlas[image];
            Vector2 pos = new Vector2(Float(sp, "x", 0), Float(sp, "y", 0));
            Vector2 just = new Vector2(Float(sp, "justificationX", 0.5f), Float(sp, "justificationY", 0.5f));
            Vector2 scale = new Vector2(Float(sp, "scaleX"), Float(sp, "scaleY"));
            float rotation = Float(sp, "rotation", 0);
            Color spColor = Color.White;

            // offset is both position and origin
            Vector2 offset = new Vector2(Float(sp, "offsetX", 0), Float(sp, "offsetY", 0));
            pos -= offset.Rotate(rotation);

            if(sp["color"] is LuaTable ct)
                spColor = TableColor(ct);

            Vector2 texPos = new Vector2(Float(meta, "texX", 0), Float(meta, "texY", 0));
            Vector2 texSize = new Vector2(Float(meta, "texW", -1), Float(meta, "texH", -1));

            ret.Add(new SpriteWithPos(tex, pos, scale, spColor, just, rotation, texPos, texSize));
            sp.Dispose();
        }
    }

    private T CallOrGet<T>(string name, T orElse = default) where T : class {
        return CallOrGetAll(name, orElse).FirstOrDefault() as T;
    }

    private object[] CallOrGetAll(string name, object orElse = default) {
        switch (plugin[name]) {
            case LuaFunction f: {
                using LuaTable entity = WrapEntity();
                using LuaTable room = EmptyTable();
                room["tilesFg"] = EmptyTable();
                room["tilesBg"] = EmptyTable();
                room["entities"] = EmptyTable();
                room["x"] = Room?.X ?? 0;
                room["y"] = Room?.Y ?? 0;
                room["width"] = Room?.Width ?? 0;
                room["height"] = Room?.Height ?? 0;

                if (entity == null)
                    return new[] { orElse };
                try {
                    return f.Call(room, entity) ?? new[] { orElse };
                } catch (Exception e) {
                    Snowberry.LogInfo($"oh no {e}");
                    return new[] { orElse };
                }
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
            table["x"] = X;
            table["y"] = Y;
            table["nodes"] = EmptyTable();

            for (var idx = 0; idx < Nodes.Count; idx++) {
                var node = Nodes[idx];
                LuaTable nodeTable = EmptyTable();
                nodeTable["x"] = node.X;
                nodeTable["y"] = node.Y;

                ((LuaTable)table["nodes"])[idx + 1] = nodeTable;
            }
        }

        return table;
    }

    private float Float<T>(LuaTable from, T index, float def = 1f) {
        if(index is int ix) // lua tables prefer longs
            return Float(from, (long)ix, def);
        if(from.Keys.OfType<T>().Any(k => k.Equals(index))) {
            object value = from[index];
            return value switch {
                float f => f,
                int i => i,
                long l => l,
                double d => (float)d,
                string s => float.TryParse(s, out var result) ? result : throw new FormatException($"The string {s} given for key {index} by {Name} is not a float."),
                _ => def
            };
        }

        return def;
    }

    private Color TableColor(LuaTable from) {
        Color color1 = new Color(Float(from, 1), Float(from, 2), Float(from, 3)) * Float(from, 4);
        from.Dispose();
        return color1;
    }

    private record SpriteWithPos(
        MTexture Texture,
        Vector2 Pos,
        Vector2 Scale,
        Color Color,
        Vector2 Justification,
        float Rotation,
        Vector2 TexPos,
        Vector2 TexSize
    ) {
        // IsExternalInit my ass
        public readonly MTexture Texture = Texture;
        public readonly Vector2 Pos = Pos, Scale = Scale, Justification = Justification;
        public readonly Color Color = Color;
        public readonly float Rotation = Rotation;
        public readonly Vector2 TexPos = TexPos, TexSize = TexSize;
    }

    private enum NodeLineRenderType {
        line, fan, circle, none
    }
}

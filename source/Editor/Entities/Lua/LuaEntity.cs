using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using NLua;
using Snowberry.UI;

namespace Snowberry.Editor.Entities.Lua;

public class LuaEntity : Entity {
    private readonly LuaTable plugin;
    private bool initialized = false;

    private string triggerText = null;

    // updated on modification
    private Color? color, fillColor, borderColor;
    private string texture, nodeTexture;
    //private List<SpriteWithPos> sprites;
    private LuaSprites sprites;
    private Vector2 justify = Vector2.One * 0.5f, nodeJustify = Vector2.One * 0.5f;
    private NodeLineRenderType nodeLines = NodeLineRenderType.none;

    public Dictionary<string, object> Values = new();

    public LuaEntity(string name, LoennPluginInfo info, LuaTable plugin, bool isTrigger) {
        Name = name;
        Info = info;
        this.plugin = plugin;
        IsTrigger = isTrigger;
        sprites = new(Name);

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

            if (texture?.StartsWith("@Internal@/") == true)
                texture = "plugins/Snowberry/" + texture.Substring("@Internal@/".Length);
            if (nodeTexture?.StartsWith("@Internal@/") == true)
                nodeTexture = "plugins/Snowberry/" + nodeTexture.Substring("@Internal@/".Length);

            var justifyTable = CallOrGet<LuaTable>("justification");
            if (justifyTable != null)
                justify = new Vector2(Float(justifyTable, 1, 0.5f), Float(justifyTable, 2, 0.5f));
            var nodeJustifyTable = CallOrGet<LuaTable>("nodeJustification");
            if (nodeJustifyTable != null)
                nodeJustify = new Vector2(Float(nodeJustifyTable, 1, 0.5f), Float(nodeJustifyTable, 2, 0.5f));

            nodeLines = Enum.TryParse<NodeLineRenderType>(CallOrGet<string>("nodeLineRenderType"), true, out var v)
                ? v
                : NodeLineRenderType.none;

            sprites.Process(CallOrGetAll("sprite"));

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
        } else if(color is Color c)
            Draw.Rect(Position, Width, Height, c);

        sprites.Render();
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

        // fill in the rest with defaults
        if (ret.Count == 0)
            ret.Add(new(Width < 6 ? X - 3 : X, Height < 6 ? Y - 3 : Y, Width < 6 ? 6 : Width, Height < 6 ? 6 : Height));

        // note that `nodeTexture` might be non-null while `texture` itself is null
        // so we need the default main selection box to be added before the textured node selections
        if ((nodeTexture ?? texture) != null) {
            MTexture tex = GFX.Game[nodeTexture ?? texture];
            for (int i = ret.Count; i < Nodes.Count + 1; i++)
                ret.Add(RectOnAbsolute(new(tex.Width, tex.Height), position: Nodes[i - 1], justify: nodeJustify));
        }

        // and then fallbacks for nodes
        for (int i = ret.Count; i < Nodes.Count + 1; i++) {
            var node = Nodes[i - 1];
            ret.Add(new Rectangle((int)node.X - 3, (int)node.Y - 3, 6, 6));
        }

        return ret;
    }

    private T CallOrGet<T>(string name, T orElse = default) where T : class => CallOrGetAll(name, orElse).FirstOrDefault() as T;

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
                    Snowberry.Log(LogLevel.Error, $"Failed to call loenn plugin function with exception: {e}");
                    return new[] { orElse };
                }
            }
            case object s:
                return new[] { s };
            default:
                return new[] { orElse };
        }
    }

    public override (UIElement, int height)? CreateOptionUi(string optionName) {
        if (Info.Options[optionName] is LuaEntityOption { Options: { /* non-null */ } options, Editable: var editable }) {
            var value = Get(optionName);

            // it's like UIPluginOptionList but evil
            UITextField text = null;
            UIButton button = null;
            button = new UIButton(editable ? "\uF036" : options.LookupName(value, value.ToString()) + " \uF036", Fonts.Regular, 2, 2) {
                OnPress = () => {
                    var dropdown = new UIDropdown(Fonts.Regular, options
                        .Select(x => new UIDropdown.DropdownEntry(x.Key, () => {
                            Set(optionName, x.Value);
                            string displayName = options.LookupName(x.Value, x.Value.ToString());
                            if (!editable)
                                button.SetText(displayName + " \uF036");
                            else
                                text.UpdateInput(displayName);
                        })).ToArray()) {
                            Position = button.GetBoundsPos() + Vector2.UnitY * (button.Height + 2) - Editor.Instance.ToolPanel.GetBoundsPos()
                        };

                    Editor.Instance.ToolPanel.Add(dropdown);
                }
            };

            if (editable) {
                text = new UITextField(Fonts.Regular, 200);
                button.Position.X += text.Width + 3;
                return (UIElement.Regroup(text, button), 19);
            }

            return (button, 19);
        }

        return base.CreateOptionUi(optionName);
    }

    public static LuaTable EmptyTable() {
        Everest.LuaLoader.Context.NewTable("snowberry_it");
        return Everest.LuaLoader.Context.DoString("return snowberry_it").FirstOrDefault() as LuaTable;
    }

    private static LuaTable WrapTable(IDictionary<string, object> dict) {
        var table = EmptyTable();
        if (table != null)
            foreach (var pair in dict)
                table[pair.Key] = ObjectToStr(pair.Value);
        return table;
    }

    private LuaTable WrapEntity() {
        LuaTable table = WrapTable(Values.OrElse(((LoennPluginInfo)Info).Defaults));

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

    private enum NodeLineRenderType {
        line, fan, circle, none
    }
}

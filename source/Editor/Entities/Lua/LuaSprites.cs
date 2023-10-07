using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using NLua;

namespace Snowberry.Editor.Entities.Lua;

internal sealed class LuaSprites {

    private readonly string entityName; // for diagnostics
    private readonly List<Drawable> drawables = new();

    public LuaSprites(string entityName) {
        this.entityName = entityName;
    }

    public void Process(object[] sprites) {
        drawables.Clear();
        if (sprites != null)
            foreach (var item in sprites.OfType<LuaTable>().SelectMany(Normalize))
                // sprites can be returned directly
                if (item["_type"] != null)
                    drawables.Add(FromTable(item));
                else {
                    // ... or a table of many
                    foreach (var k in item.Keys)
                        if (item[k] is LuaTable sp)
                            drawables.Add(FromTable(sp));
                    item.Dispose();
                }

    }

    public void Render() {
        foreach(var d in drawables)
            d?.Draw();
    }

    private Drawable FromTable(LuaTable table) {
        string type = table["_type"] as string;
        if (type == "drawableSprite") {
            if(table["meta"] is LuaTable meta && meta["image"] is string image && meta["atlas"] is string atlasName){
                Atlas atlas = atlasName.ToLowerInvariant().Equals("gui") ? GFX.Gui : atlasName.ToLowerInvariant().Equals("misc") ? GFX.Misc : GFX.Game;
                MTexture tex = atlas[image];
                Vector2 pos = new Vector2(Float(table, "x", 0), Float(table, "y", 0));
                Vector2 just = new Vector2(Float(table, "justificationX", 0.5f), Float(table, "justificationY", 0.5f));
                Vector2 scale = new Vector2(Float(table, "scaleX"), Float(table, "scaleY"));
                float rotation = Float(table, "rotation", 0);
                Color spColor = Color.White;

                // offset is both position and origin
                Vector2 offset = new Vector2(Float(table, "offsetX", 0), Float(table, "offsetY", 0));
                pos -= offset.Rotate(rotation);

                if(table["color"] is LuaTable ct)
                    spColor = TableColor(ct);

                Vector2 texPos = new Vector2(Float(meta, "texX", 0), Float(meta, "texY", 0));
                Vector2 texSize = new Vector2(Float(meta, "texW", -1), Float(meta, "texH", -1));
                if (texSize is { X: > -1, Y: > -1 })
                    tex = tex.GetSubtexture((int)texPos.X, (int)texPos.Y, (int)texSize.X, (int)texSize.Y);

                return new Sprite {
                    Color = spColor,
                    Texture = tex,
                    Position = pos,
                    Scale = scale,
                    Justification = just,
                    Rotation = rotation
                };
            }
        } else if (type == "drawableLine") {
            List<Vector2> points = new();
            Color lnColor = Color.White;
            float thickness = Float(table, "thickness", 1);

            if (table["points"] is LuaTable pointsTbl) {
                float? x = null;
                foreach (long l in pointsTbl.Keys.OfType<long>().OrderBy(l => l)) {
                    if (x == null)
                        x = Float(pointsTbl, l);
                    else {
                        points.Add(new Vector2(x.Value, Float(pointsTbl, l)));
                        x = null;
                    }
                }
            }

            if(table["color"] is LuaTable ct)
                lnColor = TableColor(ct);

            return new Line {
                Color = lnColor,
                Points = points,
                Thickness = thickness
            };
        } else if (type == "drawableNinePatch") {
            // TODO
        } else if (type == "drawableRectangle") {
            Rectangle at = new Rectangle((int)Float(table, "x", 0), (int)Float(table, "y", 0), (int)Float(table, "width", 8), (int)Float(table, "height", 8));
            Color rectColor = Color.White, rectSecondaryColor = Color.Black;
            Rect.RectMode rectMode = Rect.RectMode.fill;

            if (table["color"] is LuaTable ct)
                rectColor = TableColor(ct);
            if (table["secondaryColor"] is LuaTable sct)
                rectColor = TableColor(sct);
            if (table["mode"] is string modeName && Enum.TryParse(modeName, true, out Rect.RectMode mode))
                rectMode = mode;

            return new Rect {
                Color = rectColor,
                Area = at,
                Mode = rectMode,
                SecondaryColor = rectSecondaryColor
            };
        } else if (type == "tileGrid") {
            Snowberry.LogInfo("got a tile grid!");
            VirtualMap<MTexture> matrix = (VirtualMap<MTexture>)table["matrix"];
            float x = Float(table, "x"), y = Float(table, "y");
            Color gridColor = Color.White;
            if (table["color"] is LuaTable ct)
                gridColor = TableColor(ct);

            return new TileGrid {
                Color = gridColor,
                Position = new(x, y),
                Tiles = matrix,
            };
        }

        Snowberry.LogInfo($"got unknown sprite type {type}");

        return null; // weh
    }

    private List<LuaTable> Normalize(LuaTable sp){
        // normalize ninepatches...
        if(sp["_type"] is "drawableNinePatch" && sp["getDrawableSprite"] is LuaFunction h && h.Call(sp)?.FirstOrDefault() is LuaTable sp2)
            return sp2.Values.OfType<LuaTable>().ToList();
        return new(){ sp };
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
                string s => float.TryParse(s, out var result) ? result : throw new FormatException($"The string {s} given for key {index} by {entityName} is not a float."),
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

    internal abstract class Drawable {
        public Color Color { protected internal get; set; }

        protected internal abstract void Draw();
    }

    internal sealed class Line : Drawable {
        public List<Vector2> Points;
        public float Thickness;

        protected internal override void Draw() {
            if (Points.Count < 2)
                return;

            Vector2 current = Points[0];
            for(var i = 1; i < Points.Count; i++) {
                Monocle.Draw.Line(current, Points[i], Color, Thickness);
                current = Points[i];
            }
        }
    }

    internal sealed class Rect : Drawable {
        public Rectangle Area;
        public Color SecondaryColor;
        public RectMode Mode;

        protected internal override void Draw() {
            if (Mode == RectMode.line)
                Monocle.Draw.HollowRect(Area, Color);
            else if (Mode == RectMode.fill)
                Monocle.Draw.Rect(Area, Color);
            else if (Mode == RectMode.bordered) {
                Monocle.Draw.HollowRect(Area, SecondaryColor);
                Monocle.Draw.Rect(Area, Color);
            }
        }

        public enum RectMode {
            fill, line, bordered
        }
    }

    internal sealed class Sprite : Drawable {
        public MTexture Texture;
        public Vector2 Position, Scale, Justification;
        public float Rotation;

        protected internal override void Draw() => Texture.DrawJustified(Position, Justification, Color, Scale, Rotation);
    }

    // TODO
    internal sealed class NinePatch : Drawable {
        public MTexture Texture;
        public Rectangle Area;

        protected internal override void Draw() {

        }
    }

    internal sealed class TileGrid : Drawable {
        public Vector2 Position;
        public VirtualMap<MTexture> Tiles;

        protected internal override void Draw() {
            if (Tiles != null)
                for (int x = 0; x < Tiles.Columns; x++)
                    for (int y = 0; y < Tiles.Rows; y++)
                        Tiles[x, y]?.Draw(Position + new Vector2(x, y) * 8, Vector2.Zero, Color);
        }
    }
}
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Input;

namespace Snowberry.Editor;

using Element = BinaryPacker.Element;

public partial class Room {
    public string Name;

    public Rectangle Bounds;

    public Map Map { get; private set; }

    public int X => Bounds.X;
    public int Y => Bounds.Y;
    public int Width => Bounds.Width;
    public int Height => Bounds.Height;
    public Vector2 Position => new(X, Y);
    public Vector2 Size => new(Width, Height);

    public Rectangle ScissorRect { get; private set; }

    // Music data
    public string Music = "";
    public string AltMusic = "";
    public string Ambience = "";
    public bool[] MusicLayers = new bool[4];

    public int MusicProgress;
    public int AmbienceProgress;

    // Camera offset data
    public Vector2 CameraOffset;

    // Misc data
    public bool Dark;
    public bool Underwater;
    public bool Space;
    public bool DisableDownTransition;
    public WindController.Patterns WindPattern = WindController.Patterns.None;

    // Tiles
    private VirtualMap<char> fgTileMap, bgTileMap;
    private VirtualMap<MTexture> fgTileSprites, bgTileSprites;

    public readonly List<Decal> FgDecals = [];
    public readonly List<Decal> BgDecals = [];

    public readonly List<Entity> Entities = [];
    public readonly List<Entity> Triggers = [];
    public readonly List<Entity> AllEntities = [];

    public readonly Dictionary<Type, List<Entity>> TrackedEntities = new();
    public readonly Dictionary<Type, bool> DirtyTrackedEntities = new();

    public static readonly HashSet<string> IllegalOptionNames = ["id", "x", "y", "width", "height", "originX", "originY", "nodes"];

    internal Room(string name, Rectangle bounds, Map map) {
        Name = name;
        Bounds = bounds;
        Map = map;
        fgTileMap = NewTileMap();
        bgTileMap = NewTileMap();
        Autotile();
    }

    internal Room(LevelData data, Map map)
        : this(data.Name, data.TileBounds, map) {

        // Music
        Music = data.Music;
        AltMusic = data.AltMusic;
        Ambience = data.Ambience;

        MusicLayers = new bool[4];
        MusicLayers[0] = data.MusicLayers[0] > 0;
        MusicLayers[1] = data.MusicLayers[1] > 0;
        MusicLayers[2] = data.MusicLayers[2] > 0;
        MusicLayers[3] = data.MusicLayers[3] > 0;

        MusicProgress = data.MusicProgress;
        AmbienceProgress = data.AmbienceProgress;

        // Camera
        CameraOffset = data.CameraOffset;

        // Misc
        Dark = data.Dark;
        Underwater = data.Underwater;
        Space = data.Space;
        DisableDownTransition = data.DisableDownTransition;
        WindPattern = data.WindPattern;

        // BgTiles
        string[] array = TileSplitter().Split(data.Bg);
        for (int i = 0; i < array.Length; i++)
            for (int j = 0; j < array[i].Length; j++)
                bgTileMap[j, i] = array[i][j];

        // FgTiles
        string[] array2 = TileSplitter().Split(data.Solids);
        for (int i = 0; i < array2.Length; i++)
            for (int j = 0; j < array2[i].Length; j++)
                fgTileMap[j, i] = array2[i][j];

        Autotile();

        // BgDecals
        foreach (DecalData decal in data.BgDecals)
            BgDecals.Add(new Decal(this, decal, false));

        // FgDecals
        foreach (DecalData decal in data.FgDecals)
            FgDecals.Add(new Decal(this, decal, true));

        // Entities
        foreach (EntityData entity in data.Entities) {
            AddEntity(Entity.TryCreate(this, entity, false, out bool success));
            if (!success)
                Snowberry.Log(LogLevel.Warn, $"Attempted to load unknown entity ('{entity.Name}'), using placeholder plugin");
        }

        // Player Spawnpoints (excluded from LevelData.Entities)
        foreach (Vector2 spawn in data.Spawns) {
            Entity s = Entity.Create("player", this);
            s.Position = spawn;
            AddEntity(s);
        }

        // Triggers
        foreach (EntityData trigger in data.Triggers) {
            AddEntity(Entity.TryCreate(this, trigger, true, out bool success));
            if (!success)
                Snowberry.Log(LogLevel.Warn, $"Attempted to load unknown trigger ('{trigger.Name}')");
        }
    }

    private VirtualMap<char> NewTileMap() => new(Bounds.Width, Bounds.Height, '0');

    public char GetTile(bool fg, Vector2 at) {
        return fg ? GetFgTile(at) : GetBgTile(at);
    }

    public char GetFgTile(Vector2 at) {
        Vector2 p = (at - Position * 8) / 8;
        return fgTileMap[(int)p.X, (int)p.Y];
    }

    public char GetBgTile(Vector2 at) {
        Vector2 p = (at - Position * 8) / 8;
        return bgTileMap[(int)p.X, (int)p.Y];
    }

    public bool SetTile(int x, int y, bool fg, char tile) => fg ? SetFgTile(x, y, tile) : SetBgTile(x, y, tile);

    public bool SetFgTile(int x, int y, char tile) {
        char orig = fgTileMap[x, y];
        if (orig != tile) {
            fgTileMap[x, y] = tile;
            return true;
        }

        return false;
    }

    public bool SetBgTile(int x, int y, char tile) {
        char orig = bgTileMap[x, y];
        if (orig != tile) {
            bgTileMap[x, y] = tile;
            return true;
        }

        return false;
    }

    public void Autotile() {
        fgTileSprites = GFX.FGAutotiler.GenerateMapStable(fgTileMap, new Autotiler.Behaviour { EdgesExtend = true }).TileGrid.Tiles;
        bgTileSprites = GFX.BGAutotiler.GenerateMapStable(bgTileMap, new Autotiler.Behaviour { EdgesExtend = true }).TileGrid.Tiles;
    }

    internal List<Selection> GetSelections(
        Rectangle? rect,
        bool entities = true,
        bool triggers = false,
        bool fgDecals = false,
        bool bgDecals = false,
        bool fgTiles = false,
        bool bgTiles = false
    ) {
        List<Selection> result = [];

        if (entities || triggers)
            foreach (Entity entity in AllEntities) {
                if ((!triggers && entity.IsTrigger) || (!entities && !entity.IsTrigger))
                    continue;

                var rects = entity.SelectionRectangles;
                if (rects is { Length: > 0 })
                    for (int i = 0; i < rects.Length; i++) {
                        Rectangle r = rects[i];
                        if (rect?.Intersects(r) ?? true)
                            result.Add(new EntitySelection(entity, i - 1));
                    }
            }

        if(fgDecals)
            result.AddRange(
                from fgDecal
                in FgDecals
                where rect?.Intersects(fgDecal.Bounds) ?? true
                select new DecalSelection(fgDecal, true)
            );

        if(bgDecals)
            result.AddRange(
                from bgDecal
                in BgDecals
                where rect?.Intersects(bgDecal.Bounds) ?? true
                select new DecalSelection(bgDecal, false)
            );

        if (fgTiles || bgTiles) {
            // this is kind of awful
            Rectangle cover;
            if (rect != null) {
                Vector2 start = (rect.Value.Location.ToVector2() / 8).Floor();
                Vector2 end = (new Vector2(rect.Value.Right, rect.Value.Bottom) / 8).Ceiling();
                cover = new Rectangle((int)start.X, (int)start.Y, (int)(end.X - start.X), (int)(end.Y - start.Y));
            } else
                cover = Bounds.Multiply(8);

            for (int x = 0; x < cover.Width; x++) {
                for (int y = 0; y < cover.Height; y++) {
                    Vector2 at = new(cover.X + x, cover.Y + y);
                    if (fgTiles) {
                        char c = GetFgTile(at * 8);
                        if (c != '0')
                            result.Add(new TileSelection(at.ToPoint(), true, this));
                    }
                    if (bgTiles) {
                        char c = GetBgTile(at * 8);
                        if (c != '0')
                            result.Add(new TileSelection(at.ToPoint(), false, this));
                    }
                }
            }
        }

        return result;
    }

    internal void CalculateScissorRect(Editor.BufferCamera camera) {
        Vector2 offset = Position * 8;

        Vector2 zero = Vector2.Transform(offset, camera.Matrix).Round();
        Vector2 size = (Vector2.Transform(offset + new Vector2(Width * 8, Height * 8), camera.Matrix) - zero).Round();
        ScissorRect = new Rectangle(
            (int)zero.X, (int)zero.Y,
            (int)size.X, (int)size.Y);
    }

    internal void Render(Rectangle viewRect) {
        Vector2 offset = Position * 8;

        Draw.Rect(offset, Width * 8, Height * 8, Color.White * 0.1f);

        int startX = Math.Max(0, (viewRect.Left - X * 8) / 8);
        int startY = Math.Max(0, (viewRect.Top - Y * 8) / 8);
        int endX = Math.Min(Width, Width + (viewRect.Right - (X + Width) * 8) / 8);
        int endY = Math.Min(Height, Height + (viewRect.Bottom - (Y + Height) * 8) / 8);

        // BgTiles
        for (int x = startX; x < endX; x++)
            for (int y = startY; y < endY; y++)
                if (bgTileSprites[x, y] != null)
                    bgTileSprites[x, y].Draw(offset + new Vector2(x, y) * 8);

        // BgDecals
        foreach (Decal decal in BgDecals)
            decal.Render();

        // Entities
        Calc.PushRandom(GetHashCode());
        foreach (Entity entity in Entities)
            entity.RenderBefore();
        foreach (Entity entity in Entities)
            entity.Render();
        Calc.PopRandom();

        // FgTiles
        for (int x = startX; x < endX; x++)
            for (int y = startY; y < endY; y++)
                if (fgTileSprites[x, y] != null)
                    fgTileSprites[x, y].Draw(offset + new Vector2(x, y) * 8);

        // FgDecals
        foreach (Decal decal in FgDecals)
            decal.Render();

        // Triggers
        foreach (Entity trigger in Triggers)
            trigger.Render();
    }

    internal void PostRender() {
        DirtyTrackedEntities.Clear();

        foreach (var e in Entities)
            e.Dirty = false;
    }

    internal void HQRender() {
        // Entities
        foreach (Entity entity in Entities)
            entity.HQRender();
        // Triggers
        foreach (Entity trigger in Triggers)
            trigger.HQRender();
        // "See IDs" bind
        if (Editor.Instance.CanTypeShortcut() && MInput.Keyboard.Check(Keys.I) && this == Editor.SelectedRoom) {
            foreach (EntitySelection s in Editor.SelectedObjects.OfType<EntitySelection>()) {
                Rectangle mainRect = s.Entity.SelectionRectangles[0];
                string str = $"#{s.Entity.EntityID}";
                Vector2 size = Fonts.Regular.Measure(str) * 0.5f;
                float opacity = mainRect.Contains((int)Mouse.World.X, (int)Mouse.World.Y) ? 1 : 0.4f;
                Draw.Rect(new Vector2(mainRect.X, mainRect.Y), size.X + 3, size.Y + 2, Color.Black * opacity);
                Fonts.Regular.Draw(str,  new(mainRect.X + 1, mainRect.Y + 1), new(0.5f), Color.White);
            }
        }
    }

    public void UpdateBounds() {
        var newFgTiles = new VirtualMap<char>(Bounds.Width, Bounds.Height, '0');
        for (int x = 0; x < fgTileMap.Columns; x++)
            for (int y = 0; y < fgTileMap.Rows; y++)
                newFgTiles[x, y] = fgTileMap[x, y];
        fgTileMap = newFgTiles;

        var newBgTiles = new VirtualMap<char>(Bounds.Width, Bounds.Height, '0');
        for (int x = 0; x < bgTileMap.Columns; x++)
            for (int y = 0; y < bgTileMap.Rows; y++)
                newBgTiles[x, y] = bgTileMap[x, y];
        bgTileMap = newBgTiles;

        Autotile();
    }

    public void MoveTiles(int dx, int dy, bool autotile = true) {
        var newFgTiles = new VirtualMap<char>(Bounds.Width, Bounds.Height, '0');
        for (int x = 0; x < fgTileMap.Columns; x++)
            for (int y = 0; y < fgTileMap.Rows; y++)
                newFgTiles[x + dx, y + dy] = fgTileMap[x, y];
        fgTileMap = newFgTiles;

        var newBgTiles = new VirtualMap<char>(Bounds.Width, Bounds.Height, '0');
        for (int x = 0; x < bgTileMap.Columns; x++)
            for (int y = 0; y < bgTileMap.Rows; y++)
                newBgTiles[x + dx, y + dy] = bgTileMap[x, y];
        bgTileMap = newBgTiles;

        if(autotile)
            Autotile();
    }

    public Element CreateLevelData() {
        Element ret = new Element {
            Attributes = new Dictionary<string, object> {
                ["name"] = "lvl_" + Name,
                ["x"] = X * 8,
                ["y"] = Y * 8,
                ["width"] = Width * 8,
                ["height"] = Height * 8,

                ["music"] = Music,
                ["alt_music"] = AltMusic,
                ["ambience"] = Ambience,
                ["musicLayer1"] = MusicLayers[0],
                ["musicLayer2"] = MusicLayers[1],
                ["musicLayer3"] = MusicLayers[2],
                ["musicLayer4"] = MusicLayers[3],

                ["musicProgress"] = MusicProgress,
                ["ambienceProgress"] = AmbienceProgress,

                ["dark"] = Dark,
                ["underwater"] = Underwater,
                ["space"] = Space,
                ["disableDownTransition"] = DisableDownTransition,
                ["windPattern"] = WindPattern.ToString(),

                ["cameraOffsetX"] = CameraOffset.X,
                ["cameraOffsetY"] = CameraOffset.Y
            }
        };

        Element entitiesElement = new Element {
            Attributes = new Dictionary<string, object>(),
            Name = "entities",
            Children = []
        };
        ret.Children = [entitiesElement];

        foreach (var entity in Entities) {
            Element entityElem = new Element {
                Name = entity.Name,
                Children = [],
                Attributes = new Dictionary<string, object> {
                    ["id"] = entity.EntityID,
                    ["x"] = entity.X - X * 8,
                    ["y"] = entity.Y - Y * 8,
                    ["originX"] = entity.Origin.X,
                    ["originY"] = entity.Origin.Y
                }
            };
            if (entity.MinWidth != -1 || entity.Width != 0)
                entityElem.Attributes["width"] = entity.Width;
            if (entity.MinHeight != -1 || entity.Height != 0)
                entityElem.Attributes["height"] = entity.Height;

            entity.SaveAttrs(entityElem);

            foreach (var node in entity.Nodes)
                entityElem.Children.Add(new Element {
                    Name = "node", // for loenn compatibility
                    Attributes = new() {
                        ["x"] = node.X - X * 8,
                        ["y"] = node.Y - Y * 8
                    }
                });

            entitiesElement.Children.Add(entityElem);
        }

        Element triggersElement = new Element {
            Attributes = new Dictionary<string, object>(),
            Name = "triggers",
            Children = []
        };
        ret.Children.Add(triggersElement);

        foreach (var trigger in Triggers) {
            Element triggersElem = new Element {
                Name = trigger.Name,
                Children = [],
                Attributes = new Dictionary<string, object> {
                    ["id"] = trigger.EntityID,
                    ["x"] = trigger.X - X * 8,
                    ["y"] = trigger.Y - Y * 8,
                    ["originX"] = trigger.Origin.X,
                    ["originY"] = trigger.Origin.Y
                }
            };
            if (trigger.MinWidth != -1 || trigger.Width != 0)
                triggersElem.Attributes["width"] = trigger.Width;
            if (trigger.MinHeight != -1 || trigger.Height != 0)
                triggersElem.Attributes["height"] = trigger.Height;
            trigger.SaveAttrs(triggersElem);

            foreach (var node in trigger.Nodes)
                triggersElem.Children.Add(new Element {
                    Name = "node", // for loenn compatibility
                    Attributes = new Dictionary<string, object> {
                        ["x"] = node.X - X * 8,
                        ["y"] = node.Y - Y * 8
                    }
                });

            triggersElement.Children.Add(triggersElem);
        }

        Element fgDecalsElem = new Element {
            Name = "fgdecals",
            Children = []
        };
        ret.Children.Add(fgDecalsElem);
        foreach (var decal in FgDecals) {
            var dElem = new Element {
                Attributes = new Dictionary<string, object> {
                    ["x"] = decal.Position.X - X * 8,
                    ["y"] = decal.Position.Y - Y * 8,
                    ["scaleX"] = decal.Scale.X,
                    ["scaleY"] = decal.Scale.Y,
                    ["texture"] = decal.Texture,
                    ["color"] = decal.Color.IntoRgbString(),
                    ["rotation"] = decal.Rotation
                }
            };
            if (decal.Depth is int i)
                dElem.Attributes["depth"] = i;
            fgDecalsElem.Children.Add(dElem);
        }

        Element bgDecalsElem = new Element {
            Name = "bgdecals",
            Children = []
        };
        ret.Children.Add(bgDecalsElem);
        foreach (var decal in BgDecals) {
            var dElem = new Element {
                Attributes = new Dictionary<string, object> {
                    ["x"] = decal.Position.X - X * 8,
                    ["y"] = decal.Position.Y - Y * 8,
                    ["scaleX"] = decal.Scale.X,
                    ["scaleY"] = decal.Scale.Y,
                    ["texture"] = decal.Texture,
                    ["color"] = decal.Color.IntoRgbString(),
                    ["rotation"] = decal.Rotation
                }
            };
            if (decal.Depth is int i)
                dElem.Attributes["depth"] = i;
            bgDecalsElem.Children.Add(dElem);
        }

        StringBuilder fgTilesTxt = new StringBuilder();
        for (int y = 0; y < fgTileMap.Rows; y++) {
            for (int x = 0; x < fgTileMap.Columns; x++)
                fgTilesTxt.Append(fgTileMap[x, y]);

            fgTilesTxt.Append('\n');
        }

        StringBuilder bgTilesTxt = new StringBuilder();
        for (int y = 0; y < bgTileMap.Rows; y++) {
            for (int x = 0; x < bgTileMap.Columns; x++)
                bgTilesTxt.Append(bgTileMap[x, y]);

            bgTilesTxt.Append('\n');
        }

        Element fgSolidsElem = new Element {
            Name = "solids",
            Attributes = new Dictionary<string, object> {
                ["innerText"] = fgTilesTxt.ToString()
            }
        };
        ret.Children.Add(fgSolidsElem);

        Element bgSolidsElem = new Element {
            Name = "bg",
            Attributes = new Dictionary<string, object> {
                ["innerText"] = bgTilesTxt.ToString()
            }
        };
        ret.Children.Add(bgSolidsElem);

        return ret;
    }

    public void AddEntity(Entity e) {
        AllEntities.Add(e);
        if (e.IsTrigger)
            Triggers.Add(e);
        else
            Entities.Add(e);
        if (e.Tracked) {
            Type tracking = e.GetType();
            if (!TrackedEntities.ContainsKey(tracking))
                TrackedEntities[tracking] = [];
            TrackedEntities[tracking].Add(e);
            DirtyTrackedEntities[tracking] = true;
        }
    }

    public void RemoveEntity(Entity e) {
        AllEntities.Remove(e);
        Entities.Remove(e);
        Triggers.Remove(e);
        Type tracking = e.GetType();
        if (e.Tracked && TrackedEntities.TryGetValue(tracking, out var value)) {
            value.Remove(e);
            if (TrackedEntities[tracking].Count == 0)
                TrackedEntities.Remove(tracking);
        }
    }

    public void MarkEntityDirty(Entity e) {
        if (e.Tracked)
            DirtyTrackedEntities[e.GetType()] = true;

        e.Dirty = true;
    }

    public bool IsEntityTypeDirty(Type t) => DirtyTrackedEntities.ContainsKey(t) && DirtyTrackedEntities[t];

    public void SnapshotAndAddEntity(Entity e) {
        UndoRedo.BeginAction("add entity", SnapshotEntityInclusion(e));
        AddEntity(e);
        UndoRedo.CompleteAction();
    }

    public void SnapshotAndRemoveEntity(Entity e) {
        UndoRedo.BeginAction("remove entity", SnapshotEntityInclusion(e));
        RemoveEntity(e);
        UndoRedo.CompleteAction();
    }

    #region Snapshotters

    public UndoRedo.Snapshotter SnapshotTiles() => new TilesSnapshotter(this);

    private record TilesSnapshotter(Room r) : UndoRedo.Snapshotter<(VirtualMap<char> fg, VirtualMap<char> bg)> {

        public (VirtualMap<char> fg, VirtualMap<char> bg) Snapshot() => (r.fgTileMap.Clone(), r.bgTileMap.Clone());

        public void Apply((VirtualMap<char> fg, VirtualMap<char> bg) t) {
            (r.fgTileMap, r.bgTileMap) = t;
            r.Autotile();
        }
    }

    public UndoRedo.Snapshotter SnapshotEntityInclusion(Entity e) => new EntityInclusionSnapshotter(this, e);

    // split in this way to make tracked entities behave nicer
    private record EntityInclusionSnapshotter(Room r, Entity e) : UndoRedo.Snapshotter<bool> {
        public bool Snapshot() => r.AllEntities.Contains(e);

        public void Apply(bool included) {
            if (!included && r.AllEntities.Contains(e))
                r.RemoveEntity(e);
            else if (included && !r.AllEntities.Contains(e))
                r.AddEntity(e);
        }
    }

    public UndoRedo.Snapshotter SnapshotDecalInclusion(Decal d) => new DecalInclusionSnapshotter(this, d);

    // deja vu
    private record DecalInclusionSnapshotter(Room r, Decal d) : UndoRedo.Snapshotter<bool> {
        public bool Snapshot() => (d.Fg ? r.FgDecals : r.BgDecals).Contains(d);

        public void Apply(bool included) {
            var list = (d.Fg ? r.FgDecals : r.BgDecals);

            if (!included && list.Contains(d))
                list.Remove(d);
            else if (included && !list.Contains(d))
                list.Add(d);
        }
    }

    #endregion

    [GeneratedRegex(@"\r\n|\n\r|\n|\r")]
    private static partial Regex TileSplitter();
}
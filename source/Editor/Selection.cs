using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor;

public abstract class Selection {

    // e.g. entity type or decal path, should be unique
    public abstract string Name();

    public abstract Color Accent();

    public abstract void Move(Vector2 amount);

    public abstract void RemoveSelf();

    public abstract Rectangle Area();

    public abstract UndoRedo.Snapshotter Snapshotter();

    public bool Contains(Point p) => Area().Contains(p);
}

public class EntitySelection : Selection {

    public readonly Entity Entity;
    public readonly int Index;

    public EntitySelection(Entity entity, int index) {
        Entity = entity;
        Index = index;
    }

    public override string Name() => Entity.Name;

    public override Color Accent() => Entity.Info.Module.Color;

    public override void Move(Vector2 amount) {
        if (Index < 0)
            Entity.Move(amount);
        else
            Entity.MoveNode(Index, amount);
    }

    public override void RemoveSelf() {
        Entity.Room.RemoveEntity(Entity);
        Entity.Room.MarkEntityDirty(Entity); // tracked entities
    }

    public override Rectangle Area() => Entity.SelectionRectangles[Index + 1];

    public override UndoRedo.Snapshotter Snapshotter() => Entity.SnapshotBounds();

    public override bool Equals(object obj) => obj is EntitySelection s && s.Entity.Equals(Entity) && s.Index == Index;

    public override int GetHashCode() => Entity.GetHashCode() ^ Index;
}

public class DecalSelection : Selection {

    public readonly Decal Decal;
    public readonly bool Fg;

    public DecalSelection(Decal decal, bool fg) {
        Decal = decal;
        Fg = fg;
    }

    public override string Name() => Decal.Texture;

    public override Color Accent() => Color.White;

    public override void Move(Vector2 amount) => Decal.Position += amount;

    public override void RemoveSelf() {
        if (Fg)
            Decal.Room.FgDecals.Remove(Decal);
        else
            Decal.Room.BgDecals.Remove(Decal);
    }

    public override Rectangle Area() => Decal.Bounds;

    public override UndoRedo.Snapshotter Snapshotter() => Decal.SnapshotPosition();

    public override bool Equals(object obj) => obj is DecalSelection ds && ds.Decal == Decal;

    public override int GetHashCode() => Decal.GetHashCode();
}

public class TileSelection : Selection {

    private static VirtualMap<char?> delayedFgTileMap, delayedBgTileMap;

    public Point Position;
    public readonly bool Fg;
    public readonly Room Room;

    public TileSelection(Point position, bool fg, Room room) {
        Position = position;
        Fg = fg;
        Room = room;
    }

    public override string Name() => "";

    public override Color Accent() => Color.Red;

    public override void Move(Vector2 amount) {
        Vector2 adj = Position.ToVector2() + (amount / 8).Round();
        RemoveSelf();
        SetTileDelayed(
            (int)(adj.X - Room.Position.X),
            (int)(adj.Y - Room.Position.Y),
            Fg,
            Room.GetTile(Fg, Position.ToVector2() * 8)
        );
        Position = adj.ToPoint();
    }

    public override void RemoveSelf() =>
        SetTileDelayed((int)(Position.X - Room.Position.X), (int)(Position.Y - Room.Position.Y), Fg, '0');

    public override Rectangle Area() => new Rectangle(Position.X, Position.Y, 1, 1).Multiply(8);

    public override UndoRedo.Snapshotter Snapshotter() => Room.SnapshotTiles();

    public override bool Equals(object obj) =>
        obj is TileSelection ts && ts.Position == Position && ts.Fg == Fg && ts.Room == Room;

    public override int GetHashCode() => Position.GetHashCode() ^ Fg.Bit() ^ Room.GetHashCode();

    private void SetTileDelayed(int x, int y, bool fg, char tile) {
        VirtualMap<char?> map = (fg ? (delayedFgTileMap ??= NewCondTileMap()) : (delayedBgTileMap ??= NewCondTileMap()));
        if(!(tile == '0' && map[x, y] != null))
            map[x, y] = tile;
    }

    private VirtualMap<char?> NewCondTileMap() => new(Room.Bounds.Width, Room.Bounds.Height, null);

    internal static void FinishMove() {
        Room room = Editor.SelectedRoom;

        if(room != null && (delayedFgTileMap != null || delayedBgTileMap != null)){
            bool retile = false;
            if (delayedFgTileMap != null)
                for (int x = 0; x < room.Width; x++)
                    for (int y = 0; y < room.Height; y++)
                        if (delayedFgTileMap[x, y] is char c)
                            retile |= room.SetFgTile(x, y, c);

            if (delayedBgTileMap != null)
                for (int x = 0; x < room.Width; x++)
                    for (int y = 0; y < room.Height; y++)
                        if (delayedBgTileMap[x, y] is char c)
                            retile |= room.SetBgTile(x, y, c);

            if (retile)
                room.Autotile();
        }

        delayedFgTileMap = delayedBgTileMap = null;
    }
}
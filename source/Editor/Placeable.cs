using Microsoft.Xna.Framework;

namespace Snowberry.Editor;

public interface Placeable {

    public Room Room { get; set; }
    public Vector2 Position { get; set; }

    public int X => (int)Position.X;
    public int Y => (int)Position.Y;

    public void Render();

    public void AddToRoom(Room room);
}

public interface Resizable : Placeable {

    public int Width { get; set; }
    public int Height { get; set; }
    public int MinWidth { get; }
    public int MinHeight { get; }

    public Rectangle Bounds => new(X, Y, Width, Height);
}
using Microsoft.Xna.Framework;

namespace Snowberry.Editor;

public interface Placeable {

    public Room Room { get; set; }
    public Vector2 Position { get; set; }

    public void Render();

    public void AddToRoom(Room room);
}

public interface Resizable : Placeable {

    public int Width { get; set; }
    public int Height { get; set; }
    public int MinWidth { get; }
    public int MinHeight { get; }
}

public static class ResizableExt {

    public static Rectangle Bounds(this Resizable rx) => new((int)rx.Position.X, (int)rx.Position.Y, rx.Width, rx.Height);
}
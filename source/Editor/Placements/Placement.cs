namespace Snowberry.Editor.Placements;

public interface Placement {

    public string Name { get; }
    public string ModName { get; }

    public Placeable Build(Room room);
}
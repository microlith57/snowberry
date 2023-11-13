using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;


namespace Snowberry.Editor.Entities; 

[Plugin("slider")]
public class Plugin_Slider : Entity {
    [Option("surface")] public Slider.Surfaces Surface = Slider.Surfaces.Floor;
    [Option("clockwise")] public bool Clockwise = true;

    public override void Render() {
        base.Render();

        Draw.Circle(Position, 12f, Color.Red, 12);
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(24), justify: new(0.5f, 0.5f));
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Slider", "slider");
    }
}
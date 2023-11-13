using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Snowberry.Editor.Entities;

[Plugin("infiniteStar")]
public class Plugin_Feather : Entity{
    [Option("shielded")] public bool Shielded;
    [Option("singleUse")] public bool OneUse;

    public override void Render(){
        base.Render();
        FromSprite("flyFeather", "loop")?.DrawCentered(Position);
        if(Shielded)
            Draw.Circle(Position, 12f, Color.White, 5);
    }

    protected override IEnumerable<Rectangle> Select(){
        yield return RectOnRelative(Shielded ? new(22) : new(18), justify: new(0.5f));
    }

    public static void AddPlacements(){
        Placements.EntityPlacementProvider.Create("Feather", "infiniteStar");
        Placements.EntityPlacementProvider.Create("Feather (Shielded)", "infiniteStar", new Dictionary<string, object>() { { "shielded", true } });
    }
}
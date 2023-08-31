using Celeste;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities;

[Plugin("powerSourceNumber")]
public class Plugin_PowerSourceNumber : Entity {
    [Option("number")] public int Number = 1;
    [Option("strawberries")] public string Strawberries = "";
    [Option("keys")] public string Keys = "";

    public override void Render() {
        base.Render();

        GFX.Game["scenery/powersource_numbers/1"].DrawCentered(Position);
        GFX.Game["scenery/powersource_numbers/1_glow"].DrawCentered(Position);
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(24), justify: new(0.5f));
    }

    public static void AddPlacements() {
        Placements.Create("Power Source Number", "powerSourceNumber");
    }
}
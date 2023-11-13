using Celeste;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities;

[Plugin("refill")]
public class Plugin_Refill : Entity {
    [Option("twoDash")] public bool TwoDash = false;
    [Option("oneUse")] public bool OneUse = false;

    public override void Render() {
        base.Render();

        GFX.Game[$"objects/{(TwoDash ? "refillTwo" : "refill")}/idle00"].DrawOutlineCentered(Position);
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(TwoDash ? new(8, 12) : new(10), justify: new(0.5f));
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Refill", "refill");
        Placements.EntityPlacementProvider.Create("Double Refill", "refill", new Dictionary<string, object>() { { "twoDash", true } });
    }
}
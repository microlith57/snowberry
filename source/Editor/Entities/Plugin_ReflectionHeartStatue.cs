using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("reflectionHeartStatue")]
public class Plugin_ReflectionHeartStatue : Entity {

    public static readonly MTexture[] sprites = [
        GFX.Game["objects/reflectionHeart/statue"],
        GFX.Game["objects/reflectionHeart/gem"],
        GFX.Game["objects/reflectionHeart/torch00"]
    ];

    public static readonly Color[] gemColors = [
        Calc.HexToColor("F0F0F0"), // U
        Calc.HexToColor("9171F2"), // L
        Calc.HexToColor("0A44E0"), // DR
        Calc.HexToColor("B32D00"), // UR
        Calc.HexToColor("9171F2"), // L
        Calc.HexToColor("FFCD37") // UL
    ];

    public override int MinNodes => 5;
    public override int MaxNodes => 5;

    public override void Render() {
        base.Render();

        sprites[0].DrawJustified(Position, new(0.5f, 1));

        for (int i = 0; i < 4; i++) {
            GFX.Game[$"objects/reflectionHeart/hint0{i}"].DrawJustified(Nodes[i] + new Vector2(0, (i == 3 ? 6 : 12)), new(0.5f, 0));
            sprites[2].DrawJustified(Nodes[i] + new Vector2(-1, 16), new(0.5f, 1));
        }

        for (int i = 0; i < 6; i++) {
            sprites[1].DrawCentered(Nodes[4] + new Vector2((24 * i) - 60, 0), gemColors[i]);
        }

    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(32, 36), justify: new(0.5f, 1f));

        for (int i = 0; i < 3; i++) {
            yield return RectOnAbsolute(new(22, 36), Nodes[i] + new Vector2(0, 4), new(0.5f, 0));
        }
        yield return RectOnAbsolute(new(28, 44), Nodes[3] + new Vector2(1, 4), new(0.5f, 0));
        yield return RectOnAbsolute(new(132, 14), Nodes[4], new(0.5f, 0.5f));
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Reflection Heart Statue", "reflectionHeartStatue");
    }
}
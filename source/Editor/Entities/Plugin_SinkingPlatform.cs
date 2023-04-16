using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("sinkingPlatform")]
public class Plugin_SinkingPlatform : Entity {

    public static readonly Color LineEdgeColour = Calc.HexToColor("2a1923");
    public static readonly Color LineInnerColour = Calc.HexToColor("160b12");

    // TODO: suggestions
    [Option("texture")] private string Texture = "default";

    public override int MinWidth => 16;

    public override void InitializeAfter() {
        base.InitializeAfter();

        Texture = GetVanillaLevelTexture() ?? Texture;
    }

    public override void Render() {
        base.Render();

        Draw.Rect(X - 1 + Width / 2f, Y + 4, 3, (Room?.Height ?? 15) * 8, LineEdgeColour);
        Draw.Rect(X + Width / 2f, Y + 4, 1, (Room?.Height ?? 15) * 8, LineInnerColour);

        MTexture tex = GFX.Game["objects/woodPlatform/" + Texture];
        MTexture
            start = tex.GetSubtexture(0, 0, 8, 8),
            mid = tex.GetSubtexture(8, 0, 8, 8),
            centre = tex.GetSubtexture(16, 0, 8, 8),
            end = tex.GetSubtexture(24, 0, 8, 8);

        start.Draw(Position);
        for (int i = 8; i < Width - 8; i += 8)
            mid.Draw(Position + new Vector2(i, 0));
        end.Draw(Position + new Vector2(Width - 8, 0));
        centre.Draw(Position + new Vector2(Width / 2f - 4, 0));
    }

    private static string GetVanillaLevelTexture() {
        return Editor.VanillaLevelID switch {
            4 => "cliffside",
            _ => null
        };
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(Width, 8));
    }

    public static void AddPlacements() {
        Placements.Create("Sinking Platform", "sinkingPlatform");
    }
}
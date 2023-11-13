using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("moveBlock")]
public class Plugin_MoveBlock : Entity {

    public static readonly Color IdleBgFill = Calc.HexToColor("474070");

    private static readonly Dictionary<MoveBlock.Directions, string> ArrowNames = new() {
        [MoveBlock.Directions.Right] = "0",
        [MoveBlock.Directions.Up] = "2",
        [MoveBlock.Directions.Left] = "4",
        [MoveBlock.Directions.Down] = "6"
    };

    [Option("direction")] public MoveBlock.Directions Direction = MoveBlock.Directions.Right;
    [Option("canSteer")] public bool CanSteer = false;
    [Option("fast")] public bool Fast = false;

    public override int MinWidth => 16;
    public override int MinHeight => 16;

    public override void Render() {
        base.Render();

        int widthTiles = Width / 8;
        int heightTiles = Height / 8;
        MTexture baseTex = GFX.Game["objects/moveBlock/base"];
        MTexture buttonTex = GFX.Game["objects/moveBlock/button"];
        if (CanSteer && Direction is MoveBlock.Directions.Left or MoveBlock.Directions.Right) {
            for (int idx = 0; idx < widthTiles; ++idx) {
                int segmentX = idx == 0 ? 0 : (idx < widthTiles - 1 ? 1 : 2);
                buttonTex.GetSubtexture(segmentX * 8, 0, 8, 8).DrawCentered(Position + new Vector2(idx * 8 + 4, 0), IdleBgFill);
            }

            baseTex = GFX.Game["objects/moveBlock/base_h"];
        } else if (CanSteer && Direction is MoveBlock.Directions.Up or MoveBlock.Directions.Down) {
            for (int idx = 0; idx < heightTiles; ++idx) {
                int segmentY = idx == 0 ? 0 : (idx < heightTiles - 1 ? 1 : 2);
                buttonTex.GetSubtexture(segmentY * 8, 0, 8, 8).DrawCentered(Position + new Vector2(0, idx * 8 + 4), IdleBgFill, new Vector2(1, -1), MathHelper.PiOver2);
                buttonTex.GetSubtexture(segmentY * 8, 0, 8, 8).DrawCentered(Position + new Vector2(widthTiles * 8, idx * 8 + 4), IdleBgFill, new Vector2(1, 1), MathHelper.PiOver2);
            }

            baseTex = GFX.Game["objects/moveBlock/base_v"];
        }

        Draw.Rect(X + 3f, Y + 3f, Width - 6f, Height - 6f, IdleBgFill);

        for (int x = 0; x < widthTiles; ++x) {
            for (int y = 0; y < heightTiles; ++y) {
                int segmentX = x == 0 ? 0 : (x < widthTiles - 1 ? 1 : 2);
                int segmentY = y == 0 ? 0 : (y < heightTiles - 1 ? 1 : 2);
                baseTex.GetSubtexture(segmentX * 8, segmentY * 8, 8, 8).DrawCentered(Position + new Vector2(x * 8 + 4, y * 8 + 4));
            }
        }

        Draw.Rect(Center.X - 4f, Center.Y - 4f, 8f, 8f, IdleBgFill);
        GFX.Game[$"objects/moveBlock/arrow0{ArrowNames[Direction]}"].DrawCentered(Center);
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Move Block", "moveBlock");
        Placements.EntityPlacementProvider.Create("Move Block (Steerable)", "moveBlock", new() { ["canSteer"] = true });
    }
}
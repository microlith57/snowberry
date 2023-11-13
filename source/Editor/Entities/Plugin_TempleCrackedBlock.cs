using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("templeCrackedBlock")]
public class Plugin_TempleCrackedBlock : Entity {

    [Option("persistent")] public bool Persistent = false;

    public override int MinWidth => 24;
    public override int MinHeight => 24;

    public override void Render() {
        base.Render();

        int wTiles = Width / 8, hTiles = Height / 8;
        MTexture tex = GFX.Game["objects/temple/breakBlock00"];
        for(int x = 0; x < wTiles; ++x){
            for(int y = 0; y < hTiles; ++y){
                int tx = x >= wTiles / 2 || x >= 2 ? (x < wTiles / 2 || x < wTiles - 2 ? 2 + x % 2 : 5 - (wTiles - x - 1)) : x;
                int ty = y >= hTiles / 2 || y >= 2 ? (y < hTiles / 2 || y < hTiles - 2 ? 2 + y % 2 : 5 - (hTiles - y - 1)) : y;
                tex.GetSubtexture(tx * 8, ty * 8, 8, 8).Draw(Position + new Vector2(x * 8, y * 8));
            }
        }
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Temple Cracked Block", "templeCrackedBlock");
    }
}
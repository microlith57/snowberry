using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Snowberry;

public static class StableAutotiler {

    private static FastReflectionDelegate TileHandler = typeof(Autotiler).GetMethod("TileHandler", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).CreateFastDelegate();
    private static FieldInfo TilesTextures = typeof(Autotiler).Assembly.GetType("Celeste.Autotiler+Tiles").GetField("Textures");

    public static Autotiler.Generated GenerateMapStable(this Autotiler self, VirtualMap<char> mapData, Autotiler.Behaviour behaviour) =>
        self.GenerateStable(mapData, 0, 0, mapData.Columns, mapData.Rows, false, '0', behaviour);

    public static Autotiler.Generated GenerateStable(
        this Autotiler self,
        VirtualMap<char> mapData,
        int startX,
        int startY,
        int tilesX,
        int tilesY,
        bool forceSolid,
        char forceID,
        Autotiler.Behaviour behaviour
    ) {
        TileGrid tileGrid = new TileGrid(8, 8, tilesX, tilesY);
        AnimatedTiles animatedTiles = new AnimatedTiles(tilesX, tilesY, GFX.AnimatedTilesBank);
        Rectangle forceFill = Rectangle.Empty;
        if (forceSolid)
            forceFill = new Rectangle(startX, startY, tilesX, tilesY);
        if (mapData != null) {
            for (int x1 = startX; x1 < startX + tilesX; x1 += 50) {
                for (int y1 = startY; y1 < startY + tilesY; y1 += 50) {
                    if (!mapData.AnyInSegmentAtTile(x1, y1)) {
                        y1 = y1 / 50 * 50;
                    } else {
                        int x2 = x1;
                        for (int index1 = Math.Min(x1 + 50, startX + tilesX); x2 < index1; ++x2) {
                            int y2 = y1;
                            for (int index2 = Math.Min(y1 + 50, startY + tilesY); y2 < index2; ++y2) {
                                object tiles = TileHandler(self, mapData, x2, y2, forceFill, forceID, behaviour);
                                if (tiles != null) {
                                    List<MTexture> choices = (List<MTexture>)TilesTextures.GetValue(tiles);
                                    tileGrid.Tiles[x2 - startX, y2 - startY] = Pick(choices, x2, y2);
                                    //if (tiles.HasOverlays)
                                    //    animatedTiles.Set(x2 - startX, y2 - startY, Calc.Random.Choose(tiles.OverlapSprites));
                                }
                            }
                        }
                    }
                }
            }
        } else {
            for (int x = startX; x < startX + tilesX; ++x) {
                for (int y = startY; y < startY + tilesY; ++y) {
                    object tiles = TileHandler(self, null, x, y, forceFill, forceID, behaviour);
                    if (tiles != null) {
                        List<MTexture> choices = (List<MTexture>)TilesTextures.GetValue(tiles);
                        tileGrid.Tiles[x - startX, y - startY] = Pick(choices, x, y);
                        //if (tiles.HasOverlays)
                        //    animatedTiles.Set(x - startX, y - startY, Calc.Random.Choose(tiles.OverlapSprites));
                    }
                }
            }
        }

        return new Autotiler.Generated {
            TileGrid = tileGrid,
            SpriteOverlay = animatedTiles
        };
    }

    private static MTexture Pick(List<MTexture> choices, int x2, int y2) => choices[(x2 * 84 ^ y2) % choices.Count];
}
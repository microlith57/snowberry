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

    public static Autotiler.Generated GenerateBoxStable(this Autotiler self, char id, int tilesX, int tilesY) =>
        self.GenerateStable(null, 0, 0, tilesX, tilesY, true, id, new Autotiler.Behaviour());

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

    private static ulong mod(ulong x, ulong m) => (x%m + m)%m;
    private static MTexture Pick(List<MTexture> choices, int x, int y) => choices[(int)mod(splitmix64((((ulong)x) << 32) + (ulong)y), (ulong)choices.Count)];

    #region Splitmix64
    /*  Written in 2015 by Sebastiano Vigna (vigna@acm.org)

    To the extent possible under law, the author has dedicated all copyright
    and related and neighboring rights to this software to the public domain
    worldwide. This software is distributed without any warranty.

    See <http://creativecommons.org/publicdomain/zero/1.0/>.

    This is a fixed-increment version of Java 8's SplittableRandom generator
    See http://dx.doi.org/10.1145/2714064.2660195 and
    http://docs.oracle.com/javase/8/docs/api/java/util/SplittableRandom.html

    It is a very fast generator passing BigCrush, and it can be useful if
    for some reason you absolutely want 64 bits of state.
    */
    private static ulong splitmix64(ulong seed) {
        ulong z = seed + 0x9e3779b97f4a7c15;
        z = (z ^ z >> 30) * 0xbf58476d1ce4e5b9;
        z = (z ^ z >> 27) * 0x94d049bb133111eb;
        return z ^ z >> 31;
    }
    #endregion
}
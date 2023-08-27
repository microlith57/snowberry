using System;
using System.Collections.Generic;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Snowberry.Editor.Entities.Util;

public class EditorNinePatch {


    private MTexture[,] nineSliceTextureCorners;
    private List<MTexture> nineSliceTextureTop;
    private List<MTexture> nineSliceTextureLeft;
    private List<MTexture> nineSliceTextureRight;
    private List<MTexture> nineSliceTextureBottom;
    private MTexture[,] nineSliceTextureMiddle;
    public EditorNinePatch(MTexture mtexture) {

        int tileWidth = mtexture.Width/8;
        int tileHeight = mtexture.Height/8;

        nineSliceTextureCorners = new MTexture[2, 2];
        nineSliceTextureTop = new();
        nineSliceTextureLeft = new();
        nineSliceTextureRight = new();
        nineSliceTextureBottom = new();
        nineSliceTextureMiddle = new MTexture[tileWidth - 2, tileHeight - 2];

        for (int i = 0; i < 2; i++) {
            for (int j = 0; j < 2; j++) {
                nineSliceTextureCorners[i, j] = mtexture.GetSubtexture(new Rectangle((i == 1 ? tileWidth - 1 : i) * 8, (j == 1 ? tileHeight - 1 : j) * 8, 8, 8));
            }
        }
        for (int i = 1; i < tileWidth - 1; i++) {
            nineSliceTextureTop.Add(mtexture.GetSubtexture(new Rectangle(i*8, 0, 8, 8)));
            nineSliceTextureBottom.Add(mtexture.GetSubtexture(new Rectangle(i*8, (tileHeight - 1) * 8, 8, 8)));
        }
        for (int j = 1; j < tileHeight - 1; j++) {
            nineSliceTextureLeft.Add(mtexture.GetSubtexture(new Rectangle(0, j*8, 8, 8)));
            nineSliceTextureRight.Add(mtexture.GetSubtexture(new Rectangle((tileWidth - 1) * 8, j * 8, 8, 8)));
        }
        for (int k = 1; k < tileWidth - 1; k++) {
            for (int l = 1; l < tileHeight - 1; l++) {
                nineSliceTextureMiddle[k-1, l-1] = mtexture.GetSubtexture(new Rectangle(k*8, l * 8, 8, 8));
            }
        }
    }

    public void Draw(Vector2 pos, int width, int height, Color color) {
        int tileWidth = (int)(width / 8f);
        int tileHeight = (int)(height / 8f);

        nineSliceTextureCorners[0, 0].Draw(pos + new Vector2(0f, 0f), Vector2.Zero, color);
        nineSliceTextureCorners[1, 0].Draw(pos + new Vector2(width - 8f, 0f), Vector2.Zero, color);
        nineSliceTextureCorners[0, 1].Draw(pos + new Vector2(0f, height - 8f), Vector2.Zero, color);
        nineSliceTextureCorners[1, 1].Draw(pos + new Vector2(width - 8f, height - 8f), Vector2.Zero, color);

        for (int i = 1; i < tileWidth - 1; i++) {
            nineSliceTextureTop[(i - 1) % nineSliceTextureTop.Count].Draw(pos + new Vector2((float)(i * 8), 0f), Vector2.Zero, color);
            nineSliceTextureBottom[(i - 1) % nineSliceTextureBottom.Count].Draw(pos + new Vector2((float)(i * 8), height - 8f), Vector2.Zero, color);
        }

        for (int j = 1; j < tileHeight - 1; j++) {
            nineSliceTextureLeft[(j - 1) % nineSliceTextureLeft.Count].Draw(pos + new Vector2(0f, (float)(j * 8)), Vector2.Zero, color);
            nineSliceTextureRight[(j - 1) % nineSliceTextureRight.Count].Draw(pos + new Vector2(width - 8f, (float)(j * 8)), Vector2.Zero, color);
        }

        int kMax = nineSliceTextureMiddle.GetLength(0);
        int lMax = nineSliceTextureMiddle.GetLength(1);

        for (int k = 1; k < tileWidth - 1; k++) {
            for (int l = 1; l < tileHeight - 1; l++) {
                nineSliceTextureMiddle[(k - 1) % kMax, (l - 1) % lMax].Draw(pos + new Vector2((float)k, (float)l) * 8f, Vector2.Zero, color);
            }
        }
    }
}
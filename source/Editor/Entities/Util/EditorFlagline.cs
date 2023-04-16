using System;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities.Util;

public class EditorFlagline {
    private Color[] colors;
    private Color[] highlights;
    private Color lineColor;
    private Color pinColor;
    private Cloth[] clothes;
    private float waveTimer;
    public float ClothDroopAmount = 0.6f;
    public Vector2 To;
    public Vector2 From;

    public EditorFlagline(
        Vector2 to,
        Vector2 from,
        Color lineColor,
        Color pinColor,
        Color[] colors,
        int minFlagHeight,
        int maxFlagHeight,
        int minFlagLength,
        int maxFlagLength,
        int minSpace,
        int maxSpace
    ) {
        To = to;
        From = from;
        this.colors = colors;
        this.lineColor = lineColor;
        this.pinColor = pinColor;
        waveTimer = Calc.Random.NextFloat() * 6.2831855f;
        highlights = new Color[colors.Length];
        for (int index = 0; index < colors.Length; ++index)
            highlights[index] = Color.Lerp(colors[index], Color.White, 0.1f);
        clothes = new Cloth[10];
        for (int index = 0; index < clothes.Length; ++index)
            clothes[index] = new Cloth {
                Color = Calc.Random.Next(colors.Length),
                Height = Calc.Random.Next(minFlagHeight, maxFlagHeight),
                Length = Calc.Random.Next(minFlagLength, maxFlagLength),
                Step = Calc.Random.Next(minSpace, maxSpace)
            };
    }

    public void Render() {
        Vector2 begin = From.X < (double)To.X ? From : To;
        Vector2 end = From.X < (double)To.X ? To : From;
        float dist = (begin - end).Length();
        float distTiles = dist / 8f;
        SimpleCurve curve = new SimpleCurve(begin, end, (end + begin) / 2f + Vector2.UnitY * (distTiles + (float)(Math.Sin(waveTimer) * distTiles * 0.3)));
        if (!IsVisible(curve))
            return;
        Vector2 current = begin;
        float percent = 0;
        int num3 = 0;
        bool gap = false;
        while (percent < 1.0) {
            Cloth clothe = clothes[num3 % clothes.Length];
            percent += (gap ? clothe.Length : (float)clothe.Step) / dist;
            Vector2 next = curve.GetPoint(percent);
            Draw.Line(current, next, lineColor);
            if (percent < 1.0 & gap) {
                float num4 = clothe.Length * ClothDroopAmount;
                SimpleCurve simpleCurve = new SimpleCurve(current, next, (current + next) / 2f + new Vector2(0.0f, num4 + (float)(Math.Sin(waveTimer * 2.0 + percent) * num4 * 0.4)));
                Vector2 vector2_2 = current;
                for (float num5 = 1f; num5 <= (double)clothe.Length; ++num5) {
                    Vector2 point2 = simpleCurve.GetPoint(num5 / clothe.Length);
                    if (point2.X != (double)vector2_2.X) {
                        Draw.Rect(vector2_2.X, vector2_2.Y, (float)(point2.X - (double)vector2_2.X + 1.0), clothe.Height, colors[clothe.Color]);
                        vector2_2 = point2;
                    }
                }

                Draw.Rect(current.X, current.Y, 1f, clothe.Height, highlights[clothe.Color]);
                Draw.Rect(next.X, next.Y, 1f, clothe.Height, highlights[clothe.Color]);
                Draw.Rect(current.X, current.Y - 1f, 1f, 3f, pinColor);
                Draw.Rect(next.X, next.Y - 1f, 1f, 3f, pinColor);
                ++num3;
            }

            current = next;
            gap = !gap;
        }
    }

    private bool IsVisible(SimpleCurve curve) {
        float num1 = 0.0f;
        foreach (var cloth in clothes) {
            float num2 = cloth.Height + (float)(cloth.Length * (double)ClothDroopAmount * 1.399999976158142);
            if (num2 > (double)num1)
                num1 = num2;
        }

        return CullHelper.IsCurveVisible(curve, num1 + 8f);
    }

    private struct Cloth {
        public int Color;
        public int Height;
        public int Length;
        public int Step;
    }
}
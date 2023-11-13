using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.UI.Controls;

public class UIColorPicker : UIElement {
    private readonly int wheelWidth;
    private int svWidth, svHeight;
    private int colorPreviewSize;
    private bool alphaWheel;

    private readonly UITextField hexTextField;
    private bool hueEdit, svEdit, alphaEdit;

    public Action<Color, float> OnColorChange;
    public Color Value { get; private set; }
    private float h, s, v, a;

    public UIColorPicker(Color color = default, float? alpha = null, int svWidth = 100, int svHeight = 80, int wheelWidth = 16, int colorPreviewSize = 12, bool alphaWheel = false) {
        this.wheelWidth = wheelWidth;
        this.svWidth = svWidth;
        this.svHeight = svHeight;
        this.colorPreviewSize = Math.Max(colorPreviewSize, Fonts.Regular.LineHeight);
        this.alphaWheel = alphaWheel;
        Width = svWidth + wheelWidth - 2;
        Height = svHeight + colorPreviewSize - 2;

        if (alphaWheel)
            Width += wheelWidth - 2;

        Add(hexTextField = new UITextField(Fonts.Regular, 36) {
            Position = new Vector2(Width / 2 - 18, svHeight - 1),
            Line = Color.Transparent,
            LineSelected = Color.Transparent,
            BG = Color.Transparent,
            BGSelected = Color.Transparent
        });

        a = alpha ?? color.A / 255f;
        SetColor(color);
        HSV(color, out h, out s, out v);
        GrabsClick = true;
    }

    public void SetColor(Color c) {
        Value = c;
        bool showAlpha = a < 1 || alphaWheel;
        hexTextField.UpdateInput(showAlpha ? $"#{Value.IntoRgbString()}{((byte)(a * 255f)).ToHex()}" : $"#{Value.IntoRgbString()}");
    }

    public override void Update(Vector2 position = default) {
        base.Update(position);

        int mouseX = (int)Mouse.Screen.X;
        int mouseY = (int)Mouse.Screen.Y;
        Point mouseP = Mouse.Screen.ToPoint();
        Rectangle svRect = new Rectangle((int)position.X + 1, (int)position.Y + 1, svWidth - 2, svHeight - 2);
        Rectangle wheelRect = new Rectangle((int)position.X + svWidth + 1, (int)position.Y + 1, wheelWidth - 3, svHeight - 2);
        Rectangle alphaRect = alphaWheel ? new Rectangle(wheelRect.Right, (int)(position.Y + 1), wheelWidth - 3, svHeight - 2) : Rectangle.Empty;

        if (MInput.Mouse.CheckLeftButton) {
            if (svRect.Contains(mouseP) && ConsumeLeftClick())
                svEdit = true;
            else if (wheelRect.Contains(mouseP) && ConsumeLeftClick())
                hueEdit = true;
            else if (alphaRect.Contains(mouseP) && ConsumeLeftClick())
                alphaEdit = true;

            if (svEdit || hueEdit || alphaEdit) {
                if (svEdit) {
                    s = Calc.Clamp(mouseX - position.X, 0, svWidth) / svWidth;
                    v = 1 - Calc.Clamp(mouseY - position.Y, 0, svHeight) / svHeight;
                } else if (hueEdit)
                    h = Calc.Clamp(mouseY - position.Y, 0, svHeight) / svHeight;
                else if (alphaEdit)
                    a = Calc.Clamp(svHeight - (mouseY - position.Y), 0, svHeight) / svHeight;

                SetColor(Calc.HsvToColor(h, s, v));
                OnColorChange?.Invoke(Value, a);
            }
        } else if (MInput.Mouse.ReleasedLeftButton)
            svEdit = hueEdit = alphaEdit = false;
    }

    public override void Render(Vector2 position = default) {
        Draw.Rect(position + new Vector2(svWidth, 0), wheelWidth - 3, svHeight, Color.Black);
        Draw.Rect(position + Vector2.UnitX, svWidth - 2, svHeight, Color.Black);
        Draw.Rect(position + Vector2.UnitY, svWidth + wheelWidth - 2, svHeight - 2, Color.Black);
        Draw.HollowRect(position + new Vector2(0, svHeight - 1), Width, colorPreviewSize - 1, Color.Black);
        if (alphaWheel)
            Draw.HollowRect(position + new Vector2(svWidth + wheelWidth - 3, 0), wheelWidth - 1, svHeight, Color.Black);

        float w = svWidth - 2;
        float h = svHeight - 2;
        for (int x = 1; x <= w; x++)
            for (int y = 1; y <= h; y++)
                Draw.Point(position + new Vector2(x, y), Calc.HsvToColor(this.h, x / w, 1 - y / h));

        int wheel = wheelWidth - 3;
        for (int i = 1; i <= h; i++)
            Draw.Rect(position + new Vector2(svWidth, i), wheel, 1, Calc.HsvToColor(i / h, 1, 1));

        if (alphaWheel)
            for (int i = 1; i <= h; i++)
                Draw.Rect(position + new Vector2(svWidth + wheelWidth - 2, h - i + 1), wheel, 1, Color.White * (i / h));

        int hueX = (int)position.X + svWidth;
        int hueY = (int)position.Y + (int)(this.h * (svHeight - 3)) + 1;
        Draw.Rect(hueX, hueY - 1, wheel, 1, Color.Black);
        Draw.Rect(hueX, hueY + 1, wheel, 1, Color.Black);

        if (alphaWheel) {
            int alphaX = (int)position.X + svWidth + wheelWidth - 2;
            int alphaY = (int)position.Y + (int)((1 - a) * (svHeight - 3)) + 1;
            Draw.Rect(alphaX, alphaY - 1, wheel, 1, Color.Black);
            Draw.Rect(alphaX, alphaY + 1, wheel, 1, Color.Black);
        }

        Vector2 svPos = position + new Vector2(1 + s * (svWidth - 3), 1 + (1 - v) * (svHeight - 3));
        Draw.Point(svPos - Vector2.UnitX, Color.Black);
        Draw.Point(svPos + Vector2.UnitX, Color.Black);
        Draw.Point(svPos - Vector2.UnitY, Color.Black);
        Draw.Point(svPos + Vector2.UnitY, Color.Black);

        Draw.Rect(position + new Vector2(1, svHeight), Width - 2, colorPreviewSize - 3, Value * a);

        base.Render(position);
    }

    private static void HSV(Color color, out float hue, out float saturation, out float value) {
        float r = (float)color.R / 255;
        float g = (float)color.G / 255;
        float b = (float)color.B / 255;

        float min = Math.Min(Math.Min(r, g), b);
        float max = Math.Max(Math.Max(r, g), b);
        float d = max - min;

        hue = 0f;
        if (max == r)
            hue = 60f * ((g - b) / d % 6f);
        else if (max == g)
            hue = 60f * ((b - r) / d + 2f);
        else if (max == b)
            hue = 60f * ((r - g) / d + 4f);
        hue /= 360f;

        saturation = max == 0 ? 0 : d / max;
        value = max;
    }
}
using System;
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.UI;

public class UIButton : UIElement {
    private readonly Vector2 space, minSize;
    private string text;
    private Font font;
    private Action<Vector2, Color> icon;

    public static readonly Color DefaultFG = Calc.HexToColor("f0f0f0");
    public static readonly Color DefaultBG = Calc.HexToColor("1d1d21");
    public static readonly Color DefaultPressedFG = Calc.HexToColor("4e4ea3");
    public static readonly Color DefaultPressedBG = Calc.HexToColor("131317");
    public static readonly Color DefaultHoveredFG = Calc.HexToColor("f0f0f0");
    public static readonly Color DefaultHoveredBG = Calc.HexToColor("18181c");

    public Color FG = Calc.HexToColor("f0f0f0");
    public Color BG = Calc.HexToColor("1d1d21");
    public Color PressedFG = Calc.HexToColor("4e4ea3");
    public Color PressedBG = Calc.HexToColor("131317");
    public Color HoveredFG = Calc.HexToColor("f0f0f0");
    public Color HoveredBG = Calc.HexToColor("18181c");

    public String ButtonTooltip;
    public bool HasLeft = true, HasRight = true;

    private float lerp;
    protected bool pressed, hovering;

    private static readonly MTexture
        top,
        bottom,
        topFill,
        bottomFill,
        mid;
    public bool active = true; //Whether or not the Button is able to be pressed. Currently implemented poorly.
    public Action OnPress, OnRightPress;
    public bool Underline = false, Strikethrough = false;

    static UIButton(){
        MTexture full = GFX.Gui["Snowberry/button"];
        top = full.GetSubtexture(0, 0, 3, 4);
        topFill = full.GetSubtexture(2, 0, 1, 4);
        bottom = full.GetSubtexture(0, 5, 3, 3);
        bottomFill = full.GetSubtexture(2, 5, 1, 4);
        mid = full.GetSubtexture(0, 4, 2, 1);
    }

    private UIButton(int spaceX, int spaceY, int minWidth, int minHeight) {
        minSize = new Vector2(minWidth, minHeight);
        space = new Vector2(spaceX, spaceY);

        GrabsClick = true;
    }

    public UIButton(int width, int height, int spaceX = 0, int spaceY = 0, int minWidth = 6, int minHeight = 8)
        : this(spaceX, spaceY, minWidth, minHeight) {
        SetSize(Math.Max(6, width), Math.Max(8, height));
    }

    public UIButton(string text, Font font, int spaceX = 0, int spaceY = 0, int minWidth = 6, int minHeight = 8)
        : this(spaceX, spaceY, minWidth, minHeight) {
        SetText(text, font);
    }

    public UIButton(MTexture icon, int spaceX = 0, int spaceY = 0, int minWidth = 6, int minHeight = 8)
        : this(spaceX, spaceY, minWidth, minHeight) {
        SetIcon(icon);
        FG = PressedFG = HoveredFG = Color.White;
    }

    public UIButton(Action<Vector2, Color> action, int icoWidth, int icoHeight, int spaceX = 0, int spaceY = 0, int minWidth = 6, int minHeight = 8)
        : this(spaceX, spaceY, minWidth, minHeight) {
        SetIconAction(action, icoWidth, icoHeight);
        FG = PressedFG = HoveredFG = Color.White;
    }

    private void SetSize(int width, int height) {
        Width = (int)Math.Max(width + space.X * 2, Math.Max(6, minSize.X));
        Height = (int)Math.Max(height + space.Y * 2, Math.Max(8, minSize.Y));
    }

    public void SetText(string text, Font font = null, bool stayCentered = false) {
        Vector2 mid = Position + new Vector2(Width, Height) / 2f;
        icon = null;
        this.text = text;
        this.font = font ?? this.font;
        Vector2 size = this.font.Measure(this.text);
        SetSize((int)size.X + 6, (int)size.Y + 3);

        if (stayCentered)
            Position = (mid - new Vector2(Width, Height) / 2f).Round();
    }

    public void SetIcon(MTexture icon) {
        this.icon = (at, color) => icon.Draw(at, Vector2.Zero, color);
        SetSize(icon.Width + 6, icon.Height + 3);
    }

    public void SetIconAction(Action<Vector2, Color> action, int icoWidth, int icoHeight) {
        icon = action;
        SetSize(icoWidth + 6, icoHeight + 3);
    }

    public void ResetBgColors() {
        BG = DefaultBG;
        HoveredBG = DefaultHoveredBG;
        PressedBG = DefaultPressedBG;
    }

    public void ResetFgColors() {
        FG = DefaultFG;
        HoveredFG = DefaultHoveredFG;
        PressedFG = DefaultPressedFG;
    }

    public override void Update(Vector2 position = default) {
        base.Update();

        int mouseX = (int)Mouse.Screen.X;
        int mouseY = (int)Mouse.Screen.Y;
        if (active) {
            hovering = new Rectangle((int)position.X + 1, (int)position.Y + 1, Width - 2, Height - 2).Contains(mouseX, mouseY);

            if (hovering && (ConsumeLeftClick() || ConsumeAltClick()))
                pressed = true;
            else if (hovering && pressed) {
                if (ConsumeAltClick(pressed: false, released: true)) {
                    OnRightPress?.Invoke();
                    pressed = false;
                } else if (ConsumeLeftClick(pressed: false, released: true)) {
                    Pressed();
                    pressed = false;
                }
            }

            if (MInput.Mouse.ReleasedLeftButton || MInput.Mouse.ReleasedRightButton) {
                // we only actually activate if the mouse was released on this button + it's visible,
                // but we still need to make it visually unpress if it's dragged off and released
                pressed = false;
            }

            lerp = Calc.Approach(lerp, pressed ? 1f : 0f, Engine.DeltaTime * 20f);
        }
    }

    protected virtual void Pressed() {
        OnPress?.Invoke();
    }

    public override void Render(Vector2 position = default) {
        base.Render(position);

        Color curBg = Color.Lerp(hovering ? HoveredBG : BG, PressedBG, lerp);
        var press = DrawButtonBg(new((int)position.X, (int)position.Y, Width, Height), pressed, curBg, HasLeft, HasRight);

        Vector2 at = position + new Vector2(3 + space.X, press + space.Y);
        Color fg = Color.Lerp(hovering ? HoveredFG : FG, PressedFG, lerp);
        if (text != null && font != null) {
            font.Draw(text, at, Vector2.One, fg);
            Vector2 textArea = font.Measure(this.text);
            if (Underline)
                Draw.Rect(at + new Vector2(-2, textArea.Y), textArea.X + 4, 1, FG);
            if (Strikethrough)
                Draw.Rect(at + new Vector2(-2, textArea.Y / 2 + 1), textArea.X + 4, 1, Color.Lerp(FG, Color.Black, 0.25f));
        } else icon?.Invoke(at, fg);
    }

    public override string Tooltip() => ButtonTooltip;

    public static int DrawButtonBg(Rectangle bounds, bool pressed, Color color, bool hasLeft = true, bool hasRight = true){
        int press = pressed ? 1 : 0;
        int shl = (!hasLeft).Bit(), shr = (!hasRight).Bit() + shl;

        if (hasLeft)
            top.Draw(new Vector2(bounds.X, bounds.Y + press), Vector2.Zero, color);
        if (hasRight)
            top.Draw(new Vector2(bounds.X + bounds.Width, bounds.Y + press), Vector2.Zero, color, new Vector2(-1, 1));
        topFill.Draw(new Vector2(bounds.X + 3 - shl*3, bounds.Y + press), Vector2.Zero, color, new Vector2(bounds.Width - 6 + shr*3, 1));

        if (hasLeft)
            mid.Draw(new Vector2(bounds.X, bounds.Y + bounds.Height - 4), Vector2.Zero, color);
        if (hasRight)
            mid.Draw(new Vector2(bounds.X + bounds.Width, bounds.Y + bounds.Height - 4), Vector2.Zero, color, new Vector2(-1, 1));
        Draw.Rect(new Vector2(bounds.X + shl*3, bounds.Y + 4 + press), bounds.Width - shr*3, bounds.Height - 8, Color.Black);
        Draw.Rect(new Vector2(bounds.X + 1 - shl, bounds.Y + 4 + press), bounds.Width - 2 + shr, bounds.Height - 8, color);

        if (hasLeft)
            bottom.Draw(new Vector2(bounds.X, bounds.Y + bounds.Height - 3), Vector2.Zero, color);
        if (hasRight)
            bottom.Draw(new Vector2(bounds.X + bounds.Width, bounds.Y + bounds.Height - 3), Vector2.Zero, color, new Vector2(-1, 1));
        bottomFill.Draw(new Vector2(bounds.X + 3 - shl*3, bounds.Y + bounds.Height - 3), Vector2.Zero, color, new Vector2(bounds.Width - 6 + shr*3, 1));

        Draw.Rect(new Vector2(bounds.X + 2 - shl*2, bounds.Y + bounds.Height - 4 + press), bounds.Width - 4 + shr*2, 1, color);
        return press;
    }

    public static void Group(List<UIButton> buttons) {
        for (var idx = 0; idx < buttons.Count; idx++) {
            var b = buttons[idx];
            if (idx > 0) b.HasLeft = false;
            if (idx < buttons.Count - 1) b.HasRight = false;
        }
    }
}
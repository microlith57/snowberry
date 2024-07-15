using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Snowberry.UI.Layout;

public class UIScrollPane : UICutoutElement {
    public int BottomPadding = 0, TopPadding = 0;
    public bool ShowScrollBar = true;
    public bool Vertical = true;
    public float Scroll = 0;

    public UIScrollPane() {
        Background = Calc.HexToColor("202929") * (185 / 255f);
        GrabsScroll = true;
        GrabsClick = true;
    }

    public override void Render(Vector2 position = default) {
        base.Render(position);

        if (ShowScrollBar) {
            var hilo = HighLow();
            float minScroll = Max - hilo.lo, maxScroll = -hilo.hi;
            if (minScroll < maxScroll) { // otherwise, we can't scroll at all
                float offscreen = maxScroll - minScroll;
                // we would like to keep the blank area = offscreen area, so dragging appears to linearly move things
                // until we get to very small sizes, then we need to just rely on a minimum
                float thumbSize = Math.Max(Max - offscreen, 12);
                if (Vertical) {
                    Draw.Rect(position + new Vector2(Width - 4, (Height - thumbSize) * (1 - (Scroll - minScroll) / offscreen)), 3, thumbSize, Color.DarkCyan * 0.35f);
                } else {
                    Draw.Rect(position + new Vector2((Width - thumbSize) * (1 - (Scroll - minScroll) / offscreen), Height - 4), thumbSize, 3, Color.DarkCyan * 0.35f);
                }
            }
        }
    }

    protected override Vector2 ContentOffset() => (Vertical ? Vector2.UnitY : Vector2.UnitX) * Scroll;

    public override void Update(Vector2 position = default) {
        base.Update(position);

        if (Mouse.IsFocused && ViewBounds(position).Contains(Mouse.Screen.ToPoint())) {
            ScrollBy(MInput.Mouse.WheelDelta);

            if (!UIScene.Instance.UI.NestedGrabsKeyboard()) {
                if (MInput.Keyboard.Pressed(Keys.Down)) ScrollBy(-30);
                if (MInput.Keyboard.Pressed(Keys.Up)) ScrollBy(30);
                if (MInput.Keyboard.Pressed(Keys.PageDown)) ScrollBy(-150);
                if (MInput.Keyboard.Pressed(Keys.PageUp)) ScrollBy(150);
                if (MInput.Keyboard.Pressed(Keys.Home)) ClampToStart();
                if (MInput.Keyboard.Pressed(Keys.End)) ClampToEnd();
            }
        }

        // TODO: make optional? or into UIElement behaviour?
        // fit to parent's height if not set, like for the tile brush panel
        Height = Height == 0 ? Parent?.Height ?? 0 : Height;
    }

    public override Vector2 BoundsOffset() => ContentOffset();

    private float Max => Vertical ? Height : Width;
    private float Magnitude(UIElement e) => Vertical ? e.Position.Y : e.Position.X;

    public void ScrollBy(float amount) {
        // note that Scroll is almost always negative, hence the maximum value is always negative unless some element has negative coords
        var hilo = HighLow();
        Scroll += amount;
        Scroll = Clamp(Scroll, Max - hilo.lo, -hilo.hi);
    }

    public (float hi, float lo) HighLow() {
        UIElement low = null, high = null;
        foreach(var item in Children.Where(item => item.Visible)){
            if (low == null || Magnitude(item) > Magnitude(low)) low = item;
            if (high == null || Magnitude(item) < Magnitude(high)) high = item;
        }

        return (high != null ? Magnitude(high) - TopPadding : 0, low != null ? Magnitude(low) + (Vertical ? low.Height : low.Width) + BottomPadding : 0);
    }

    public void ClampToEnd() {
        var hilo = HighLow();
        Scroll = Clamp(Max - hilo.lo, Max - hilo.lo, -hilo.hi);
    }

    public void ClampToStart() {
        var hilo = HighLow();
        Scroll = Clamp(-hilo.hi, Max - hilo.lo, -hilo.hi);
    }

    // MathHelper.clamp, but we check constraints the other way around,
    // so scrollpanes with few elements get stuck to the top instead of bottom
    public static float Clamp(float value, float min, float max) {
        value = value < min ? min : value;
        value = value > max ? max : value;
        return value;
    }
}
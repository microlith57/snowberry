using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.UI.Controls;

public class UISlider : UIElement {

    public float Value;
    public float Min, Max;
    public int HandleWidth, HandleHeight;
    public Action<float> OnInputChanged = null;

    protected float lerp;
    protected bool pressed, hovering;

    public UISlider() {
        Width = 80;
        Height = 20;
        HandleWidth = 10;
        HandleHeight = 20;
    }

    public override void Update(Vector2 position = default) {
        base.Update(position);

        Rectangle handle = HandleRect();
        hovering = handle.Contains(Mouse.Screen.ToPoint());

        if (hovering && ConsumeLeftClick())
            pressed = true;
        if (MInput.Mouse.ReleasedLeftButton)
            pressed = false;

        if (pressed) {
            var oldValue = Value;
            // min + variance * percentage
            Value = MathHelper.Clamp(Min + (Max - Min) * ((Mouse.Screen.X - position.X) / Width), Min, Max);
            if (oldValue != Value)
                OnInputChanged?.Invoke(Value);
        }

        lerp = Calc.Approach(lerp, pressed ? 1f : 0f, Engine.DeltaTime * 20f);
    }

    public override void Render(Vector2 position = default) {
        base.Render(position);
        Draw.Line(position + new Vector2(0, Height / 2f), position + new Vector2(Width, Height / 2f), UIButton.DefaultFG);

        Rectangle handle = HandleRect();
        Color curBg = Color.Lerp(hovering ? UIButton.DefaultHoveredBG : UIButton.DefaultBG, UIButton.DefaultPressedBG, lerp);
        UIButton.DrawButtonBg(handle, pressed, curBg);
    }

    protected Rectangle HandleRect() => new(Bounds.X + (int)(Percent * Width - HandleWidth / 2f), Bounds.Y, HandleWidth, HandleHeight);
    protected float Percent => (Value - Min) / (Max - Min);
}
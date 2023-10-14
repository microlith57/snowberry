using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.UI.Controls;

// it's like UISlider but for music
public class UIKnob : UIElement {

    public float Value;
    public float Min, Max;

    public UIKnob() {
        Width = Height = 30;
        GrabsScroll = true;
    }

    public override void Render(Vector2 position = default) {
        base.Render(position);

        const float ang = MathHelper.Pi + MathHelper.PiOver2;
        const float start = MathHelper.Pi - (ang - MathHelper.Pi) / 2;

        Draw.Circle(position, 12, Color.White, 11);
        DrawArc(position, 14, start,  Percent * ang, Color.White, 13);
        Draw.Point(position + Calc.AngleToVector(start + Percent * ang, 8), Color.White);
    }

    private static void DrawArc(
        Vector2 position,
        float radius,
        float startAngle,
        float totalAngle,
        Color color,
        int resolution,
        float thickness = 1
    ) {
        Vector2 right = Calc.AngleToVector(startAngle, radius);
        for (int index = 1; index <= resolution; ++index) {
            Vector2 nextRight = Calc.AngleToVector(startAngle + index * totalAngle / resolution, radius);
            Draw.Line(position + right, position + nextRight, color, thickness);
            right = nextRight;
        }
    }

    public override void Update(Vector2 position = default) {
        base.Update(position);

        if (Bounds.Contains(Mouse.Screen.ToPoint())) {
            int v = MInput.Mouse.WheelDelta;
            if (v != 0) {
                Value += v * (Max - Min) / 500;
                Value = MathHelper.Clamp(Value, Min, Max);
            }
        }
    }

    protected float Percent => (Value - Min) / (Max - Min);
}
using Microsoft.Xna.Framework;
using Monocle;
using Snowberry.UI.Controls;

namespace Snowberry.UI.Layout;

// it's like if ImGui looked good
public class UIPopOut : UICutoutElement {

    // this class is Pure Evil because it changes its position as it likes
    // ofc course, every actual pop-out is constrained to some area,
    public Rectangle Constraint;

    public int TopPadding = 14;
    public int SidePadding = 3;

    public string Title = "Pop-out";

    private bool dragging = false;

    public override void Update(Vector2 position = default) {
        base.Update(position);
        if (!Active)
            return;

        if (MInput.Mouse.PressedLeftButton
            && new Rectangle((int)position.X, (int)position.Y, Width, TopPadding).Contains(Mouse.Screen.ToPoint()))
            dragging = true;

        if (MInput.Mouse.ReleasedLeftButton)
            dragging = false;

        if (dragging)
            Position += Mouse.Screen - Mouse.ScreenLast;
    }

    public override void Render(Vector2 position = default) {
        UIButton.DrawButtonBg(new Rectangle((int)position.X, (int)position.Y, Width, Height), false, UIButton.DefaultBG);
        Fonts.Regular.Draw(Title, position + new Vector2(4, 1), Vector2.One, Color.White);
        base.Render(position);
    }

    public int ContentWidth => Width - SidePadding * 2;
    public int ContentHeight => Height - TopPadding - SidePadding;

    private Vector2 ContentStart() => new(SidePadding, TopPadding);

    // TODO: a
    protected override Vector2 ContentOffset() => ContentStart();
    public override Vector2 BoundsOffset() => ContentStart();

    protected override Rectangle ViewBounds(Vector2 position) =>
        new((int)position.X + SidePadding, (int)position.Y + TopPadding, Width - SidePadding * 2, Height - TopPadding - SidePadding);
}
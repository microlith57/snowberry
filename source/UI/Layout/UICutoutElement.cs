using System.Linq;
using Microsoft.Xna.Framework;

namespace Snowberry.UI.Layout;

// a UI element that only shows children within it's bounds, and prevents mouse interaction outside
public class UICutoutElement : UIElement{

    public UICutoutElement() {
        RenderChildren = false;
    }

    public override void Render(Vector2 position = default) {
        // note that we're responsible for drawing *all* children, but not e.g. backgrounds or bounds
        base.Render(position);

        Rectangle bounds = ViewBounds(position);
        Vector2 contentOffset = ContentOffset();
        DrawUtil.WithinScissorRectangle(bounds, () => {
            foreach (var element in Children.Where(element => element.Visible && element.Bounds.Intersects(bounds)))
                element.Render(position + element.Position + contentOffset);
        });
    }

    public override void Update(Vector2 position = default) {
        if (UIScene.Instance is not { /* non-null */ } scene) {
            base.Update(position);
            return;
        }

        bool mouseClicked = scene.MouseClicked;
        scene.MouseClicked = mouseClicked || !ViewBounds(position).Contains(Mouse.Screen.ToPoint());
        base.Update(position + ContentOffset());
        scene.MouseClicked = mouseClicked;
    }

    protected virtual Rectangle ViewBounds(Vector2 position) => new((int)position.X, (int)position.Y, Width, Height);
    protected virtual Vector2 ContentOffset() => Vector2.Zero;
}
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.UI;

// Evil element that controls its own layouting
public class UITree : UIElement {
    public float PadLeft = 5, PadRight = 5, PadUp = 5, PadDown = 5;
    public float Spacing = 5;

    public bool Collapsed = false;
    protected UIElement Header;

    public UITree(UIElement header) : this(header, new(5, 3)) {}

    public UITree(UIElement header, Vector2 headerOffset) {
        RenderChildren = false;

        Header = new UIElement();
        UIButton b = null;
        Header.Add(b = new UIButton("\u2193", Fonts.Regular, 2, 2) {
            OnPress = () => {
                Collapsed = !Collapsed;
                b.SetText(Collapsed ? "\u2192" : "\u2193");
                Layout();
            }
        });
        Header.AddRight(header, headerOffset);
        Header.CalculateBounds();
        Add(Header);
    }

    public override void Update(Vector2 position = default) {
        if (UIScene.Instance == null) {
            base.Update(position); return;
        }

        // relies on Relayout sending hidden entities to 9999,9999 but that's like fine yknow
        bool mouseClicked = UIScene.Instance.MouseClicked;
        UIScene.Instance.MouseClicked = !Bounds.Contains(Mouse.Screen.ToPoint()) || mouseClicked;
        base.Update(position);
        UIScene.Instance.MouseClicked = mouseClicked;
    }

    public override void Render(Vector2 position = default) {
        // note that we're responsible for drawing *all* children, but not e.g. backgrounds or bounds
        base.Render(position);

        if (Collapsed)
            Header.Render(position + Header.Position);
        else {
            foreach (var element in Children.Where(element => element.Visible))
                element.Render(position + element.Position);
        }
        Draw.Rect(position + new Vector2(0, PadUp), 1, Height - PadUp - PadDown, Color.White);
        Draw.Rect(position + new Vector2(1, PadUp + 1), 1, Height - PadUp - PadDown - 2, Color.Gray);
    }

    public void Layout() {
        if (Collapsed) {
            foreach (UIElement e in Children) // just make sure they're not inside our bounds during Update
                e.Position = new(9999);
            Header.Position = new(PadLeft, PadUp);
            Width = (int)((Header.Position.X + Header.Width));
            Height = (int)((Header.Position.Y + Header.Height));
        } else {
            int i = 0;
            foreach (UIElement e in Children.Except(toRemove)) {
                e.Position = new(PadLeft, PadUp + i);
                i += (int)(e.Height + Spacing);
            }
            CalculateBounds();
        }
        Width += (int)PadRight;
        Height += (int)PadDown;
        (Parent as UITree)?.Layout();
    }
}
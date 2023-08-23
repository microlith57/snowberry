using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using Snowberry.Editor.UI;
using System;
using System.Linq;
using Snowberry.Editor.UI.Menus;

namespace Snowberry.Editor.Tools;

public class SelectionTool : Tool {
    private static bool canSelect;
    private static bool selectEntities = true, selectTriggers = true;
    private static UIEntitySelection selectionPanel;

    // entity resizing
    private static bool resizingX, resizingY, fromLeft, fromTop;
    private static Rectangle oldEntityBounds;

    public override string GetName(){
        return Dialog.Clean("SNOWBERRY_EDITOR_TOOL_ENTITYSELECT");
    }

    public override UIElement CreatePanel(int height) {
        UIElement panel = new UIElement{
            Width = 200,
            Background = Calc.HexToColor("202929") * (185 / 255f),
            GrabsClick = true,
            GrabsScroll = true,
            Height = height
        };

        panel.Add(selectionPanel = new UIEntitySelection{
            Width = 200,
            Height = height - 30,
            Background = null
        });

        UIElement filtersPanel = new UIElement{
            Position = new Vector2(5, height - 20)
        };
        filtersPanel.AddRight(UIPluginOptionList.BoolOption("entities", selectEntities, s => selectEntities = s));
        filtersPanel.AddRight(UIPluginOptionList.BoolOption("triggers", selectTriggers, s => selectTriggers = s), new Vector2(10, 0));
        panel.Add(filtersPanel);

        return panel;
    }

    public override void Update(bool canClick) {
        var editor = Editor.Instance;

        if (MInput.Mouse.CheckLeftButton && canClick) {
            Point mouse = new Point((int)Editor.Mouse.World.X, (int)Editor.Mouse.World.Y);
            Vector2 world = Editor.Mouse.World;
            if (MInput.Mouse.PressedLeftButton) {
                canSelect = !(Editor.SelectedEntities != null && Editor.SelectedEntities.Any(s => s.Contains(mouse)));
            }

            if (canSelect && Editor.SelectedRoom != null) {
                int ax = (int)Math.Min(Editor.Mouse.World.X, editor.worldClick.X);
                int ay = (int)Math.Min(Editor.Mouse.World.Y, editor.worldClick.Y);
                int bx = (int)Math.Max(Editor.Mouse.World.X, editor.worldClick.X);
                int by = (int)Math.Max(Editor.Mouse.World.Y, editor.worldClick.Y);
                Editor.Selection = new Rectangle(ax, ay, bx - ax, by - ay);

                Editor.SelectedEntities = Editor.SelectedRoom.GetSelectedEntities(Editor.Selection.Value, selectEntities, selectTriggers);
            } else if (Editor.SelectedEntities != null) {
                // if only one entity is selected near the corners, resize
                Entity solo = GetSoloEntity();
                if (solo != null) {
                    if(MInput.Mouse.PressedLeftButton) {
                        // TODO: can this be shared between RoomTool & SelectionTool?
                        fromLeft = Math.Abs(Editor.Mouse.World.X - solo.Position.X) <= 4;
                        resizingX = Math.Abs(Editor.Mouse.World.X - (solo.Position.X + solo.Width)) <= 4 || fromLeft;
                        fromTop = Math.Abs(Editor.Mouse.World.Y - solo.Position.Y) <= 4;
                        resizingY = Math.Abs(Editor.Mouse.World.Y - (solo.Position.Y + solo.Height)) <= 4 || fromTop;
                        oldEntityBounds = solo.Bounds;
                    } else if (resizingX || resizingY) {
                        var wSnapped = (Editor.Mouse.World / 8).Round() * 8;
                        if (resizingX) {
                            // compare against the opposite edge
                            solo.SetWidth(Math.Max((int)Math.Round((fromLeft ? oldEntityBounds.Right - world.X : world.X - solo.X) / 8f) * 8, solo.MinWidth));
                            if (fromLeft)
                                solo.SetPosition(new((int)Math.Floor(wSnapped.X), solo.Y));
                        }

                        if (resizingY) {
                            solo.SetHeight(Math.Max((int)Math.Round((fromTop ? oldEntityBounds.Bottom - world.Y : world.Y - solo.Y) / 8f) * 8, solo.MinHeight));
                            if (fromTop)
                                solo.SetPosition(new(solo.X, (int)Math.Floor(wSnapped.Y)));
                        }

                        // skip dragging code
                        goto postResize;
                    }
                }

                // otherwise, move
                bool noSnap = MInput.Keyboard.Check(Keys.LeftControl) || MInput.Keyboard.Check(Keys.RightControl);
                Vector2 worldSnapped = noSnap ? Editor.Mouse.World : (Editor.Mouse.World / 8).Round() * 8;
                Vector2 worldLastSnapped = noSnap ? Editor.Mouse.WorldLast : (Editor.Mouse.WorldLast / 8).Round() * 8;
                Vector2 move = worldSnapped - worldLastSnapped;
                foreach (EntitySelection s in Editor.SelectedEntities)
                    s.Move(move);
            }
        } else
            Editor.Selection = null;

        if (Editor.SelectedEntities == null)
            return;

        postResize:
        bool refreshPanel = false;
        if (Editor.Instance.CanTypeShortcut()) {
            if (MInput.Keyboard.Check(Keys.Delete)) {
                foreach (var item in Editor.SelectedEntities) {
                    refreshPanel = true;
                    item.Entity.Room.RemoveEntity(item.Entity);
                }

                Editor.SelectedEntities.Clear();
            } else if (MInput.Keyboard.Pressed(Keys.N)) {
                // iterate backwards to allow modifying the list as we go
                for (var idx = Editor.SelectedEntities.Count - 1; idx >= 0; idx--) {
                    var item = Editor.SelectedEntities[idx];
                    var e = item.Entity;
                    if (e.Nodes.Count < e.MaxNodes || e.MaxNodes == -1) {
                        int oldIdx = item.Selections[0].Index;
                        int newNodeIdx = oldIdx + 1;
                        Vector2 oldPos = oldIdx == -1 ? e.Position : e.Nodes[oldIdx];
                        e.AddNode(oldPos + new Vector2(24, 0), newNodeIdx);
                        Editor.SelectedEntities.Remove(item);
                        Editor.SelectedEntities.Add(new EntitySelection(e, new() { new(e, newNodeIdx) }));
                    }
                }
            } else if (MInput.Keyboard.Pressed(Keys.Escape)) {
                if (Editor.SelectedEntities.Count > 0)
                    refreshPanel = true;
                Editor.SelectedEntities.Clear();
            }
        }

        if ((MInput.Mouse.ReleasedLeftButton && canClick && canSelect) || refreshPanel)
            selectionPanel?.Display(Editor.SelectedEntities);
    }

    public override void RenderWorldSpace() {
        base.RenderWorldSpace();
        if (Editor.SelectedRoom != null)
            foreach (var item in Editor.SelectedRoom.GetSelectedEntities(new Rectangle((int)Editor.Mouse.World.X, (int)Editor.Mouse.World.Y, 0, 0), selectEntities, selectTriggers))
                if (Editor.SelectedEntities == null || !Editor.SelectedEntities.Contains(item))
                    foreach (var s in item.Selections)
                        Draw.Rect(s.Rect, Color.Blue * 0.15f);
    }

    public override void SuggestCursor(ref MTexture cursor, ref Vector2 justify) {
        Point mouse = new Point((int)Editor.Mouse.World.X, (int)Editor.Mouse.World.Y);

        // only have 1 entity selected & at the borders? show resizing tooltips
        Entity solo = GetSoloEntity();
        if(solo != null){
            var fromLeft = Math.Abs(Editor.Mouse.World.X - solo.Position.X) <= 4;
            var fromRight = Math.Abs(Editor.Mouse.World.X - (solo.Position.X + solo.Width)) <= 4;
            var fromTop = Math.Abs(Editor.Mouse.World.Y - solo.Position.Y) <= 4;
            var fromBottom = Math.Abs(Editor.Mouse.World.Y - (solo.Position.Y + solo.Height)) <= 4;
            if (fromLeft || fromRight || fromTop || fromBottom) {
                justify = Vector2.One / 2f;

                if ((fromBottom && fromLeft) || (fromTop && fromRight)) {
                    cursor = Editor.cursors.GetSubtexture(32, 32, 16, 16);
                    return;
                }

                if ((fromTop && fromLeft) || (fromBottom && fromRight)) {
                    cursor = Editor.cursors.GetSubtexture(48, 32, 16, 16);
                    return;
                }

                if (fromLeft || fromRight) {
                    cursor = Editor.cursors.GetSubtexture(0, 32, 16, 16);
                    return;
                }

                if (fromBottom || fromTop) {
                    cursor = Editor.cursors.GetSubtexture(16, 32, 16, 16);
                    return;
                }
            }
        }

        // hovering over a selected entity? movement arrow
        if (Editor.SelectedEntities != null && Editor.SelectedEntities.Any(s => s.Contains(mouse))) {
            justify = Vector2.One / 2f;
            cursor = Editor.cursors.GetSubtexture(16, 16, 16, 16);
        }
    }

    private static Entity GetSoloEntity() {
        // list patterns would be nice here...
        if (Editor.SelectedEntities != null && Editor.SelectedEntities.Count == 1) {
            EntitySelection selection = Editor.SelectedEntities[0];
            if (selection.Selections.Count == 1) {
                var rect = selection.Selections[0];
                if (rect.Index == -1)
                    return rect.Entity;
            }
        }

        return null;
    }
}
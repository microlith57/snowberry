using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using Snowberry.Editor.UI;
using System;
using Snowberry.Editor.UI.Menus;

namespace Snowberry.Editor.Tools;

public class SelectionTool : Tool {
    private static bool canSelect;
    private static bool selectEntities = true, selectTriggers = true;
    private static UIEntitySelection selectionPanel;

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
            if (MInput.Mouse.PressedLeftButton) {
                Point mouse = new Point((int)Editor.Mouse.World.X, (int)Editor.Mouse.World.Y);

                canSelect = true;
                if (Editor.SelectedEntities != null) {
                    foreach (EntitySelection s in Editor.SelectedEntities) {
                        if (s.Contains(mouse)) {
                            canSelect = false;
                            break;
                        }
                    }
                }
            }

            if (canSelect && Editor.SelectedRoom != null) {
                int ax = (int)Math.Min(Editor.Mouse.World.X, editor.worldClick.X);
                int ay = (int)Math.Min(Editor.Mouse.World.Y, editor.worldClick.Y);
                int bx = (int)Math.Max(Editor.Mouse.World.X, editor.worldClick.X);
                int by = (int)Math.Max(Editor.Mouse.World.Y, editor.worldClick.Y);
                Editor.Selection = new Rectangle(ax, ay, bx - ax, by - ay);

                Editor.SelectedEntities = Editor.SelectedRoom.GetSelectedEntities(Editor.Selection.Value, selectEntities, selectTriggers);
            } else if (Editor.SelectedEntities != null) {
                bool noSnap = (MInput.Keyboard.Check(Keys.LeftControl) || MInput.Keyboard.Check(Keys.RightControl));
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
}
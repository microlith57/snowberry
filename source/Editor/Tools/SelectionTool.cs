using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Celeste.Mod;
using Snowberry.UI;
using Snowberry.UI.Controls;
using Snowberry.UI.Menus;

namespace Snowberry.Editor.Tools;

public class SelectionTool : Tool {
    public static readonly MTexture SelectionAtlas = GFX.Gui["Snowberry/selections"];

    private static bool canSelect;
    private static bool selectEntities = true, selectTriggers = true, selectFgDecals = false, selectBgDecals = false, selectFgTiles = false, selectBgTiles = false;
    private static UISelectionPane selectionPanel;
    private static List<UIButton> modeButtons = new(), toggleButtons = new();
    private static List<Selection> next = new();

    private static Rectangle? SelectionInProgress;
    private static List<Vector2> PathInProgress;

    // selection modes
    private enum SelectionEffect {
        Set, Add, Subtract
    }
    private enum SelectionMode {
        Rect, Line, Lasso, MagicWand
    }
    private static readonly List<Keys> ModeKeybinds = new() {
        Keys.R, Keys.L, Keys.S, Keys.W
    };

    private static SelectionMode currentMode = SelectionMode.Rect;

    // entity resizing
    private static bool resizingX, resizingY, fromLeft, fromTop;
    private static Rectangle oldEntityBounds;
    private const int resizeMargins = 2;

    // paste preview
    private static bool pasting = false;
    private static List<Entity> toPaste;

    // selection cycling
    private static bool movedMouse = false;

    // double click
    private static bool wasDoubleClick = false;

    // undoable selection moving
    // TODO: turn this whole thing into a damn state machine already
    private static bool MoveInProgress = false;

    public override string GetName() => Dialog.Clean("SNOWBERRY_EDITOR_TOOL_SELECT");

    public override UIElement CreatePanel(int height) {
        UIElement panel = new UIElement {
            Width = 210,
            Background = Calc.HexToColor("202929") * (185 / 255f),
            GrabsClick = true,
            GrabsScroll = true,
            Height = height
        };

        panel.Add(selectionPanel = new UISelectionPane {
            Width = 210,
            Background = null
        });

        return panel;
    }

    public override UIElement CreateActionBar() {
        modeButtons.Clear(); toggleButtons.Clear();
        UIElement p = new UIElement();
        Vector2 offset = new Vector2(0, 4);

        modeButtons.Clear();
        foreach (var mode in Enum.GetValues(typeof(SelectionMode))) {
            var button = new UIKeyboundButton(SelectionAtlas.GetSubtexture(16 * (int)mode, 0, 16, 16), 3, 3) {
                OnPress = () => currentMode = (SelectionMode)mode,
                ButtonTooltip = Dialog.Clean($"SNOWBERRY_EDITOR_SELECT_{mode.ToString().ToUpperInvariant()}_TT"),
                Key = ModeKeybinds[(int)mode]
            };
            p.AddRight(button, offset);
            modeButtons.Add(button);
        }
        UIButton.Group(modeButtons);

        p.AddRight(new(){ Width = 1 }, new(4, 0)); // world's best layouting

        p.AddRight(CreateToggleButton(0, 32, Keys.E, "ENTITIES", () => selectEntities, s => selectEntities = s), offset);
        p.AddRight(CreateToggleButton(32, 32, Keys.T, "TRIGGERS", () => selectTriggers, s => selectTriggers = s), offset);
        p.AddRight(CreateToggleButton(0, 48, Keys.F, "FG_DECALS", () => selectFgDecals, s => selectFgDecals = s), offset);
        p.AddRight(CreateToggleButton(32, 48, Keys.B, "BG_DECALS", () => selectBgDecals, s => selectBgDecals = s), offset);
        p.AddRight(CreateToggleButton(0, 64, Keys.H, "FG_TILES", () => selectFgTiles, s => selectFgTiles = s), offset);
        p.AddRight(CreateToggleButton(32, 64, Keys.J, "BG_TILES", () => selectBgTiles, s => selectBgTiles = s), offset);
        UIButton.Group(toggleButtons);

        return p;
    }

    private static UIButton CreateToggleButton(int icoX, int icoY, Keys toggleBind, string tooltipKey, Func<bool> value, Action<bool> onPress) {
        MTexture active = UIScene.ActionbarAtlas.GetSubtexture(icoX, icoY, 16, 16);
        MTexture inactive = UIScene.ActionbarAtlas.GetSubtexture(icoX + 16, icoY, 16, 16);
        UIKeyboundButton button = null; // to allow referring to it in OnPress
        button = new UIKeyboundButton(value() ? active : inactive, 3, 3) {
            OnPress = () => {
                onPress(!value());
                button.SetIcon(value() ? active : inactive);
            },
            Shift = true,
            Key = toggleBind,
            ButtonTooltip = Dialog.Clean($"SNOWBERRY_EDITOR_SELECT_{tooltipKey}_TT")
        };
        toggleButtons.Add(button);
        return button;
    }

    public override void Update(bool canClick) {
        var editor = Editor.Instance;
        bool refreshPanel = false;

        if (Editor.SelectedRoom == null && Editor.SelectedObjects.Count > 0) {
            // refreshPanel code gets skipped because there's no room
            Editor.SelectedObjects = new();
            selectionPanel?.Display(null);
        }

        for (int i = 0; i < modeButtons.Count; i++) {
            UIButton button = modeButtons[i];
            if (currentMode == (SelectionMode)i)
                button.BG = button.PressedBG = button.HoveredBG = Color.Gray;
            else
                button.ResetBgColors();
        }

        if (pasting) {
            AdjustPastedEntities();

            if (MInput.Keyboard.Released(Keys.V)) {
                if (Editor.SelectedRoom != null)
                    foreach (Entity e in toPaste) {
                        e.EntityID = PlacementTool.AllocateId();
                        Editor.SelectedRoom.AddEntity(e);
                    }

                pasting = false;
                toPaste.Clear();
            }
        }

        if (MInput.Mouse.CheckLeftButton && canClick) {
            Point mouse = new Point((int)Mouse.World.X, (int)Mouse.World.Y);
            Vector2 world = Mouse.World;

            if (MInput.Mouse.PressedLeftButton) {
                canSelect = !(Editor.SelectedObjects.Any(s => s.Contains(mouse)));
                movedMouse = false;
                wasDoubleClick = Mouse.IsDoubleClick;
            }

            bool justMovedMouse = false;
            if (Mouse.World != Mouse.WorldLast) {
                justMovedMouse = !movedMouse;
                movedMouse = true;
            }

            if (movedMouse && Editor.SelectedRoom != null)
                if (canSelect) {
                    if (currentMode == SelectionMode.Rect) {
                        int ax = (int)Math.Min(Mouse.World.X, editor.worldClick.X);
                        int ay = (int)Math.Min(Mouse.World.Y, editor.worldClick.Y);
                        int bx = (int)Math.Max(Mouse.World.X, editor.worldClick.X);
                        int by = (int)Math.Max(Mouse.World.Y, editor.worldClick.Y);
                        Rectangle r = new Rectangle(ax, ay, bx - ax, by - ay);
                        SelectionInProgress = r;
                        PathInProgress = null;
                        next = GetEnabledSelections(r);
                    } else {
                        // all other selection tools use paths
                        PathInProgress ??= new();
                        SelectionInProgress = null;
                        // add to the path if the mouse moves a lot
                        if (PathInProgress.Count == 0 || (PathInProgress.Last() - Mouse.World).LengthSquared() > 7 * 7)
                            PathInProgress.Add(Mouse.World);
                        if (currentMode == SelectionMode.Line) {
                            // note that we never need to remove anything in a line selection
                            foreach(var s in GetEnabledSelections(Mouse.World.ToRect()).Where(s => !next.Contains(s)))
                                next.Add(s);
                        } else if (currentMode == SelectionMode.Lasso) {
                            // accept any entities that are overlapped by the path or have a centre contained in it
                            // if the last point is too far from the first one, add points along the middle to make sure entities cut that way get selected
                            List<Vector2> path = PathInProgress;
                            Vector2 back = (path[0] - path.Last());
                            if (back.LengthSquared() > 7 * 7) {
                                path = new(path);
                                var numPts = back.Length() / 7;
                                for (int i = 0; i < numPts; i++)
                                    // note that path.Last() is different each time as we get closer to the start of the path
                                    path.Add(path.Last() + back / numPts);
                            }

                            // TODO: calculate covering rect for better performance?
                            //  esp with tile selections in large rooms
                            next = GetEnabledSelections(null)
                                .Where(x => path.Any(p => x.Contains(p.ToPoint())) || Util.PointInPolygon(x.Area().Center.ToVector2(), PathInProgress))
                                .ToList();
                        } else if (currentMode == SelectionMode.MagicWand) {
                            // note that we never need to remove anything in a magic wand selection
                            foreach(var s in MagicWand().Where(s => !next.Contains(s)))
                                next.Add(s);
                        }
                    }
                } else {
                    // if only one entity is selected near the corners, resize
                    Entity solo = GetSoloEntity();
                    if (solo != null) {
                        if (MInput.Mouse.PressedLeftButton || justMovedMouse) {
                            // TODO: can this be shared between RoomTool & SelectionTool?
                            fromLeft = Math.Abs(Mouse.World.X - solo.Position.X) <= resizeMargins;
                            resizingX = solo.MinWidth > -1 && (Math.Abs(Mouse.World.X - (solo.Position.X + solo.Width)) <= resizeMargins || fromLeft);
                            fromTop = Math.Abs(Mouse.World.Y - solo.Position.Y) <= resizeMargins;
                            resizingY = solo.MinHeight > -1 && (Math.Abs(Mouse.World.Y - (solo.Position.Y + solo.Height)) <= resizeMargins || fromTop);
                            oldEntityBounds = solo.Bounds;
                        } else if (resizingX || resizingY) {
                            var wSnapped = Mouse.World.RoundTo(8);
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

                            // TODO: don't snap offgrid entities while resizing, except with AggressiveSnap

                            // skip dragging code
                            goto postResize;
                        }
                    }

                    // otherwise, move
                    bool noSnap = MInput.Keyboard.Check(Keys.LeftControl) || MInput.Keyboard.Check(Keys.RightControl);
                    Vector2 worldSnapped = noSnap ? Mouse.World : Mouse.World.RoundTo(8);
                    Vector2 worldLastSnapped = noSnap ? Mouse.WorldLast : Mouse.WorldLast.RoundTo(8);
                    Vector2 move = worldSnapped - worldLastSnapped;
                    if (move != Vector2.Zero) {
                        if (!MoveInProgress) {
                            UndoRedo.BeginAction("move objects", Editor.SelectedObjects.Select(x => x.Snapshotter()));
                            MoveInProgress = true;
                        }

                        foreach (Selection s in Editor.SelectedObjects) {
                            s.Move(move);
                            SnapIfNecessary(s);
                        }

                        TileSelection.FinishMove();
                    }
                }
        } else {
            if (MInput.Mouse.ReleasedLeftButton && canClick && !movedMouse) {
                if (CurrentEffect != SelectionEffect.Set) {
                    movedMouse = true;
                    next = currentMode == SelectionMode.MagicWand ? MagicWand() : GetEnabledSelections(Mouse.World.ToRect());
                    SelectionInProgress ??= new();
                    goto applyNext;
                }

                // releasing double click on a selected entity -> select all of type
                if ((wasDoubleClick || currentMode == SelectionMode.MagicWand) && Editor.SelectedRoom != null)
                    Editor.SelectedObjects = MagicWand();
                else {
                    // releasing click on a selected object -> cycle selection
                    var at = GetEnabledSelections(Mouse.World.ToRect());
                    Selection first = Editor.SelectedObjects is { Count: > 0 } es ? es[0] : null;
                    int idx = at.IndexOf(first);
                    if (idx != -1)
                        Editor.SelectedObjects = new() { at[(idx + 1) % at.Count] };
                    // not hovering over something selected -> just take the first one
                    if (idx == -1 || first == null)
                        Editor.SelectedObjects = at.Count == 0 ? new() : new() { at[0] };
                }

                refreshPanel = true;
            }

            applyNext:
            if (movedMouse && (MInput.Mouse.ReleasedLeftButton || !canClick) && (((object)SelectionInProgress ?? PathInProgress) != null)) {
                Editor.SelectedObjects = CurrentEffect switch {
                    SelectionEffect.Add => Editor.SelectedObjects.Concat(next).Distinct().ToList(),
                    SelectionEffect.Subtract => Editor.SelectedObjects.Except(next).ToList(),
                    _ => new(next)
                };
                next.Clear();
            }

            SelectionInProgress = null;
            PathInProgress = null;

            if (MoveInProgress) {
                UndoRedo.CompleteAction();
                MoveInProgress = false;
            }
        }

        if (Editor.SelectedObjects == null)
            return;

        postResize:
        if (Editor.Instance.CanTypeShortcut()) {
            bool ctrl = MInput.Keyboard.Check(Keys.LeftControl) || MInput.Keyboard.Check(Keys.RightControl);
            if (MInput.Keyboard.Check(Keys.Delete)) { // Del to delete entities
                foreach (var item in Editor.SelectedObjects) {
                    item.RemoveSelf();
                    refreshPanel = true;
                }

                Editor.SelectedObjects.Clear();
            } else if (MInput.Keyboard.Pressed(Keys.N)) { // N to create node
                // only visit each entity once
                HashSet<Entity> seen = new();
                // iterate backwards to allow modifying the list as we go
                for (var idx = Editor.SelectedObjects.Count - 1; idx >= 0; idx--) {
                    var item = Editor.SelectedObjects[idx];
                    if (item is EntitySelection { Entity: var e, Index: var oldIdx } && (e.Nodes.Count < e.MaxNodes || e.MaxNodes == -1)) {
                        if (!seen.Contains(e)) {
                            int newNodeIdx = oldIdx + 1;
                            Vector2 oldPos = oldIdx == -1 ? e.Position : e.Nodes[oldIdx];
                            e.AddNode(oldPos + new Vector2(24, 0), newNodeIdx);
                            Editor.SelectedObjects.Add(new EntitySelection(e, newNodeIdx));
                            seen.Add(e);
                        }

                        Editor.SelectedObjects.Remove(item);
                    }
                }
            } else if (MInput.Keyboard.Pressed(Keys.Escape)) { // Esc to deselect all & cancel paste
                if (Editor.SelectedObjects.Count > 0)
                    refreshPanel = true;
                Editor.SelectedObjects.Clear();
                pasting = false;
                toPaste = null;
            } else if (MInput.Keyboard.Pressed(Keys.Up)) { // Up/Down/Left/Right to nudge entities
                Nudge(new(0, ctrl ? -1 : -8));
            } else if (MInput.Keyboard.Pressed(Keys.Down)) {
                Nudge(new(0, ctrl ? 1 : 8));
            } else if (MInput.Keyboard.Pressed(Keys.Left)) {
                Nudge(new(ctrl ? -1 : -8, 0));
            } else if (MInput.Keyboard.Pressed(Keys.Right)) {
                Nudge(new(ctrl ? 1 : 8, 0));
            } else if (Editor.SelectedRoom != null && ctrl) {
                if (MInput.Keyboard.Pressed(Keys.A)) { // Ctrl-A to select all
                    Editor.SelectedObjects = GetEnabledSelections(null);
                    refreshPanel = true;
                } else if (MInput.Keyboard.Pressed(Keys.C, Keys.X)) { // Ctrl-C to copy
                    CopyPaste.Clipboard = CopyPaste.CopyEntities(Editor.SelectedObjects.OfType<EntitySelection>().Select(x => x.Entity).Distinct());

                    if (MInput.Keyboard.Pressed(Keys.X)) {
                        foreach (var item in Editor.SelectedObjects) {
                            item.RemoveSelf();
                            refreshPanel = true;
                        }

                        Editor.SelectedObjects.Clear();
                    }
                } else if (MInput.Keyboard.Pressed(Keys.V)) { // Ctrl-V to paste
                    try {
                        List<(EntityData data, bool trigger)> entities = CopyPaste.PasteEntities(CopyPaste.Clipboard);
                        if (entities.Count != 0) {
                            pasting = true;
                            toPaste = new(entities.Count);
                            foreach (var entity in entities)
                                toPaste.Add(Entity.TryCreate(Editor.SelectedRoom, entity.data, entity.trigger, out bool _));
                            AdjustPastedEntities();
                        }
                    } catch (ArgumentException ae) {
                        Snowberry.LogInfo(ae.Message);
                    } catch (InvalidOperationException) {
                        Snowberry.LogInfo("Failed to paste (invalid data)");
                    }
                }
            }
        }

        if ((MInput.Mouse.ReleasedLeftButton && canClick && canSelect) || refreshPanel)
            selectionPanel?.Display(Editor.SelectedObjects);
    }

    public override void RenderWorldSpace() {
        base.RenderWorldSpace();
        if (Editor.SelectedRoom != null) {
            Color nextCol = CurrentEffect switch {
                SelectionEffect.Add => Color.Green,
                SelectionEffect.Subtract => Color.Red,
                _ => Color.Blue
            };
            // highlight what's hovered
            foreach (var item in GetEnabledSelections(Mouse.World.ToRect()))
                Draw.Rect(item.Area(), nextCol * 0.15f);
            // highlight the selection rect
            if (SelectionInProgress.HasValue)
                Draw.Rect(SelectionInProgress.Value, nextCol * 0.25f);
            if (PathInProgress != null) {
                if (currentMode == SelectionMode.Lasso)
                    DrawUtil.DrawPolygon(PathInProgress.ToArray(), nextCol * 0.25f);
                DrawUtil.Path(PathInProgress, nextCol * 0.25f, 2);
            }
            // highlight already-selected stuff in blue
            if (Editor.SelectedObjects != null)
                foreach (var rect in Editor.SelectedObjects.Select(s => s.Area()))
                    Draw.Rect(rect, Color.Blue * 0.25f);
            // highlight what's about to get selected
            foreach (var rect in next.Select(s => s.Area()))
                Draw.Rect(rect, nextCol * 0.15f);

            if (MInput.Mouse.CheckLeftButton && !canSelect && (resizingX || resizingY) && GetSoloEntity() is {} nonNull)
                DrawUtil.DrawGuidelines(nonNull.Bounds, Color.White);

            if (Editor.SelectedObjects != null && Editor.Instance.CanTypeShortcut() && MInput.Keyboard.Check(Keys.D))
                foreach (EntitySelection es in Editor.SelectedObjects.OfType<EntitySelection>())
                    DrawUtil.DrawGuidelines(es.Entity.Bounds, Color.White);

            if (pasting) {
                Calc.PushRandom(RuntimeHelpers.GetHashCode(toPaste));
                foreach (Entity e in toPaste)
                    e.Render();
                Calc.PopRandom();
            }
        }
    }

    public override void SuggestCursor(ref MTexture cursor, ref Vector2 justify) {
        Point mouse = new Point((int)Mouse.World.X, (int)Mouse.World.Y);

        // cursor depending on selection mode and effect
        justify = new(0, 1);
        cursor = SelectionAtlas.GetSubtexture((int)currentMode * 16, (1 + (int)CurrentEffect) * 16, 16, 16);

        // hovering over a selected entity? and we're not selecting? movement arrow
        if (Editor.SelectedObjects != null && SelectionInProgress == null && PathInProgress == null && Editor.SelectedObjects.Any(s => s.Contains(mouse))) {
            justify = Vector2.One / 2f;
            cursor = UIScene.CursorsAtlas.GetSubtexture(16, 16, 16, 16);

            // only have 1 entity selected & at the borders? show resizing tooltips
            Entity solo = GetSoloEntity();
            if (solo != null) {
                var fromLeft = solo.MinWidth > -1 && Math.Abs(Mouse.World.X - solo.Position.X) <= resizeMargins;
                var fromRight = solo.MinWidth > -1 && Math.Abs(Mouse.World.X - (solo.Position.X + solo.Width)) <= resizeMargins;
                var fromTop = solo.MinHeight > -1 && Math.Abs(Mouse.World.Y - solo.Position.Y) <= resizeMargins;
                var fromBottom = solo.MinHeight > -1 && Math.Abs(Mouse.World.Y - (solo.Position.Y + solo.Height)) <= resizeMargins;
                if (fromLeft || fromRight || fromTop || fromBottom) {
                    if ((fromBottom && fromLeft) || (fromTop && fromRight)) {
                        cursor = UIScene.CursorsAtlas.GetSubtexture(32, 32, 16, 16);
                        return;
                    }

                    if ((fromTop && fromLeft) || (fromBottom && fromRight)) {
                        cursor = UIScene.CursorsAtlas.GetSubtexture(48, 32, 16, 16);
                        return;
                    }

                    if (fromLeft || fromRight) {
                        cursor = UIScene.CursorsAtlas.GetSubtexture(0, 32, 16, 16);
                        return;
                    }

                    if (fromBottom || fromTop)
                        cursor = UIScene.CursorsAtlas.GetSubtexture(16, 32, 16, 16);
                }
            }
        }
    }

    private void Nudge(Vector2 by) {
        if (Editor.SelectedObjects.Count > 0) {
            UndoRedo.BeginAction("nudge objects", Editor.SelectedObjects.Select(x => x.Snapshotter()));
            foreach (Selection s in Editor.SelectedObjects) {
                s.Move(by);
                SnapIfNecessary(s);
            }
            TileSelection.FinishMove();
            UndoRedo.CompleteAction();
        }
    }

    private static Selection GetSoloSelection() =>
        Editor.SelectedObjects != null && Editor.SelectedObjects.Count == 1 ? Editor.SelectedObjects[0] : null;

    private static Entity GetSoloEntity() =>
        GetSoloSelection() is EntitySelection { Entity: var e, Index: -1 } ? e : null;

    private static void AdjustPastedEntities() {
        Rectangle cover = CoveringRect(toPaste.Select(e => e.Bounds).Concat(toPaste.SelectMany(e => e.Nodes.Select(Util.ToRect))).ToList());
        Vector2 offset = (Mouse.World - cover.Center.ToVector2()).RoundTo(8);
        foreach (Entity e in toPaste) {
            e.Move(offset);
            for (int i = 0; i < e.Nodes.Count; i++)
                e.MoveNode(i, offset);
            SnapIfNecessary(e, true);
        }
    }

    private static Rectangle CoveringRect(IReadOnlyList<Rectangle> rects) =>
        rects.Aggregate(rects[0], Rectangle.Union);

    private static void SnapIfNecessary(Entity e, bool ignoreCtrl = false) {
        if (Snowberry.Settings.AggressiveSnap && (ignoreCtrl || !MInput.Keyboard.Check(Keys.LeftControl, Keys.RightControl))) {
            e.SetPosition(e.Position.RoundTo(8));
            for (var idx = 0; idx < e.Nodes.Count; idx++)
                e.SetNode(idx, e.Nodes[idx].RoundTo(8));
        }
    }

    private static void SnapIfNecessary(Selection s, bool ignoreCtrl = false) {
        if (s is EntitySelection { Entity: var e })
            SnapIfNecessary(e, ignoreCtrl);
    }

    private static List<Selection> GetEnabledSelections(Rectangle? r) =>
        Editor.SelectedRoom?.GetSelections(r, selectEntities, selectTriggers, selectFgDecals, selectBgDecals, selectFgTiles, selectBgTiles) ?? new();

    private static List<Selection> MagicWand() {
        // first get everything under the mouse
        var at = Editor.SelectedRoom.GetSelections(Mouse.World.ToRect(), selectEntities, selectTriggers /* nothing else does anything here */);
        // then get all types of those entities
        HashSet<string> entityTypes = new(at.OfType<EntitySelection>().Select(x => x.Entity.Name));
        // add all entities of the same type
        List<Selection> into = new();
        foreach (var entity in Editor.SelectedRoom.AllEntities.Where(entity => entityTypes.Contains(entity.Name)))
            if (entity.SelectionRectangles is { Length: > 0 } rs)
                into.AddRange(rs.Select((_, i) => new EntitySelection(entity, i - 1)).ToList());
        return into;
    }

    private static SelectionEffect CurrentEffect =>
        MInput.Keyboard.Check(Keys.LeftControl, Keys.RightControl) ? SelectionEffect.Add :
        MInput.Keyboard.Check(Keys.LeftAlt, Keys.RightAlt) ? SelectionEffect.Subtract :
        SelectionEffect.Set;
}
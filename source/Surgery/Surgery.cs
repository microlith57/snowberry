using System;
using System.Collections.Generic;
using System.IO;
using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using Snowberry.Editor.UI;
using Snowberry.Editor.UI.Menus;
using Mouse = Snowberry.Editor.Mouse;

namespace Snowberry.Surgery;

using Element = BinaryPacker.Element;

public class Surgery : Scene {

    private RenderTarget2D uiBuffer;
    private UIElement ui = new();
    private UIMessage message;

    private string path;
    private Element elem;

    public Surgery(string path, Element elem) {
        this.path = path;
        this.elem = elem;
    }

    public override void Begin() {
        base.Begin();

        uiBuffer = new RenderTarget2D(Engine.Instance.GraphicsDevice, Engine.ViewWidth / 2, Engine.ViewHeight / 2);
        ui.Width = uiBuffer.Width;
        ui.Height = uiBuffer.Height;

        SurgeryUi();

        ui.Add(message = new UIMessage {
            Width = ui.Width,
            Height = ui.Height
        });
    }

    public override void End() {
        base.End();
        uiBuffer.Dispose();
        ui.Destroy();
    }

    public override void Update() {
        base.Update();

        Mouse.WorldLast = Mouse.World;
        Mouse.ScreenLast = Mouse.Screen;

        MouseState m = Microsoft.Xna.Framework.Input.Mouse.GetState();
        Mouse.Screen = new Vector2(m.X, m.Y) / 2;
        Mouse.World = Mouse.Screen; // "world" doesn't exist, but we'll keep this field valid

        Editor.Editor.MouseClicked = false;
        ui.Update();

        if (MInput.Mouse.PressedLeftButton)
            Mouse.LastClick = DateTime.Now;
    }

    public override void Render() {
        Draw.SpriteBatch.GraphicsDevice.Clear(Editor.Editor.bg);

        #region UI Rendering

        Engine.Instance.GraphicsDevice.SetRenderTarget(uiBuffer);
        Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

        ui.Render();

        MTexture curCursor = Editor.Editor.defaultCursor;
        Vector2 curJustify = Vector2.Zero;
        curCursor.DrawJustified(Mouse.Screen, curJustify);

        // Tooltip rendering
        var tooltip = ui.HoveredTooltip();
        if (tooltip != null) {
            string[] array = tooltip.Split(new[] { "\\n" }, StringSplitOptions.None);
            for(int i = 0; i < array.Length; i++) {
                string line = array[i];
                var tooltipArea = Fonts.Regular.Measure(line);
                var at = Mouse.Screen.Floor() - new Vector2((tooltipArea.X + 8), -(tooltipArea.Y + 6) * i);
                Draw.Rect(at, tooltipArea.X + 8, tooltipArea.Y + 6, Color.Black * 0.8f);
                Fonts.Regular.Draw(line, at + new Vector2(4, 3), Vector2.One, Color.White);
            }
        }

        Draw.SpriteBatch.End();

        #endregion

        Engine.Instance.GraphicsDevice.SetRenderTarget(null);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
        Draw.SpriteBatch.Draw(uiBuffer, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, Vector2.One * 2, SpriteEffects.None, 0f);
        Draw.SpriteBatch.End();
    }

    internal void SurgeryUi() {
        UIElement topBar = new() {
            Background = Color.DarkRed,
            Width = ui.Width,
            Height = 40
        };

        topBar.AddRight(new UILabel("snowberry", Fonts.Regular, 2), new(8, 8));
        topBar.AddRight(new UILabel("surgery", Fonts.Bold, 2) {
            Underline = true,
            FG = Color.Red
        }, new(8, 8));
        topBar.AddRight(new UILabel($"on {path}", Fonts.Regular) {
            FG = Color.Gray
        }, new(8, 20));

        UITextField mapName = new UITextField(Fonts.Regular, 400, Path.GetFileName(path));
        topBar.AddRight(new UIButton(Editor.Editor.actionbarIcons.GetSubtexture(16, 0, 16, 16), 3, 3) {
            OnPress = () => BinaryExporter.Export(elem, mapName.Value + ".bin")
        }, new(40, 8));
        topBar.AddRight(mapName, new(8, 14));

        ui.Add(topBar);

        UIScrollPane rest = new() {
            Width = ui.Width,
            Height = ui.Height - 40,
            Position = new(0, 40),
            TopPadding = 10
        };

        rest.AddBelow(Render(elem, null), new(10));

        ui.Add(rest);
    }

    internal UIElement Render(Element e, Element parent) {
        UIElement ret = new();

        ret.Add(string.IsNullOrEmpty(e.Name)
            ? new UILabel(e.Name is null ? "(null)" : "(blank)") {
                FG = Color.Gray
            }
            : new UILabel(e.Name));

        if(parent != null){
            ret.AddRight(new UIButton("x", Fonts.Regular) {
                OnPress = () => {
                    parent.Children?.Remove(e);
                    ret.RemoveSelf();
                },
                FG = Color.Red,
                HoveredFG = Color.Crimson,
                PressedFG = Color.DarkRed
            }, new(3, -1));
        }

        if (e.Attributes != null)
            foreach (KeyValuePair<string, object> kvp in e.Attributes) {
                UIElement attr = new();
                attr.Add(new UILabel(kvp.Key));
                attr.AddRight(new UILabel("="), new(4, 0));
                var text = (kvp.Value ?? "null").ToString();
                attr.AddRight(new UILabel(text.Contains("\n") ? "..." : text), new(4, 0));
                attr.CalculateBounds();

                ret.AddBelow(attr, new(8, 2));
            }

        if (e.Children != null)
            foreach (Element child in e.Children)
                ret.AddBelow(Render(child, e), new(20, 4));

        ret.CalculateBounds();
        return ret;
    }
}
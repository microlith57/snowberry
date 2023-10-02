using System;
using System.Linq;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Snowberry.UI.Menus.MainMenu;

public class UILevelRibbon : UIRibbon {
    private readonly UILevelSelector selector;

    private readonly string raw;
    public readonly string Name;

    private readonly int w;
    public int W { get; private set; }

    private float lerp, listLerp = 1f;
    private readonly int n;

    private bool hover;

    private readonly bool dropdown;
    private bool open;
    private float openLerp;
    private readonly int h;
    public int H { get; private set; }

    private bool pressing;

    private readonly ModeProperties mode;

    private UILevelRibbon(UILevelSelector selector, ModeProperties mode, int n)
        : base("", 39) {
        this.selector = selector;
        this.n = n;

        Name = mode.MapData.Area.Mode switch {
            AreaMode.Normal => "A",
            AreaMode.BSide => "B",
            AreaMode.CSide => "C",
            _ => "X",
        };
        raw = mode.MapData.Filename;
        SetText(Name);

        w = (int)Fonts.Regular.Measure(raw).X;
        W = Width + w + 5;

        this.mode = mode;
    }

    public UILevelRibbon(UILevelSelector selector, AreaData area, int n)
        : base("", 26) {
        this.selector = selector;
        this.n = n;

        Name = area.Name;
        raw = Dialog.Has(Name) ? $"» {Name}" : "...";

        ModeProperties[] modes = area.Mode.Where(m => m?.MapData != null).ToArray();
        if (dropdown = modes.Length > 1) {
            h = modes.Length * 13 + 1;
            for (int i = 0; i < modes.Length; i++) {
                ModeProperties m = modes[i];
                Add(new UILevelRibbon(selector, m, i + 1) {
                    Position = new Vector2(-5, 13 * (i + 1)),
                });
            }
        } else
            mode = modes[0];

        SetText($"{(dropdown ? "\uF034" : " ")} {Dialog.Clean(Name)}");

        w = (int)Fonts.Regular.Measure(raw).X;
        W = Width + w + 5;

        RenderChildren = false;
    }

    protected override void Initialize() {
        base.Initialize();
        foreach (UIElement child in Children) {
            if (child is UILevelRibbon lvl) {
                lvl.FG = FG;
                lvl.BG = BG;
                lvl.BGAccent = BGAccent;
            }
        }
    }

    private bool HoveringChildren() {
        foreach (UIElement child in Children)
            if (child is UILevelRibbon lvl && lvl.hover)
                return true;
        return false;
    }

    public override void Update(Vector2 position = default) {
        base.Update(position);

        int mouseX = (int)Mouse.Screen.X;
        int mouseY = (int)Mouse.Screen.Y;
        hover = !UIScene.Instance.Message.Shown && Visible &&
                new Rectangle((int)position.X + 16, (int)position.Y - 1, Width + w, Height + H + 2).Contains(mouseX, mouseY);

        lerp = Calc.Approach(lerp, (hover || pressing).Bit(), Engine.DeltaTime * 6f);
        listLerp = Calc.Approach(listLerp, (selector.LevelRibbonAnim < n).Bit(), Engine.DeltaTime * 4f);

        if (Visible) {
            if (!UIScene.Instance.Message.Shown && hover && ConsumeLeftClick()) {
                if (dropdown) {
                    if (!HoveringChildren()) {
                        openLerp = open.Bit();
                        open = !open;
                        SetText((open ? '\uF036' : '\uF034') + Text.Substring(1));
                    }
                } else if (Parent is not UILevelRibbon lvl || lvl.open) {
                    pressing = true;
                }
            }
            if (UIScene.Instance.Message.Shown || pressing && ConsumeLeftClick(pressed: false, released: true)) {
                pressing = false;
                if (hover) {
                    if (MInput.Keyboard.CurrentState[Keys.LeftControl] == KeyState.Down || MInput.Keyboard.CurrentState[Keys.RightControl] == KeyState.Down)
                        //Editor.Editor.Open(mode.MapData);
                        TryOpen();
                    else {
                        UIScene.Instance.Message.Clear();

                        UIScene.Instance.Message.AddElement(ConfirmLoadMessage(), 0.5f, 0.5f, 0.5f, -0.1f);
                        var buttons = UIMessage.YesAndNoButtons(TryOpen, () => UIScene.Instance.Message.Shown = false, 0, 4, 0.5f, 0f);
                        UIScene.Instance.Message.AddElement(buttons, 0.5f, 0.5f, 0.5f, 1.1f);

                        UIScene.Instance.Message.Shown = true;
                    }
                }
            }
            if (MInput.Mouse.ReleasedLeftButton) {
                // we only actually activate if the mouse was released on this button + it's visible,
                // but we still need to make it visually unpress if it's dragged off and released
                pressing = false;
            }
        }

        openLerp = Calc.Approach(openLerp, open.Bit(), Engine.DeltaTime * 2f);
        float openEase = (open ? Ease.ExpoOut : Ease.ExpoIn)(openLerp);
        H = (int)(openEase * h);
    }

    private void TryOpen(){
        try{
            Editor.Editor.Open(mode.MapData);
        }catch(Exception e){
            UIScene.Instance.Message.Clear();
            UILabel errInfo = new UILabel("failed to load map!");
            errInfo.Position = new(-errInfo.Width / 2, -10);
            UIButton ok = new UIButton("ok", Fonts.Regular, 4, 4){
                OnPress = () => UIScene.Instance.Message.Shown = false
            };
            ok.Position = new(-ok.Width / 2, 20);
            var element = UIElement.Regroup(errInfo, ok);
            Vector2 offset = new Vector2(element.Width / 2f, element.Height);
            errInfo.Position -= offset;
            ok.Position -= offset;
            UIScene.Instance.Message.AddElement(element, 0.5f, 0.5f, 0.5f, -0.1f);
            UIScene.Instance.Message.Shown = true;
            Snowberry.Log(LogLevel.Error, $"Failed to load map {mode.MapData.Area}: {e}");
        }
    }

    public override void Render(Vector2 position = default) {
        Vector2 from = position;

        float ease = Ease.CubeOut(lerp);
        float listEase = Ease.ExpoIn(listLerp);
        position.X += (int)(ease * 16 - Width * listEase + (pressing ? 4 : 0));

        float sin = Settings.Instance.DisableFlashes || lerp == 0f ? 0f : ((float)Math.Sin(Engine.Scene.TimeActive * 12f) * 0.1f);
        Fonts.Regular.Draw(raw, position + Vector2.UnitX * (Width + 5), Vector2.One, Color.Lerp(Util.Colors.CloudGray, Util.Colors.White, ease * (0.9f + sin)) * (1 - listEase));

        base.Render(position);

        if (dropdown) {
            foreach (UIElement child in Children)
                if (child is UILevelRibbon lvl)
                    lvl.Render(from + lvl.Position);
            Draw.Rect(new Vector2(from.X, position.Y + Height + H + 2), Parent.Width, h - H, Util.Colors.DarkGray);
            Draw.Rect(new Vector2(from.X, position.Y + Height), 24, H + 2, Util.Colors.DarkGray);
            Draw.Rect(new Vector2(from.X + 24, position.Y + Height), 1, H + 2, BG);
        }
    }

    private UIElement ConfirmLoadMessage() {
        UIRibbon ribbon = new UIRibbon(Dialog.Clean(mode.MapData.Data.Name), 8, 8, true, true) {
            FG = FG,
            BG = BG,
            BGAccent = BGAccent,
        };
        ribbon.Position = new Vector2(-ribbon.Width / 2, 0);

        UILabel msg = new UILabel(Dialog.Clean("SNOWBERRY_MAINMENU_LOAD_CONFIRM"));
        msg.Position = new Vector2(-msg.Width / 2, ribbon.Position.Y + ribbon.Height + 4);

        UILabel warn = new UILabel(Dialog.Clean("SNOWBERRY_MAINMENU_LOAD_UNSAVED")) {
            FG = Util.Colors.CloudLightGray,
        };
        warn.Position = new Vector2(-warn.Width / 2, msg.Position.Y + msg.Height);

        UILabel tip = new UILabel(Dialog.Clean("SNOWBERRY_MAINMENU_LOAD_TIP")) {
            FG = Util.Colors.CloudLightGray,
        };
        tip.Position = new Vector2(-tip.Width / 2, warn.Position.Y + warn.Height);

        var element = Regroup(ribbon, msg, warn, tip);

        Vector2 offset = new Vector2(element.Width / 2f, element.Height);
        ribbon.Position -= offset;
        msg.Position -= offset;
        warn.Position -= offset;
        tip.Position -= offset;

        return element;
    }
}
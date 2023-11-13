using System;
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using Snowberry.UI.Controls;

namespace Snowberry.UI;

public class UIMessage : UIElement {
    private class Msg(UIElement element, Vector2 displayJustify, Vector2 hiddenJustify, Vector2 initialOffset) {
        public void UpdateElement(int w, int h, float lerp) =>
            element.Position = initialOffset + (new Vector2(w - element.Width, h - element.Height) * Vector2.Lerp(hiddenJustify, displayJustify, lerp)).Round();
    }

    private readonly List<Msg> msgs = new();

    private float lerp;
    public bool Shown;

    public void Clear() {
        base.Clear();
        msgs.Clear();
    }

    public void AddElement(UIElement element, Vector2 offset = default, float justifyX = 0.5f, float justifyY = 0.5f, float hiddenJustifyX = 0.5f, float hiddenJustifyY = 1.1f) {
        Add(element);
        var msg = new Msg(element, new Vector2(justifyX, justifyY), new Vector2(hiddenJustifyX, hiddenJustifyY), offset);
        msgs.Add(msg);
        msg.UpdateElement(Width, Height, Ease.ExpoOut(lerp));
    }

    public override void Update(Vector2 position = default) {
        base.Update(position);

        lerp = Calc.Approach(lerp, Shown.Bit(), Engine.DeltaTime * 2f);
        float ease = Ease.ExpoOut(lerp);

        if (!Shown && lerp < 0.005f)
            Clear();

        foreach (Msg msg in msgs)
            msg.UpdateElement(Width, Height, ease);

        if (MInput.Keyboard.Check(Keys.Escape))
            Shown = false;

        GrabsClick = GrabsScroll = Shown;
    }

    public override void Render(Vector2 position = default) {
        Draw.Rect(position, Width, Height, Color.Black * lerp * 0.75f);
        base.Render(position);
    }

    public static UIElement YesAndNoButtons(Action yesPress = null, Action noPress = null) {
        UIButton yes = new UIButton(Dialog.Clean("SNOWBERRY_MAINMENU_YES"), Fonts.Regular, 4, 6) {
            FG = Util.Colors.White,
            BG = Util.Colors.Blue,
            PressedBG = Util.Colors.White,
            PressedFG = Util.Colors.Blue,
            HoveredBG = Util.Colors.DarkBlue,
            OnPress = yesPress
        };
        UIButton no = new UIButton(Dialog.Clean("SNOWBERRY_MAINMENU_NO"), Fonts.Regular, 4, 6) {
            FG = Util.Colors.White,
            BG = Util.Colors.Red,
            PressedBG = Util.Colors.White,
            PressedFG = Util.Colors.Red,
            HoveredBG = Util.Colors.DarkRed,
            Position = new Vector2(yes.Position.X + yes.Width + 4, yes.Position.Y),
            OnPress = noPress
        };

        return Regroup(yes, no);
    }

    public static void ShowInfoPopup(string infoKey, string closeKey){
        UIScene.Instance.Message.Clear();
        UIScene.Instance.Message.AddElement(new UILabel(Dialog.Clean(infoKey)), new(0, -10), hiddenJustifyY: -0.1f);
        UIScene.Instance.Message.AddElement(new UIButton(Dialog.Clean(closeKey), Fonts.Regular, 4, 4){
            OnPress = () => UIScene.Instance.Message.Shown = false
        }, new(0, 20), hiddenJustifyY: -0.1f);
        UIScene.Instance.Message.Shown = true;
    }
}
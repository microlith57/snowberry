using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Snowberry.Editor.Recording;
using Snowberry.UI;

namespace Snowberry.Editor.Tools;

public class PlaytestTool : Tool {

    private float time = 0;

    public override string GetName() => Dialog.Clean("SNOWBERRY_EDITOR_TOOL_PLAYTEST");

    public override UIElement CreatePanel(int height) => new();

    public override UIElement CreateActionBar() {
        UIElement p = new UIElement();

        p.AddRight(new UIButton(UIScene.ActionbarAtlas.GetSubtexture(32, 80, 16, 16), 3, 3) {
            OnPress = () => {
                Editor.Instance.BeginPlaytest();
                RecInProgress.BeginRecording();
            }
        }, new(0, 4));
        p.AddRight(new UIButton("<<", Fonts.Regular, 6, 6) {
            OnPress = () => {
                time = 0;
            }
        }, new(6, 4));

        return p;
    }

    public override void Update(bool canClick) {
        time += Engine.DeltaTime;
    }

    public override void RenderWorldSpace() {
        base.RenderWorldSpace();

        foreach (Recorder r in RecInProgress.Recorders)
            r.RenderWorldSpace(time);
    }

    public override void RenderScreenSpace() {
        base.RenderScreenSpace();

        foreach (Recorder r in RecInProgress.Recorders)
            r.RenderScreenSpace(time);
    }

    public override void SuggestCursor(ref MTexture cursor, ref Vector2 justify) {
        justify = new(0.5f);
        cursor = UIScene.CursorsAtlas.GetSubtexture(0, 64, 16, 16);
    }
}
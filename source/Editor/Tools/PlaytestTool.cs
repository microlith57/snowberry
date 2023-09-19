using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using Snowberry.Editor.Recording;
using Snowberry.UI;

namespace Snowberry.Editor.Tools;

public class PlaytestTool : Tool {

    private float time = 0, maxTime = 0;
    private bool playing = false;

    private UISlider timeSlider;
    private UIKeyboundButton playPauseButton;

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

        if (RecInProgress.Get<TimeRecorder>() is { /* non-null */ } timer) {
            maxTime = timer.MaxTime;

            playPauseButton = new UIKeyboundButton(UIScene.ActionbarAtlas.GetSubtexture(9, 101, 6, 6), 3, 4);
            playPauseButton.Key = Keys.Space;
            playPauseButton.OnPress = () => {
                playing = !playing;
                SetPlayPauseIcon();
            };
            p.AddRight(playPauseButton, new(6, 8));

            p.AddRight(timeSlider = new UISlider {
                Min = 0,
                Max = maxTime,
                Width = 200,
                OnInputChanged = t => {
                    time = t;
                    playing = false;
                    SetPlayPauseIcon();
                }
            }, new(10, 4));

            UIElement frameButtons = new();
            frameButtons.Add(new UIKeyboundButton(UIScene.ActionbarAtlas.GetSubtexture(20, 99, 8, 4), 2, 2) {
                Key = Keys.OemPeriod,
                OnPress = () => {
                    int curIdx = timer.FrameTimes.FindLastIndex(x => x <= time);
                    if (curIdx == -1) curIdx = 0;
                    if (timer.FrameTimes.Count > curIdx + 1) {
                        time = timer.FrameTimes[curIdx + 1];
                        timeSlider.Value = time;
                        playing = false;
                        SetPlayPauseIcon();
                    }
                }
            });
            frameButtons.AddBelow(new UIKeyboundButton(UIScene.ActionbarAtlas.GetSubtexture(20, 105, 8, 4), 2, 2) {
                Key = Keys.OemComma,
                OnPress = () => {
                    int curIdx = timer.FrameTimes.FindLastIndex(x => x <= time);
                    if (curIdx - 1 >= 0) {
                        time = timer.FrameTimes[curIdx - 1];
                        timeSlider.Value = time;
                        playing = false;
                        SetPlayPauseIcon();
                    }
                }
            });
            p.AddRight(frameButtons, new(6, 7));
        }

        return p;
    }

    public override void Update(bool canClick) {
        if (playing) {
            if (time < maxTime) {
                time += Engine.DeltaTime;
                timeSlider.Value = time;
            } else {
                playing = false;
                SetPlayPauseIcon();
            }
        }
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
        if (playing) {
            justify = new(0.5f);
            cursor = UIScene.CursorsAtlas.GetSubtexture(0, 64, 16, 16);
        }
    }

    private void SetPlayPauseIcon() =>
        playPauseButton.SetIcon(UIScene.ActionbarAtlas.GetSubtexture(playing ? 2 : 9, 101, 6, 6));
}
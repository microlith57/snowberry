using System.Linq;
using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using Snowberry.Editor.Recording;
using Snowberry.UI;
using Snowberry.UI.Controls;

namespace Snowberry.Editor.Tools;

public class PlaytestTool : Tool {

    private float time = 0, maxTime = 0;
    private bool playing = false;

    private UISlider timeSlider;
    private UIKeyboundButton playPauseButton;

    public override string GetName() => Dialog.Clean("SNOWBERRY_EDITOR_TOOL_PLAYTEST");

    public override UIElement CreatePanel(int height) {
        UIElement panel = new() {
            Width = 160,
            Background = Calc.HexToColor("202929") * (185 / 255f),
            GrabsClick = true,
            GrabsScroll = true,
            Height = height
        };

        if (RecInProgress.Recorders.Count == 0) {
            var title = new UILabel(Dialog.Clean("SNOWBERRY_EDITOR_PT_NO_DATA"));
            title.Position = new((160 - title.Width) / 2f, 10);
            panel.Add(title);
        }

        foreach (Recorder r in RecInProgress.Recorders) {
            var optionsPane = r.CreateOptionsPane();
            if (optionsPane != null) {
                optionsPane.Width = 160 - 10;
                panel.AddBelow(optionsPane, new(5));
            }
        }

        return panel;
    }

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

            p.AddRight(playPauseButton = new UIKeyboundButton(UIScene.ActionbarAtlas.GetSubtexture(9, 101, 6, 6), 3, 4) {
                Key = Keys.Space,
                OnPress = () => {
                    playing = !playing;
                    UpdatePlayPauseButton();
                },
                ButtonTooltip = Dialog.Clean("SNOWBERRY_EDITOR_PT_PLAY_TT")
            }, new(6, 8));

            p.AddRight(timeSlider = new UISlider {
                Min = 0,
                Max = maxTime,
                Width = 200,
                // SetTime does set the slider's Value, but this does not trigger this again
                OnInputChanged = t => SetTime(t)
            }, new(10, 7));

            UIElement frameButtons = new();
            frameButtons.Add(new UIKeyboundButton(UIScene.ActionbarAtlas.GetSubtexture(20, 99, 8, 4), 2, 2) {
                Key = Keys.OemPeriod,
                OnPress = () => {
                    int curIdx = timer.FrameTimes.FindLastIndex(x => x <= time);
                    if (curIdx == -1) curIdx = 0;
                    if (timer.FrameTimes.Count > curIdx + 1)
                        SetTime(timer.FrameTimes[curIdx + 1]);
                },
                ButtonTooltip = Dialog.Clean("SNOWBERRY_EDITOR_PT_NEXT_FRAME_TT")
            });
            frameButtons.AddBelow(new UIKeyboundButton(UIScene.ActionbarAtlas.GetSubtexture(20, 105, 8, 4), 2, 2) {
                Key = Keys.OemComma,
                OnPress = () => {
                    int curIdx = timer.FrameTimes.FindLastIndex(x => x <= time);
                    if (curIdx - 1 >= 0)
                        SetTime(timer.FrameTimes[curIdx - 1]);
                },
                ButtonTooltip = Dialog.Clean("SNOWBERRY_EDITOR_PT_PREV_FRAME_TT")
            });
            frameButtons.CalculateBounds();
            p.AddRight(frameButtons, new(6, 7));
        }

        return p;
    }

    public override void Update(bool canClick) {
        if (playing) {
            if (time < maxTime)
                SetTime(time + Engine.DeltaTime, false);
            else {
                playing = false;
                UpdatePlayPauseButton();
            }
        }
    }

    public override void RenderWorldSpace() {
        base.RenderWorldSpace();

        foreach(var r in RecInProgress.Recorders.Where(r => r.GetSettings().show))
            r.RenderWorldSpace(time);
    }

    public override void RenderScreenSpace() {
        base.RenderScreenSpace();

        foreach(var r in RecInProgress.Recorders.Where(r => r.GetSettings().show))
            r.RenderScreenSpace(time);
    }

    public override void SuggestCursor(ref MTexture cursor, ref Vector2 justify) {
        if (playing) {
            justify = new(0.5f);
            cursor = UIScene.CursorsAtlas.GetSubtexture(0, 64, 16, 16);
        }
    }

    public float Now => time;

    private void UpdatePlayPauseButton() {
        playPauseButton.SetIcon(UIScene.ActionbarAtlas.GetSubtexture(playing ? 2 : 9, 101, 6, 6));
        playPauseButton.ButtonTooltip = Dialog.Clean(playing ? "SNOWBERRY_EDITOR_PT_PAUSE_TT" : "SNOWBERRY_EDITOR_PT_PLAY_TT");
    }

    private void SetTime(float newTime, bool pause = true) {
        this.time = newTime;
        timeSlider.Value = newTime;
        if (pause) {
            playing = false;
            UpdatePlayPauseButton();
        }
    }
}
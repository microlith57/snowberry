using System;
using System.Collections.Generic;
using Celeste;
using Snowberry.UI;
using Snowberry.UI.Menus;

namespace Snowberry.Editor.Recording;

public abstract class Recorder{

    public static readonly List<Func<Recorder>> Recorders = [
        () => new TimeRecorder(),
        () => new PlayerRecorder(),
        () => new HitboxesRecorder(),
        () => new CameraRecorder(),
        () => new FlagsRecorder()
    ];

    public abstract void UpdateInGame(Level l, float time);
    public virtual void FinalizeRecording(){}

    public abstract void RenderScreenSpace(float time);
    public abstract void RenderWorldSpace(float time);

    public abstract string Name();
    public virtual UIElement CreateOptionsPane() {
        UIElement ret = new();

        ret.AddBelow(new UILabel(Name()), new(3));
        ret.AddBelow(UIPluginOptionList.BoolOption(Dialog.Clean("SNOWBERRY_EDITOR_PT_OPTS_SHOW"), GetSettings().show, b => {
            Snowberry.Settings.RecorderSettings[GetType().FullName] = (b, GetSettings().record);
            Snowberry.Instance.SaveSettings();
        }), new(6, 3));
        ret.AddBelow(UIPluginOptionList.BoolOption(Dialog.Clean("SNOWBERRY_EDITOR_PT_OPTS_RECORD"), GetSettings().record, b => {
            Snowberry.Settings.RecorderSettings[GetType().FullName] = (GetSettings().show, b);
            Snowberry.Instance.SaveSettings();
        }), new(6, 3));
        ret.CalculateBounds();
        ret.Height += 3;
        ret.Background = Util.Colors.Cyan * 0.3f;

        return ret;
    }

    public (bool show, bool record) GetSettings() =>
        Snowberry.Settings.RecorderSettings.TryGetValue(GetType().FullName, out var v) ? v : (true, true);
}
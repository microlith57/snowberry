using System.Collections.Generic;
using System.Linq;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Snowberry.UI;

namespace Snowberry.Editor.Recording;

public class FlagsRecorder : Recorder {

    public override string Name() => Dialog.Clean("SNOWBERRY_EDITOR_PT_FLAGS");

    private readonly List<(string flag, List<(bool value, float time)> snapshots)> States = [];

    public override void UpdateInGame(Level l, float time) {
        HashSet<string> toTrack = [..l.Session.Flags];

        foreach(var state in States){
            bool value = toTrack.Contains(state.flag);
            if (state.snapshots.Last().value != value)
                state.snapshots.Add((value, time));
            if (value)
                toTrack.Remove(state.flag);
        }

        // discovered new flags to track
        foreach(string flag in toTrack)
            States.Add((flag, [(l.Session.GetFlag(flag), time)]));
    }

    public override void RenderScreenSpace(float time) {
        if (States.Count == 0)
            return;

        int screenHeight = (UIScene.Instance?.UI?.Height ?? 0);
        float maxWidth =
            States.Select(x =>x .flag).Select(MeasureWidth).Max()
            + MeasureWidth(" :") + 10
            + new[]{ true, false }.Select(Display).Select(MeasureWidth).Max();
        int h = States.Count * 12 + 10;
        Draw.Rect(0, screenHeight - h, 10 + maxWidth, h, Color.Gray * 0.5f);
        for (var idx = 0; idx < States.Count; idx++) {
            var flagStates = States[idx];
            float y = screenHeight - (idx + 1) * 12 - 5;
            Fonts.Regular.Draw(flagStates.flag, new(5, y), new(), Color.White);
            bool value = false;
            foreach (var state in flagStates.snapshots.AsEnumerable().Reverse())
                if (state.time <= time) {
                    value = state.value;
                    break;
                }
            string text = flagStates.flag + " :";
            Fonts.Regular.Draw(text, new(5, y), new(1), Color.White);
            Fonts.Regular.Draw(Display(value), new(MeasureWidth(text) + 10, y), new(1), DisplayColor(value));
        }
    }

    public override void RenderWorldSpace(float time) {}

    public bool? GetFlagAt(string flag, float time) {
        foreach (var stateSet in States) {
            if(stateSet.flag == flag) {
                bool? value = null;
                foreach (var state in stateSet.snapshots.AsEnumerable().Reverse())
                    if (state.time <= time) {
                        value = state.value; break;
                    }

                return value;
            }
        }

        return null;
    }

    private static string Display(bool b) => Dialog.Clean(b switch {
        true => "SNOWBERRY_EDITOR_PT_FLAG_TRUE",
        false => "SNOWBERRY_EDITOR_PT_FLAG_FALSE"
    });

    private static Color DisplayColor(bool? b) => b switch {
        true => Color.LightGreen,
        false => Color.Red,
        null => Color.Gray
    };

    private static float MeasureWidth(string s) => Fonts.Regular.Measure(s).X;
}
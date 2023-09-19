using System.Collections.Generic;
using Celeste;

namespace Snowberry.Editor.Recording;

public class TimeRecorder : Recorder {

    public List<float> FrameTimes = new();
    public float MaxTime = 0;

    public override void UpdateInGame(Level l, float time) {
        MaxTime = time; // if this ever goes down, something has gone seriouesly wrong
        FrameTimes.Add(time);
    }

    public override void RenderScreenSpace(float time) {}
    public override void RenderWorldSpace(float time) {}
}
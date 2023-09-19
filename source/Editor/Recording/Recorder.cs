using System;
using System.Collections.Generic;
using Celeste;

namespace Snowberry.Editor.Recording;

public abstract class Recorder{

    public static readonly List<Func<Recorder>> Recorders = new() {
        () => new PlayerRecorder(),
        () => new CameraRecorder(),
        () => new HitboxesRecorder(),
        () => new TimeRecorder()
    };

    public abstract void UpdateInGame(Level l, float time);
    public virtual void FinalizeRecording(){}

    public abstract void RenderScreenSpace(float time);
    public abstract void RenderWorldSpace(float time);
}
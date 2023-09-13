using System;
using System.Collections.Generic;
using Celeste;

namespace Snowberry.Editor.Recording;

public abstract class Recorder{

    public static readonly List<Func<Recorder>> Recorders = new() {
        () => new PlayerRecorder(),
        () => new CameraRecorder(),
        () => new HitboxesRecorder()
    };

    public abstract void UpdateInGame(Level l);

    public abstract void RenderScreenSpace(float time);
    public abstract void RenderWorldSpace(float time);
}
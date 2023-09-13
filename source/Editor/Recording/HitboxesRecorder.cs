using System.Collections.Generic;
using System.Linq;
using Celeste;
using Monocle;

namespace Snowberry.Editor.Recording;

public class HitboxesRecorder : Recorder{

    private readonly List<(List<Collider> colliders, float time)> States = new();

    // TODO: can cause OOMS from inefficiency
    public override void UpdateInGame(Level l){
        States.Add((l.Entities.Select(x => {
            var collider = x.Collider?.Clone();
            if (collider != null) collider.Position += x.Position;
            return collider;
        }).Where(x => x != null).ToList(), l.TimeActive));
    }

    public override void RenderScreenSpace(float time){}

    public override void RenderWorldSpace(float time){
        foreach (var state in States)
            if (time <= state.time) {
                foreach (Collider c in state.colliders)
                    c.Render(null);
                break;
            }
    }
}
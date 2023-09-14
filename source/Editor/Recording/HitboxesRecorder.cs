using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Recording;

using CEntity = Monocle.Entity;

public class HitboxesRecorder : Recorder{

    // split things up into "each entity at a specific time", so the various entities that don't do much with theirs
    // can be more efficient about it.
    private List<(WeakReference<CEntity> entityRef, List<(Collider collider, Vector2 offset, bool collidable, float time)> cs)> OldStates = new(), InProgressStates = new();

    public override void UpdateInGame(Level l, float time){
        HashSet<CEntity> toTrack = new(l.Entities);

        foreach(var state in InProgressStates /* copy to allow mutation */ .ToList()){
            if(state.entityRef.TryGetTarget(out var entity) && toTrack.Contains(entity)){
                // all entities should have at least one state
                var lastState = state.cs.Last();
                // only create a new entity state if necessary
                if (!CollidersEq(lastState.collider, entity.Collider) || lastState.offset != entity.Position || lastState.collidable != entity.Collidable)
                    state.cs.Add((entity.Collider?.Clone(), entity.Position, entity.Collidable, time));
                // no need to look at it anymore
                toTrack.Remove(entity);
            }else{
                // no need to think about this one anymore
                InProgressStates.Remove(state);
                OldStates.Add(state);
                // explicitly mark it as done
                state.cs.Add((null, new(0), false, time));
            }
        }

        // discovered new entities to track
        foreach(CEntity entity in toTrack)
            if(entity.Collider != null)
                InProgressStates.Add((new WeakReference<CEntity>(entity), new() { (entity.Collider.Clone(), entity.Position, entity.Collidable, time) }));
    }

    public override void FinalizeRecording(){
        // move entities that were still alive at the end to the old list
        OldStates.AddRange(InProgressStates);
        InProgressStates = null;

        void MoveCollider(Collider c, Vector2 v) {
            c.Position += v;
            if(c is ColliderList cl)
                foreach (Collider sub in cl.colliders)
                    MoveCollider(sub, v);
        }

        foreach(var state in OldStates.SelectMany(entityStates => entityStates.cs))
            if(state.collider != null)
                MoveCollider(state.collider, state.offset);
    }

    public override void RenderScreenSpace(float time){}

    public override void RenderWorldSpace(float time) {
        Editor.BufferCamera sbCam = Editor.Instance.Camera;
        Camera culler = new Camera(sbCam.ViewRect.Width, sbCam.ViewRect.Height) {
            Position = sbCam.ViewRect.Location.ToVector2()
        };

        foreach (var entityStates in OldStates)
            foreach (var state in entityStates.cs.AsEnumerable().Reverse())
                if (state.time <= time) {
                    state.collider?.Render(culler, state.collidable ? Color.Red : Color.DarkRed);
                    break;
                }
    }

    // TODO: provide a way for modded Colliders to work better
    private static bool CollidersEq(Collider l, Collider r) {
        return l switch {
            Hitbox hl => r is Hitbox hr && hl.Size == hr.Size && hl.Position == hr.Position,
            Circle cl => r is Circle cr && cl.Radius == cr.Radius && cl.Position == cr.Position,
            ColliderList ll => r is ColliderList lr && ll.colliders.Zip(lr.colliders, CollidersEq).All(x => x),
            Grid => r is Grid,
            _ => false
        };
    }
}
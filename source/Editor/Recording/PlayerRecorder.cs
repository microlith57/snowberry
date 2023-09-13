using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;

namespace Snowberry.Editor.Recording;

public class PlayerRecorder : Recorder{

    private readonly List<Player.ChaserState> States = new();
    private readonly PlayerSprite Sprite;
    private readonly PlayerHair Hair;
    // TODO: death animation...

    public PlayerRecorder() {
        Sprite = new PlayerSprite(PlayerSpriteMode.Madeline);
        Hair = new PlayerHair(Sprite);
    }

    public override void UpdateInGame(Level l){
        if(l.Tracker.GetEntity<Player>() is { ChaserStates.Count: > 0 } player)
            States.Add(player.ChaserStates[player.ChaserStates.Count - 1]);
    }

    public override void RenderScreenSpace(float time){}

    public override void RenderWorldSpace(float time){
        Player.ChaserState? display = States.FirstOrDefault(chaserState => time <= chaserState.TimeStamp);
        if(display is { /*non null*/ } state){
            Sprite.Visible = true;
            Hair.Visible = true;

            Sprite.RenderPosition = state.Position;
            if (state.Animation != Sprite.CurrentAnimationID && state.Animation != null && Sprite.Has(state.Animation))
                Sprite.Play(state.Animation, true);
            Sprite.Scale = state.Scale;
            if (Sprite.Scale.X != 0.0)
                Hair.Facing = (Facings) Math.Sign(Sprite.Scale.X);
            Hair.Color = state.HairColor;
            if (Sprite.Mode == PlayerSpriteMode.Playback)
                Sprite.Color = Hair.Color;

            Sprite.Update();
            Hair.Update();
            Hair.AfterUpdate();

            Hair.Render();
            Sprite.Render();
        }else{
            // ReSharper disable once HeuristicUnreachableCode
            Sprite.Visible = false;
            Hair.Visible = false;
        }
    }
}
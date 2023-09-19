using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Snowberry.UI;
using Snowberry.UI.Menus;

namespace Snowberry.Editor.Recording;

public class PlayerRecorder : Recorder{

    private readonly List<Player.ChaserState> States = new();
    private PlayerSprite Sprite;
    private PlayerHair Hair;
    private PlayerSpriteMode Mode;
    // TODO: death animation...

    public PlayerRecorder() {
        Mode = PlayerSpriteMode.Madeline;
        Sprite = new PlayerSprite(Mode);
        Hair = new PlayerHair(Sprite);
    }

    public override void UpdateInGame(Level l, float time){
        if(l.Tracker.GetEntity<Player>() is { ChaserStates.Count: > 0 } player) {
            Player.ChaserState state = player.ChaserStates[player.ChaserStates.Count - 1];
            state.TimeStamp = time; // it's a struct, so no need to copy
            States.Add(state);
        }
    }

    public override void RenderScreenSpace(float time){}

    public override void RenderWorldSpace(float time){
        Player.ChaserState? display = States.LastOrDefault(chaserState => chaserState.TimeStamp <= time);
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

    public override string Name() => Dialog.Clean("SNOWBERRY_EDITOR_PT_PLAYER");
    public override UIElement CreateOptionsPane() {
        UIElement orig = base.CreateOptionsPane();
        orig.AddBelow(UIPluginOptionList.DropdownOption(Dialog.Clean("SNOWBERRY_EDITOR_PT_OPTS_SKIN"), Mode, mode => {
            Mode = mode;
            ChangeMode(ref Sprite, ref Hair, Mode);
        }), new(6, 3));
        orig.CalculateBounds();
        orig.Height += 3;
        return orig;
    }

    private static void ChangeMode(ref PlayerSprite sprite, ref PlayerHair hair, PlayerSpriteMode mode) {
        string currentAnimationId = sprite.CurrentAnimationID;
        int currentAnimationFrame = sprite.CurrentAnimationFrame;
        sprite = new PlayerSprite(mode);
        if (sprite.Has(currentAnimationId)) {
            sprite.Play(currentAnimationId);
            if (currentAnimationFrame < sprite.CurrentAnimationTotalFrames)
                sprite.SetAnimationFrame(currentAnimationFrame);
        }

        hair.Sprite = sprite;
    }
}
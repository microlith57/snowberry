using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Microsoft.Xna.Framework;
using Snowberry.UI;

namespace Snowberry.Editor.Recording;

public class PlayerRecorder : Recorder{

    private readonly List<Player.ChaserState> States = new();
    private PlayerSprite Sprite;
    private PlayerHair Hair;
    // TODO: death animation...

    public PlayerRecorder() {
        SmhInterop.RunWithSkin(() =>
            Sprite = new PlayerSprite(PlayerSpriteMode.Madeline)
        );
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
        // it's kind of like if UIPluginOptionList was evil
        UIButton button = null;
        button = new UIButton(Dialog.Clean($"SNOWBERRY_EDITOR_PT_SKIN_MADELINE") + " \uF036", Fonts.Regular, 2, 2) {
            OnPress = () => {
                List<UIDropdown.DropdownEntry> entries = new();
                foreach (PlayerSpriteMode sm in Enum.GetValues(typeof(PlayerSpriteMode)).OfType<PlayerSpriteMode>()) {
                    string name = Dialog.Clean($"SNOWBERRY_EDITOR_PT_SKIN_{sm.ToString().ToUpperInvariant()}");
                    entries.Add(new UIDropdown.DropdownEntry(name, () => {
                        UpdateSprite(sm, "Default");
                        button.SetText(name + " \uF036");
                    }));
                }
                foreach (var skin in SmhInterop.PlayerSkinIds) {
                    string skinName = Dialog.Clean(skin.key);
                    if (skinName == "")
                        skinName = skin.id;
                    string name = Dialog.Get("SNOWBERRY_EDITOR_PT_OPTS_SMH").Substitute(skinName);
                    entries.Add(new UIDropdown.DropdownEntry(name, () => {
                        UpdateSprite(PlayerSpriteMode.Madeline, skin.id);
                        button.SetText(name + " \uF036");
                    }));
                }

                var dropdown = new UIDropdown(Fonts.Regular, entries.ToArray()) {
                    Position = button.GetBoundsPos() + Vector2.UnitY * (button.Height + 2) - Editor.Instance.ToolPanel.GetBoundsPos()
                };

                Editor.Instance.ToolPanel.Add(dropdown);
            }
        };
        orig.AddBelow(button, new(6, 3));
        /*orig.AddBelow(UIPluginOptionList.DropdownOption(Dialog.Clean("SNOWBERRY_EDITOR_PT_OPTS_SKIN"), Mode, mode => {
            Mode = mode;
            ChangeMode(ref Sprite, ref Hair, Mode);
        }), new(6, 3));*/
        orig.CalculateBounds();
        orig.Height += 3;
        return orig;
    }

    private void UpdateSprite(PlayerSpriteMode mode, string skin) =>
        SmhInterop.RunWithSkin(() =>
            UpdateSprite(ref Sprite, ref Hair, mode), skin);

    private static void UpdateSprite(ref PlayerSprite sprite, ref PlayerHair hair, PlayerSpriteMode mode) {
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
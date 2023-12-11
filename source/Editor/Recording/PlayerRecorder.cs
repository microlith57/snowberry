using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using Snowberry.UI;
using Snowberry.UI.Controls;
using Snowberry.UI.Menus;

namespace Snowberry.Editor.Recording;

public class PlayerRecorder : Recorder{

    private record struct PlayerState(
        Player.ChaserState underlying,
        Vector2 Speed,
        int State,
        int Dashes,
        float Stamina,
        float CoyoteTimer
    ) {
        public static PlayerState Observe(Player p) => new(
            p.ChaserStates[^1],
            p.Speed,
            p.StateMachine.State,
            p.Dashes,
            p.Stamina,
            DynamicData.For(p).Get<float>("jumpGraceTimer")
        );

        public float TimeStamp {
            get => underlying.TimeStamp;
            set {
                var tmp = underlying;
                tmp.TimeStamp = value;
                underlying = tmp;
            }
        }
    }

    private readonly List<PlayerState> States = [];
    private PlayerSprite Sprite;
    private PlayerHair Hair;
    private string Skin = "Default";
    private bool ShowPreciseData = false;
    // TODO: death animation...

    public PlayerRecorder() {
        SmhInterop.RunWithSkin(() => {
            Sprite = new PlayerSprite(PlayerSpriteMode.Madeline);
            Hair = new PlayerHair(Sprite);
            // mods like Gravity Helper expect a non-null entity here, though they place no requirements on what it is
            Monocle.Entity dummy = new();
            Sprite.Added(dummy);
            Hair.Added(dummy);
        });
    }

    public override void UpdateInGame(Level l, float time){
        if(l.Tracker.GetEntity<Player>() is { ChaserStates.Count: > 0 } player) {
            PlayerState state = PlayerState.Observe(player);
            state.TimeStamp = time; // it's a struct, so no need to copy
            States.Add(state);
        }
    }

    public override void RenderScreenSpace(float time){}

    public override void RenderWorldSpace(float time){
        PlayerState? display = States.LastOrDefault(chaserState => chaserState.TimeStamp <= time);
        if(display is { /*non null*/ } state){
            Sprite.Visible = true;
            Hair.Visible = true;

            Player.ChaserState cs = state.underlying;
            Sprite.RenderPosition = cs.Position;
            if (cs.Animation != Sprite.CurrentAnimationID && cs.Animation != null && Sprite.Has(cs.Animation))
                Sprite.Play(cs.Animation, true);
            Sprite.Scale = cs.Scale;
            if (Sprite.Scale.X != 0.0)
                Hair.Facing = (Facings) Math.Sign(Sprite.Scale.X);
            Hair.Color = cs.HairColor; // note that this sets it to the hair colours of whatever skinmod was enabled
            if (Sprite.Mode == PlayerSpriteMode.Playback)
                Sprite.Color = Hair.Color;

            Sprite.Update();
            Hair.Update();
            Hair.AfterUpdate();

            SmhInterop.RunWithSkin(() => Hair.Render(), Skin); // fix bangs sprite
            Sprite.Render();

            // TODO: expose this in a reasonable format
            /*if (ShowPreciseData) {
                string data = $"""
                              State: {state.State}
                              Position: {state.underlying.Position}
                              Speed: {state.Speed}
                              Dashes: {state.Dashes}
                              Stamina: {state.Stamina}
                              """;
                if (state.CoyoteTimer > 0)
                    data += $"\nCoyote Timer: {state.CoyoteTimer}";
                var area = Fonts.Regular.Measure(data);
                Draw.Rect(cs.Position.X - area.X - 5, cs.Position.Y - area.Y - 5, area.X + 10, area.Y + 10, Color.Gray * 0.5f);
                Fonts.Regular.Draw(data, cs.Position - new Vector2(area.X - 5, -5), new(1), Color.White);
            }*/
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
        button = new UIButton(Dialog.Clean("SNOWBERRY_EDITOR_PT_SKIN_MADELINE") + " \uF036", Fonts.Regular, 2, 2) {
            OnPress = () => {
                List<UIDropdown.DropdownEntry> entries = [];
                foreach (PlayerSpriteMode sm in Enum.GetValues(typeof(PlayerSpriteMode)).OfType<PlayerSpriteMode>()) {
                    string name = Dialog.Clean($"SNOWBERRY_EDITOR_PT_SKIN_{sm.ToString().ToUpperInvariant()}");
                    entries.Add(new UIDropdown.DropdownEntry(name, () => {
                        UpdateSprite(sm, Skin = "Default");
                        button.SetText(name + " \uF036");
                    }));
                }
                foreach (var skin in SmhInterop.PlayerSkinIds) {
                    string skinName = Dialog.Clean(skin.key);
                    if (skinName == "")
                        skinName = skin.id;
                    string name = Dialog.Get("SNOWBERRY_EDITOR_PT_OPTS_SMH").Substitute(skinName);
                    entries.Add(new UIDropdown.DropdownEntry(name, () => {
                        UpdateSprite(PlayerSpriteMode.Madeline, Skin = skin.id);
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
        //orig.AddBelow(UIPluginOptionList.BoolOption("precise info", ShowPreciseData, u => ShowPreciseData = u), new(6, 3));
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
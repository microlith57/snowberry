using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Triggers;

[Plugin("minitextboxTrigger")]
public class Plugin_MiniTextboxTrigger : Trigger {

    [Option("mode")] public Modes Mode = Modes.OnPlayerEnter;
    [Option("dialog_id")] public string DialogId = "";
    [Option("only_once")] public bool OnlyOnce = false;
    [Option("death_count")] public int DeathCount = -1;

    public override void Render() {
        base.Render();
        var str = $"\"{DialogId}\"";
        Fonts.Pico8.Draw(str, Center + new Vector2(0, 6), Vector2.One, new(0.5f), Color.Black);
    }

    public new static void AddPlacements() {
        Placements.Create("Mini Textbox Trigger", "minitextboxTrigger", trigger: true);
    }

    public enum Modes{
        OnPlayerEnter,
        OnLevelStart,
        OnTheoEnter
    }
}
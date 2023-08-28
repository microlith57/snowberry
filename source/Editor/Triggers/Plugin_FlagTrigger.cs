using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Triggers;

[Plugin("everest/flagTrigger")]
public class Plugin_FlagTrigger : Trigger {

    [Option("flag")] public string Flag = "";
    [Option("state")] public bool State = false;
    [Option("mode")] public Modes Mode = Modes.OnPlayerEnter;
    [Option("only_once")] public bool OnlyOnce = false;
    [Option("death_count")] public int DeathCount = -1;

    public override void Render() {
        base.Render();
        string prefix = State ? "" : "!";
        var str = (Flag != "") ? $"({prefix}{Flag})" : "";
        Fonts.Pico8.Draw(str, Center + new Vector2(0, 6), Vector2.One, new(0.5f), Color.Black);
    }

    public new static void AddPlacements() {
        Placements.Create("Flag (Everest)", "everest/flagTrigger");
    }

    public enum Modes{
        OnPlayerEnter,
        OnPlayerLeave,
        OnLevelStart
    }
}
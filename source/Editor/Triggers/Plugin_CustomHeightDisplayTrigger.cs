using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Triggers;

[Plugin("everest/customHeightDisplayTrigger")]
public class Plugin_CustomHeightDisplayTrigger : Trigger {
    [Option("vanilla")] public bool Vanilla = false;
    [Option("target")] public float Target = 0;
    [Option("from")] public float From = 0;
    [Option("text")] public string text = "{x}m";
    [Option("progressAudio")] public bool ProgressAudio = false;
    [Option("displayOnTransition")] public bool DisplayOnTransition = false;

    public override void Render() {
        base.Render();

        var postfix = (text.IndexOf("}") != text.Length) ? text.Substring(text.LastIndexOf("}") + 1) : "m";

        var str = (From == Target) ? $"({Target}{postfix})" : $"({From}{postfix} -> {Target}{postfix})";
        Fonts.Pico8.Draw(str, Center + Vector2.UnitY * 6, Vector2.One, new(0.5f), Color.Black);
    }

    public new static void AddPlacements() {
        Placements.Create("Custom Height Display Trigger (Everest)", "everest/customHeightDisplayTrigger");
    }
}
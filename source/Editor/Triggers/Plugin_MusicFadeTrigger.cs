using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Triggers;

[Plugin("musicFadeTrigger")]
public class Plugin_MusicFadeTrigger : Trigger {
    [Option("direction")] public MusicFadeDirections Direction = MusicFadeDirections.leftToRight;
    [Option("parameter")] public string Parameter = "";
    [Option("fadeA")] public float From = 0;
    [Option("fadeB")] public float To = 0;

    public override void Render() {
        base.Render();
        var str = $"(\"{Parameter}\": {From} -> {To})";
        Fonts.Pico8.Draw(str, Center + Vector2.UnitY * 6, Vector2.One, new(0.5f), Color.Black);
    }

    public new static void AddPlacements() {
        Placements.Create("Music Fade Trigger", "musicFadeTrigger");
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")] // match MusicFadeTrigger
    public enum MusicFadeDirections {
        leftToRight, topToBottom
    }
}
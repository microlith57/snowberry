using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Triggers;

[Plugin("everest/musicLayerTrigger")]
public class Plugin_MusicLayerTrigger : Trigger {

    [Option("layers")] public string Track = "";
    [Option("enable")] public bool Enable = false;

    public override void Render() {
        base.Render();

        var str = (Track == "") ? "" : $"(\"{Track}\", {Enable})";
        Fonts.Pico8.Draw(str, Center + Vector2.UnitY * 6, Vector2.One, new(0.5f), Color.Black);
    }

    public new static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Music Layer (Everest)", "everest/musicLayerTrigger", trigger: true);
    }
}
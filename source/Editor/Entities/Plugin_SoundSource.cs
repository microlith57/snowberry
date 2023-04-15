using Celeste;

namespace Snowberry.Editor.Entities;

[Plugin("soundSource")]
public class Plugin_SoundSource : Entity {

    // any string is allowed, TODO: suggestions
    [Option("sound")]
    public string Sound = "";

    public override void Render() {
        base.Render();

        GFX.Game["plugins/Snowberry/sound_source"].DrawCentered(Position);
    }

    public static void AddPlacements() {
        Placements.Create("Sound Source", "soundSource");
    }
}
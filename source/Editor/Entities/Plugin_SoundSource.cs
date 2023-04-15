using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;

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

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(24), justify: new(0.5f));
    }

    public static void AddPlacements() {
        Placements.Create("Sound Source", "soundSource");
    }
}
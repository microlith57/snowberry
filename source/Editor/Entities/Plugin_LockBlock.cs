using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities;

[Plugin("lockBlock")]
public class Plugin_LockBlock : Entity {

    [Option("sprite")] public LockBlockSprite Sprite = LockBlockSprite.wood;
    [Option("unlock_sfx")] public string UnlockSfx = "";
    [Option("stepMusicProgress")] public bool StepMusicProgress = false;

    public override void Render() {
        base.Render();

        FromSprite("lockdoor_" + Sprite, "idle").DrawCentered(Position + new Vector2(16));
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(32));
    }

    public static void AddPlacements() {
        foreach (var type in new[]{ "Wood", "Temple A", "Temple B", "Moon" })
            Placements.EntityPlacementProvider.Create($"Locked Door ({type})", "lockBlock", new() { ["sprite"] = type.ToLowerInvariant().Replace(' ', '_') });
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")] // must exactly match
    public enum LockBlockSprite {
        wood, temple_a, temple_b, moon
    }
}
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("templeMirrorPortal")]
public class Plugin_TempleMirrorPortal : Entity {

    private static readonly MTexture[] texs = [
        GFX.Game["objects/temple/portal/portalframe"],
        GFX.Game["objects/temple/portal/portalcurtain00"],
        GFX.Game["objects/temple/portal/portaltorch00"]
    ];

    public override void Render() {
        base.Render();

        texs[0].DrawCentered(Position);
        texs[1].DrawCentered(Position);
        texs[2].DrawJustified(Position + new Vector2(90, 0), new(0.5f, 0.75f));
        texs[2].DrawJustified(Position + new Vector2(-90, 0), new(0.5f, 0.75f));
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Temple Mirror Portal", "templeMirrorPortal");
    }
}
using Celeste;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities;

[Plugin("bonfire")]
public class Plugin_Bonfire : Entity {
    [Option("mode")] public Bonfire.Mode Mode = Bonfire.Mode.Unlit;

    public override void Render() {
        base.Render();

        FromSprite("campfire", Mode switch {
            Bonfire.Mode.Lit => "burn",
            Bonfire.Mode.Smoking => "smoking",
            _ => "idle"
        })?.DrawJustified(Position, new Vector2(0.5f, 1.0f));
    }

    public static void AddPlacements() {
        Placements.Create("Campfire", "bonfire");
    }
}
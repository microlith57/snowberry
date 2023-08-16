using Celeste;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Triggers;

[Plugin("spawnFacingTrigger")]
public class Plugin_SpawnFacingTrigger : Trigger{

    [Option("facing")] public Facings Facing = Facings.Left;

    public Plugin_SpawnFacingTrigger(){
        Tracked = true;
    }

    public override void Render(){
        base.Render();
        Fonts.Pico8.Draw(Facing.ToString().ToLower(), Center + new Vector2(0, 6), Vector2.One, new(0.5f), Color.Black);
    }

    public new static void AddPlacements(){
        Placements.Create("Spawn Facing Trigger", "spawnFacingTrigger");
    }
}
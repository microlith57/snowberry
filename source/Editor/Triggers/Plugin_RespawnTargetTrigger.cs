using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Triggers;

[Plugin("respawnTargetTrigger")]
public class Plugin_RespawnTargetTrigger : Trigger {

    public override int MinNodes => 1;
    public override int MaxNodes => 1;

    public override void Render() {
        base.Render();

        DrawUtil.DottedLine(Center, Nodes[0], Color.White * 0.5f, 8, 4);
    }

    public new static void AddPlacements() {
        Placements.Create("Respawn Target Trigger", "respawnTargetTrigger", trigger: true);
    }
}
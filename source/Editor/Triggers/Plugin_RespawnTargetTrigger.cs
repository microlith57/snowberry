namespace Snowberry.Editor.Triggers;

[Plugin("respawnTargetTrigger")]
public class Plugin_RespawnTargetTrigger : Trigger {

    public override int MinNodes => 1;
    public override int MaxNodes => 1;

    public new static void AddPlacements() {
        Placements.Create("Respawn Target Trigger", "respawnTargetTrigger");
    }
}
namespace Snowberry.Editor.Triggers;

[Plugin("changeRespawnTrigger")]
public class Plugin_ChangeRespawnTrigger : Trigger {

    public override int MinNodes => 0;
    public override int MaxNodes => 1;

    public new static void AddPlacements() {
        Placements.Create("Change Respawn Trigger", "changeRespawnTrigger");
    }
}
namespace Snowberry.Editor.Triggers;

[Plugin("everest/lavaBlockerTrigger")]
public class Plugin_LavaBlockerTrigger : Trigger {

    [Option("canReenter")] public bool CanReenter = false;

    public new static void AddPlacements() {
        Placements.Create("Lava Blocker (Everest)", "everest/lavaBlockerTrigger");
    }
}
namespace Snowberry.Editor.Triggers;

[Plugin("everest/activateDreamBlocksTrigger")]
public class Plugin_ActivateDreamBlocksTrigger : Trigger {

    [Option("fullRoutine")] public bool FullRoutine = false;
    [Option("activate")] public bool Activate = true;
    [Option("fastAnimation")] public bool FastAnimation = false;

    public new static void AddPlacements() {
        Placements.Create("Activate Dream Blocks Trigger (Everest)", "everest/activateDreamBlocksTrigger", trigger: true);
    }
}
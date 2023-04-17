namespace Snowberry.Editor.Triggers;

[Plugin("cameraTargetTrigger")]
public class Plugin_CameraTargetTrigger : Trigger {
    [Option("deleteFlag")] public string DeleteFlag = "";
    [Option("lerpStrength")] public float LerpStrength = 1;
    [Option("positionMode")] public Celeste.Trigger.PositionModes PositionMode = Celeste.Trigger.PositionModes.NoEffect;
    [Option("xOnly")] public bool XOnly = false;
    [Option("yOnly")] public bool YOnly = false;

    public override int MinNodes => 1;
    public override int MaxNodes => 1;

    public new static void AddPlacements() {
        Placements.Create("Camera Target Trigger", "cameraTargetTrigger");
    }
}
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Triggers;

[Plugin("cameraAdvanceTargetTrigger")]
public class Plugin_CameraAdvanceTargetTrigger : Trigger {
    [Option("lerpStrengthX")] public float LerpStrengthX = 1;
    [Option("lerpStrengthY")] public float LerpStrengthY = 1;
    [Option("positionModeX")] public Celeste.Trigger.PositionModes PositionModeX = Celeste.Trigger.PositionModes.NoEffect;
    [Option("positionModeY")] public Celeste.Trigger.PositionModes PositionModeY = Celeste.Trigger.PositionModes.NoEffect;
    [Option("xOnly")] public bool XOnly = false;
    [Option("yOnly")] public bool YOnly = false;

    public override int MinNodes => 1;
    public override int MaxNodes => 1;

    public override void Render() {
        base.Render();

        DrawUtil.DottedLine(Center, Nodes[0], Color.White * 0.5f, 8, 4);
    }

    public new static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Camera Advance Target Trigger", "cameraAdvanceTargetTrigger", trigger: true);
    }
}
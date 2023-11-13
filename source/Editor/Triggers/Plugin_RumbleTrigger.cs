using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Triggers;

[Plugin("rumbleTrigger")]
public class Plugin_RumbleTrigger : Trigger {
    [Option("manualTrigger")] public bool ManualTrigger = false;
    [Option("persistent")] public bool Persistent = false;
    [Option("constrainHeight")] public bool ConstrainHeight = false;

    public override int MinNodes => 2;
    public override int MaxNodes => 2;

    public override void Render() {
        base.Render();

        DrawUtil.DottedLine(Center, Nodes[0], Color.White * 0.5f, 8, 4);
        DrawUtil.DottedLine(Center, Nodes[1], Color.White * 0.5f, 8, 4);
    }

    public new static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Rumble", "rumbleTrigger", trigger: true);
    }
}
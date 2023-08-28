using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Triggers;

[Plugin("everest/smoothCameraOffsetTrigger")]
public class Plugin_SmoothCameraOffsetTrigger : Trigger {
    [Option("offsetXFrom")] public float OffsetXFrom = 0.0f;
    [Option("offsetXTo")] public float OffsetXTo = 0.0f;
    [Option("offsetYFrom")] public float OffsetYFrom = 0.0f;
    [Option("offsetYTo")] public float OffsetYTo = 0.0f;
    [Option("positionMode")] public Celeste.Trigger.PositionModes PositionMode = Celeste.Trigger.PositionModes.NoEffect;
    [Option("onlyOnce")] public bool OnlyOnce = false;
    [Option("xOnly")] public bool XOnly = false;
    [Option("yOnly")] public bool YOnly = false;


    public override void Render() {
        base.Render();

        string deltaX = (OffsetXFrom != OffsetXTo && !YOnly) ? $"(X: {OffsetXFrom} -> {OffsetXTo})" : "";
        string deltaY = (OffsetYFrom != OffsetYTo && !XOnly) ? $"(Y: {OffsetYFrom} -> {OffsetYTo})" : "";
        int yTextScale = (YOnly) ? 6 : 12;
        Fonts.Pico8.Draw(deltaX, Center + Vector2.UnitY * 6, Vector2.One, new Vector2(0.5f, 0.5f), Color.Black);
        Fonts.Pico8.Draw(deltaY, Center + Vector2.UnitY * yTextScale, Vector2.One, new Vector2(0.5f, 0.5f), Color.Black);
    }

    public new static void AddPlacements() {
        Placements.Create("Smooth Camera Offset Trigger (Everest)", "everest/smoothCameraOffsetTrigger");
    }
}
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities;

[Plugin("coreModeToggle")]
public class Plugin_CoreModeToggle : Entity {

    [Option("mode")] public CoreToggleMode Mode = CoreToggleMode.Both;
    [Option("persistent")] public bool Persistent = false;

    public override void Render() {
        base.Render();

        string postfix;
        switch (Mode) {
            case CoreToggleMode.OnlyFire: {
                postfix = "15";
                break;
            }
            case CoreToggleMode.OnlyIce: {
                postfix = "13";
                break;
            }
            default:
                postfix = "01";
                break;
        }

        GFX.Game[$"objects/coreFlipSwitch/switch{postfix}"].DrawCentered(Position);
    }

    protected override IEnumerable<Rectangle> Select() {

        float iceModSize = Mode == CoreToggleMode.OnlyIce ? 3 : 0;
        float iceModPos = Mode == CoreToggleMode.OnlyIce ? -8.5f : 0;
        yield return RectOnRelative(new(16, 20 + iceModSize), position: new(0, 5 + iceModPos), justify: new(0.5f, 0.5f));
    }

    public override void SaveAttrs(BinaryPacker.Element e) {
        if (Mode == CoreToggleMode.OnlyFire)
            e.Attributes["onlyFire"] = true;
        if (Mode == CoreToggleMode.OnlyIce)
            e.Attributes["onlyIce"] = true;
        e.Attributes["persistent"] = Persistent;
    }
    public static void AddPlacements() {
        Placements.Create("Core Mode Toggle", "coreModeToggle");
    }

    public enum CoreToggleMode {
        Both,
        OnlyFire,
        OnlyIce
    }
}
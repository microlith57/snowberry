using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("cutsceneNode")]
public class Plugin_CutsceneNode : Entity {

    [Option("nodeName")] public string NodeName = "";

    public override void Render() {
        base.Render();

        MTexture icon = GFX.Game["plugins/Snowberry/cutscene_node"];
        icon.DrawCentered(Position);
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(24), justify: new(0.5f));
    }

    public static void AddPlacements() {
        Placements.Create("Cutscene Node", "cutsceneNode");
    }
}
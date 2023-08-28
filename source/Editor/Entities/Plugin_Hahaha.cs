using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("hahaha")]
public class Plugin_Hahaha : Entity {

    [Option("ifset")] public string IfSet = "";
    [Option("triggerLaughSfx")] public bool TriggerLaughSFX = false;

    public override int MaxNodes => 1;

    public override void Render() {
        base.Render();

        // This rendering is like, fucked somehow. No clue why. See discord for explanation
        // https://discord.com/channels/920506235927793675/1144069604491149364/1145856399335772221
        DrawHahahas(Position, 1);
        if (Nodes.Count != 0) {
            DrawHahahas(Nodes[0], 0.5f);
        }
    }

    public override void HQRender() {
        base.HQRender();

        if (Nodes.Count != 0)
            DrawUtil.DottedLine(Center, Nodes[0] + new Vector2(Width, Height) / 2f, Color.White, 4, 2);
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(30, 10), justify: new(0.5f, 0.5f));

        if (Nodes.Count != 0)
            foreach (var node in Nodes)
                yield return RectOnAbsolute(new(30, 10), position: node, justify: new(0.5f, 0.5f));
    }

    // Commented until rendering is fixed
    //public static void AddPlacements() {
        //Placements.Create("Hahaha", "hahaha");
    //}

    public void DrawHahahas(Vector2 pos, float opacity) {
        MTexture ha = GFX.Game["characters/oldlady/ha00"];

        ha.DrawCentered(pos + new Vector2(-10, -1), Color.White * opacity);
        ha.DrawCentered(pos, Color.White * opacity);
        ha.DrawCentered(pos + new Vector2(10, -1), Color.White * opacity);
    }
}
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("summitGemManager")]
public class Plugin_SummitGemManager : Entity {

    public override int MinNodes => 6;
    public override int MaxNodes => 6;

    public override void Render() {
        base.Render();

        MTexture icon = GFX.Game["plugins/Snowberry/summit_gem_manager"];
        icon.DrawCentered(Position);

        for (int i = 0; i < Nodes.Count; i++) {
            MTexture gemTex = GFX.Game[$"collectables/summitgems/{i}/gem00"];
            gemTex.DrawCentered(Nodes[i], Color.White * 0.5f);
        }
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(24), justify: new(0.5f));
        foreach (var node in Nodes)
            yield return RectOnAbsolute(new(20, 20), position: node, justify: new(0.5f));
    }

    //This should absolutely not be placeable, as we are sane individuals. - Gamation

}
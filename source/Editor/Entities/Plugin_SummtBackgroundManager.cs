using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("summitBackgroundManager")]
public class Plugin_SummitBackgroundManager : Entity {

    [Option("index")] public int Index = 0;
    [Option("cutscene")] public string Cutscene = "";
    [Option("intro_launch")] public bool IntroLaunch = false;
    [Option("dark")] public bool Dark = false;
    [Option("ambience")] public string Ambience = "";

    public override void Render() {
        base.Render();

        MTexture icon = GFX.Game["plugins/Snowberry/summit_background_manager"];
        icon.DrawCentered(Position);
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(24), justify: new(0.5f));
    }

    public static void AddPlacements() {
        Placements.Create("Summit Background Manager", "summitBackgroundManager");
    }
}
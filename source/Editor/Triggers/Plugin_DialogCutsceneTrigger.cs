using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Triggers;

[Plugin("everest/dialogTrigger")]
public class Plugin_DialogCutsceneTrigger : Trigger{

    [Option("dialogId")] public string DialogId = "";
    [Option("onlyOnce")] public bool OnlyOnce = true;
    [Option("endLevel")] public bool EndLevel = false;
    [Option("deathCount")] public int DeathCount = -1;

    public override void Render() {
        base.Render();
        var str = $"\"{DialogId}\"";
        Fonts.Pico8.Draw(str, Center + new Vector2(0, 6), Vector2.One, new(0.5f), Color.Black);
    }

    public new static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Dialog Cutscene Trigger (Everest)", "everest/dialogTrigger", trigger: true);
    }
}
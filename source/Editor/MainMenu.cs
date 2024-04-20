using Celeste;
using Monocle;
using Snowberry.Editor.Recording;
using Snowberry.UI;
using Snowberry.UI.Menus;

namespace Snowberry.Editor;

public class MainMenu : UIScene {

    internal static void OpenMainMenu(bool fast = false) {
        Audio.Stop(Audio.CurrentAmbienceEventInstance);
        Audio.Stop(Audio.CurrentMusicEventInstance);
        RecInProgress.DiscardRecording();

        if (fast)
            Engine.Scene = new MainMenu();
        else
            _ = new FadeWipe(Engine.Scene, false, () => Engine.Scene = new MainMenu()) {
                Duration = 0.85f
            };
    }

    protected override void BeginContent() {
        base.BeginContent();
        UI.Add(new UIElement {
            Width = UI.Width,
            Height = UI.Height,
            Background = Util.Colors.DarkGray
        });
        UI.Add(new UIMainMenu(UI.Width, UI.Height));
    }
}
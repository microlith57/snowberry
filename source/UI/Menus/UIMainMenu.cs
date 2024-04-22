using System;
using System.Diagnostics;
using System.IO;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Snowberry.Editor;
using Snowberry.UI.Controls;

namespace Snowberry.UI.Menus;

public class UIMainMenu : UIElement {

    public enum States {
        Start, Create, Load, Exiting, Settings
    }

    private States state = States.Start;
    private readonly float[] stateLerp = [1f, 0f, 0f, 0f, 0f];

    private readonly UIRibbon authors, version;
    private readonly UIButton settings;
    private readonly UIMainMenuButtons buttons;
    private readonly UILevelSelector levelSelector;
    private readonly UIElement settingsOptions;

    private float fade;

    public UIMainMenu(int width, int height) {
        Width = width;
        Height = height;

        Add(levelSelector = new UILevelSelector());

        string mainmenuload = Dialog.Clean("SNOWBERRY_MAINMENU_LOAD");
        string mainmenucreate = Dialog.Clean("SNOWBERRY_MAINMENU_CREATE");
        string mainmenuclose = Dialog.Clean("SNOWBERRY_MAINMENU_CLOSE");

        UIButton create = null, load = null, exit;

        create = new UIButton(mainmenucreate, Fonts.Regular, 16, 24) {
            FG = Util.Colors.White,
            BG = Util.Colors.Cyan,
            PressedBG = Util.Colors.White,
            PressedFG = Util.Colors.Cyan,
            HoveredBG = Util.Colors.DarkCyan,
            OnPress = () => {
                if (state == States.Create) {
                    state = States.Start;
                    create.SetText(mainmenucreate, stayCentered: true);
                } else {
                    state = States.Create;
                    load.SetText(mainmenuload, stayCentered: true);
                    create.SetText(mainmenuclose, stayCentered: true);
                }
            }
        };

        load = new UIButton(mainmenuload, Fonts.Regular, 5, 4) {
            OnPress = () => {
                if (state == States.Load) {
                    state = States.Start;
                    load.SetText(mainmenuload, stayCentered: true);
                } else {
                    state = States.Load;
                    levelSelector.Reload();
                    create.SetText(mainmenucreate, stayCentered: true);
                    load.SetText(mainmenuclose, stayCentered: true);
                }
            }
        };

        exit = new UIButton(Dialog.Clean("SNOWBERRY_MAINMENU_EXIT"), Fonts.Regular, 10, 4) {
            FG = Util.Colors.White,
            BG = Util.Colors.Red,
            PressedBG = Util.Colors.White,
            PressedFG = Util.Colors.Red,
            HoveredBG = Util.Colors.DarkRed,
            OnPress = () => state = States.Exiting
        };

        create.Position = new Vector2(-create.Width / 2, 0);
        load.Position = new Vector2(-load.Width / 2, create.Position.Y + create.Height + 4);
        exit.Position = new Vector2(-exit.Width / 2, load.Position.Y + load.Height + 4);

        buttons = new UIMainMenuButtons();
        Add(buttons);
        RegroupIn(buttons, create, load, exit);
        buttons.Position = new Vector2(width - buttons.Width, height - buttons.Height) / 2;

        settings = new UIButton(Dialog.Clean("SNOWBERRY_MAINMENU_SETTINGS"), Fonts.Regular, 4, 8) {
            OnPress = () => {
                if (state is States.Start or States.Load) {
                    state = States.Settings;
                    load.SetText(mainmenuload, stayCentered: true);
                } else if (state is States.Settings)
                    state = States.Start;
            }
        };
        Add(settings);
        settings.Position = Vector2.UnitX * (Width - settings.Width) + new Vector2(-8, 8);

        settingsOptions = new UIElement {
            Position = new(30, 0),
            Visible = false
        };
        Vector2 settingsOffset = new(0, 25), descOffset = new(5, 3);
        settingsOptions.Add(UIPluginOptionList.BoolOption(Dialog.Clean("SNOWBERRY_SETTINGS_MIDDLE_CLICK_PAN"), Snowberry.Settings.MiddleClickPan, b => {
            Snowberry.Settings.MiddleClickPan = b;
            Snowberry.Instance.SaveSettings();
        }));
        settingsOptions.AddBelow(new UILabel(Dialog.Clean("SNOWBERRY_SETTINGS_MIDDLE_CLICK_PAN_SUB")), descOffset);
        settingsOptions.AddBelow(UIPluginOptionList.BoolOption(Dialog.Clean("SNOWBERRY_SETTINGS_FANCY_RENDER"), Snowberry.Settings.FancyRender, b => {
            Snowberry.Settings.FancyRender = b;
            Snowberry.Instance.SaveSettings();
        }), settingsOffset);
        settingsOptions.AddBelow(new UILabel(Dialog.Clean("SNOWBERRY_SETTINGS_FANCY_RENDER_SUB")), descOffset);
        settingsOptions.AddBelow(UIPluginOptionList.BoolOption(Dialog.Clean("SNOWBERRY_SETTINGS_SG_PREVIEW"), Snowberry.Settings.StylegroundsPreview, b => {
            Snowberry.Settings.StylegroundsPreview = b;
            Snowberry.Instance.SaveSettings();
        }), settingsOffset);
        settingsOptions.AddBelow(new UILabel(Dialog.Clean("SNOWBERRY_SETTINGS_SG_PREVIEW_SUB")), descOffset);
        settingsOptions.AddBelow(UIPluginOptionList.BoolOption(Dialog.Clean("SNOWBERRY_SETTINGS_AGGRESSIVE_SNAP"), Snowberry.Settings.AggressiveSnap, b => {
            Snowberry.Settings.AggressiveSnap = b;
            Snowberry.Instance.SaveSettings();
        }), settingsOffset);
        settingsOptions.AddBelow(new UILabel(Dialog.Clean("SNOWBERRY_SETTINGS_AGGRESSIVE_SNAP_SUB")), descOffset);

        // TODO: reenable when the main menu is a scene & can resize properly
        /*settingsOptions.AddBelow(UIPluginOptionList.BoolOption(Dialog.Clean("SNOWBERRY_SETTINGS_SMALL_SCALE"), Snowberry.Settings.SmallScale, b => {
            Snowberry.Settings.SmallScale = b;
            Snowberry.Instance.SaveSettings();
        }), settingsOffset);
        settingsOptions.AddBelow(new UILabel(Dialog.Clean("SNOWBERRY_SETTINGS_SMALL_SCALE_SUB")), descOffset);*/

        UIButton openBackupsFolder = new(Dialog.Clean("SNOWBERRY_BACKUPS_OPEN_FOLDER"), Fonts.Regular, 6, 6) {
            OnPress = () => {
                string folder = Backups.BackupsDirectory;
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                if (!folder.EndsWith("/")) folder += "/";
                Process.Start("file://" + folder);
            },
            BG = Calc.HexToColor("ff8c00"),
            HoveredBG = Calc.HexToColor("e37e02"),
            PressedBG = Calc.HexToColor("874e07")
        };
        long folderSize = Directory.Exists(Backups.BackupsDirectory) ? Util.DirSize(new DirectoryInfo(Backups.BackupsDirectory)) : 0;
        UILabel backupsTotalSize = new UILabel(Dialog.Get("SNOWBERRY_BACKUPS_TOTAL_SIZE").Substitute(Util.FormatFilesize(folderSize)));
        UIElement group = new();
        group.AddRight(openBackupsFolder);
        group.AddRight(backupsTotalSize, new(6, (openBackupsFolder.Height - backupsTotalSize.Height) / 2f));
        group.CalculateBounds();

        settingsOptions.AddBelow(group, settingsOffset);

        Add(settingsOptions);

        Color rib = Calc.HexToColor("20212e"), acc = Calc.HexToColor("3889d9");
        Add(authors = new UIRibbon(Dialog.Clean("SNOWBERRY_MAINMENU_CREDITS")) {
            Position = new Vector2(0, 8),
            BG = rib,
            BGAccent = acc
        });
        Add(version = new UIRibbon($"ver{Snowberry.Instance.Metadata.VersionString}") {
            Position = new Vector2(0, 23),
            BG = rib,
            BGAccent = acc,
        });
    }

    public override void Update(Vector2 position = default) {
        base.Update(position);

        for (int i = 0; i < stateLerp.Length; i++)
            stateLerp[i] = Calc.Approach(stateLerp[i], ((int)state == i).Bit(), Engine.DeltaTime * 2f);

        switch (state) {
            case States.Exiting:
                fade = Calc.Approach(fade, 1f, Engine.DeltaTime * 2f);
                if (Math.Abs(fade - 1f) < 0.05) {
                    if (SaveData.Instance == null) {
                        SaveData.InitializeDebugMode();
                        SaveData.Instance.CurrentSession_Safe = new Session(AreaKey.Default);
                    }
                    Engine.Scene = new OverworldLoader(Overworld.StartMode.MainMenu);
                }
                break;

            case States.Create:
                fade = Calc.Approach(fade, 1f, Engine.DeltaTime * 2f);
                if (Math.Abs(fade - 1f) < 0.05) {
                    Editor.Editor.OpenNew();
                }

                break;
        }

        float startEase = 1 - Ease.CubeInOut(stateLerp[0]);
        authors.Position.X = (int)Math.Round(startEase * (-authors.Width - 2));
        version.Position.X = (int)Math.Round(startEase * (-version.Width - 2));

        float createEase = Ease.CubeInOut(stateLerp[1]);
        float loadEase = Ease.CubeInOut(stateLerp[2]);
        buttons.Position.X = (int)Math.Round((Width - buttons.Width) / 2 - Width / 3 * loadEase + Width / 3 * createEase);

        levelSelector.Position.X = (int)Math.Round(buttons.Position.X + buttons.Width + 24 - levelSelector.Width * (1 - loadEase));
        levelSelector.Visible = stateLerp[2] != 0f;
        levelSelector.Enabled = stateLerp[2] > 0.9f;

        float settingsEase = Ease.CubeInOut(stateLerp[4]);
        buttons.Position.Y = (int)Math.Round((Height - buttons.Height) / 2 - (Height / 2 + buttons.Height) * settingsEase);
        settingsOptions.Visible = stateLerp[4] != 0;
        settingsOptions.Position.Y = 25 + 1000 * (1 - settingsEase);
    }

    public override void Render(Vector2 position = default) {
        base.Render(position);

        if (fade != 0f)
            Draw.Rect(Bounds, Color.Black * fade);
    }

    public float StateLerp(States state)
        => stateLerp[(int)state];
}
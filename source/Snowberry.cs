using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Monocle;

namespace Snowberry;

public sealed class Snowberry : EverestModule {
    private static Hook hook_MapData_orig_Load, hook_Session_get_MapData;

    public const string PlaytestSid = "Snowberry/Playtest";

    public static Snowberry Instance {
        get;
        private set;
    }

    public static SnowberryModule[] Modules { get; private set; }

    public Snowberry() {
        Instance = this;
    }

    public override Type SettingsType => typeof(SnowberrySettings);
    public static SnowberrySettings Settings => (SnowberrySettings)Instance._Settings;

    // TODO: kind of dirty to leave it here
    public static bool CrawlForLua = false;

    public override void Load() {
        hook_MapData_orig_Load = new Hook(
            typeof(MapData).GetMethod("orig_Load", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(Editor.Editor).GetMethod("CreatePlaytestMapDataHook", BindingFlags.Static | BindingFlags.NonPublic)
        );

        hook_Session_get_MapData = new Hook(
            typeof(Session).GetProperty("MapData", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
            typeof(Editor.Editor).GetMethod("HookSessionGetAreaData", BindingFlags.Static | BindingFlags.NonPublic)
        );

        On.Celeste.Editor.MapEditor.ctor += UsePlaytestMap;
        On.Celeste.MapData.StartLevel += DontCrashOnEmptyPlaytestLevel;
        On.Celeste.LevelEnter.Routine += DontEnterPlaytestMap;
        On.Celeste.Mod.ModContent.Add += HoldLuaAssets;

        Everest.Events.MainMenu.OnCreateButtons += MainMenu_OnCreateButtons;
        Everest.Events.Level.OnCreatePauseMenuButtons += Level_OnCreatePauseMenuButtons;
    }

    public override void LoadContent(bool firstLoad) {
        base.LoadContent(firstLoad);

        LoadModules();
        LoennPluginLoader.LoadPlugins();

        Fonts.Load();
    }

    public override void Unload() {
        hook_MapData_orig_Load?.Dispose();
        hook_Session_get_MapData?.Dispose();

        On.Celeste.Editor.MapEditor.ctor -= UsePlaytestMap;
        On.Celeste.MapData.StartLevel -= DontCrashOnEmptyPlaytestLevel;
        On.Celeste.LevelEnter.Routine -= DontEnterPlaytestMap;
        On.Celeste.Mod.ModContent.Add -= HoldLuaAssets;

        Everest.Events.MainMenu.OnCreateButtons -= MainMenu_OnCreateButtons;
        Everest.Events.Level.OnCreatePauseMenuButtons -= Level_OnCreatePauseMenuButtons;
    }

    private static void LoadModules() {
        List<SnowberryModule> modules = new List<SnowberryModule>();

        foreach (EverestModule module in Everest.Modules) {
            Assembly asm = module.GetType().Assembly;
            foreach (Type type in asm.GetTypesSafe().Where(t => !t.IsAbstract && typeof(SnowberryModule).IsAssignableFrom(t))) {
                ConstructorInfo ctor = type.GetConstructor(new Type[] {});
                if (ctor != null) {
                    SnowberryModule editorModule = (SnowberryModule)ctor.Invoke(new object[] {});

                    PluginInfo.GenerateFromAssembly(asm, editorModule);

                    modules.Add(editorModule);
                    Log(LogLevel.Info, $"Successfully loaded Snowberry Module '{editorModule.Name}'");
                }
            }
        }

        Modules = modules.ToArray();
    }

    private void MainMenu_OnCreateButtons(OuiMainMenu menu, List<MenuButton> buttons) {
        MainMenuSmallButton btn = new MainMenuSmallButton("EDITOR_MAINMENU", "menu/editor", menu, Vector2.Zero, Vector2.Zero, Editor.Editor.OpenMainMenu);
        int c = 2;
        if (Celeste.Celeste.PlayMode == Celeste.Celeste.PlayModes.Debug) c++;
        buttons.Insert(c, btn);
    }

    // from Collab Utils 2, adjusted for Snowberry
    private void Level_OnCreatePauseMenuButtons(Level level, TextMenu menu, bool minimal) {
        int buttonIdx(string label) =>
            menu.GetItems().FindIndex(item =>
                item.GetType() == typeof(TextMenu.Button) && ((TextMenu.Button)item).Label == Dialog.Clean(label));

        if (level.Session.Area.SID == PlaytestSid) {
            // find the position just under "Return to Map".
            var returnToMapIndex = buttonIdx("MENU_PAUSE_RETURN");

            // TODO: uncomment once Playtest is inaccessible through level select
            /*if (returnToMapIndex == -1) {
                // fall back to the bottom of the menu.
                returnToMapIndex = menu.GetItems().Count - 1;
            }*/

            if (returnToMapIndex != -1) {
                // instantiate the "Return to Editor" button
                TextMenu.Button rteBtn = new TextMenu.Button(Dialog.Clean("SNOWBERRY_RETURN_TO_EDITOR"));
                rteBtn.Pressed(() => Editor.Editor.Open(level.Session.MapData, rte: true));

                // replace the "Return to Map" button with "Return to Editor"
                menu.Remove(menu.Items[returnToMapIndex]);
                menu.Insert(returnToMapIndex, rteBtn);
            }

            // find the position just under "Save and Quit".
            int saveAndQuitIndex = buttonIdx("MENU_PAUSE_SAVEQUIT");

            // TODO: uncomment once Playtest is inaccessible through level select
            /*if (saveAndQuitIndex == -1) {
                // fall back to the bottom of the menu.
                saveAndQuitIndex = menu.GetItems().Count - 1;
            }*/

            if (saveAndQuitIndex != -1) {
                // TODO: add confirmation screen w/ "save and quit", "quit without saving", and "cancel"

                // instantiate the "Quit" button
                TextMenu.Button quitBtn = new TextMenu.Button(Dialog.Clean("SNOWBERRY_QUIT"));
                quitBtn.Pressed(() => level.DoScreenWipe(false, () => Engine.Scene = new LevelExit(LevelExit.Mode.SaveAndQuit, level.Session, level.HiresSnow), true));
                // quitBtn.ConfirmSfx = "event:/ui/main/message_confirm";

                // replace the "Save and Quit" button with "Quit"
                menu.Remove(menu.Items[saveAndQuitIndex]);
                menu.Insert(saveAndQuitIndex, quitBtn);
            }

        }
    }

    public static void Log(LogLevel level, string message) {
        Logger.Log(level, "Snowberry", message);
    }

    public static void LogInfo(string message) {
        Log(LogLevel.Info, message);
    }

    private void UsePlaytestMap(On.Celeste.Editor.MapEditor.orig_ctor orig, Celeste.Editor.MapEditor self, AreaKey area, bool reloadMapData) {
        orig(self, area, reloadMapData);
        var selfData = new DynamicData(self);
        if (selfData.Get<Session>("CurrentSession") == Editor.Editor.PlaytestSession) {
            var templates = selfData.Get<List<Celeste.Editor.LevelTemplate>>("levels");
            templates.Clear();
            foreach (LevelData level in Editor.Editor.PlaytestMapData.Levels) {
                templates.Add(new Celeste.Editor.LevelTemplate(level));
            }

            foreach (Rectangle item in Editor.Editor.PlaytestMapData.Filler) {
                templates.Add(new Celeste.Editor.LevelTemplate(item.X, item.Y, item.Width, item.Height));
            }
        }
    }

    private LevelData DontCrashOnEmptyPlaytestLevel(On.Celeste.MapData.orig_StartLevel orig, MapData self) {
        // TODO: just add an empty room
        if (self.Area.SID == PlaytestSid && self.Levels.Count == 0) {
            var empty = new BinaryPacker.Element {
                Children = new List<BinaryPacker.Element>(),
                Attributes = new Dictionary<string, object> {
                    ["name"] = "lvl_empty_map"
                }
            };
            return new LevelData(empty);
        }

        return orig(self);
    }

    private System.Collections.IEnumerator DontEnterPlaytestMap(On.Celeste.LevelEnter.orig_Routine orig, LevelEnter self) {
        var session = new DynamicData(self).Get<Session>("session");
        if (session.Area.SID == PlaytestSid && session != Editor.Editor.PlaytestSession && string.IsNullOrEmpty(LevelEnter.ErrorMessage)) {
            return CantEnterRoutine(self);
        }

        return orig(self);
    }

    private void HoldLuaAssets(On.Celeste.Mod.ModContent.orig_Add orig, ModContent self, string path, ModAsset asset) {
        if (!CrawlForLua)
            orig(self, path, asset);
        else {
            if (asset.Type == null)
                path = Everest.Content.GuessType(path, out asset.Type, out asset.Format);
            asset.PathVirtual = path;

            if (path.Replace('\\', '/').StartsWith("Loenn/"))
                LoennPluginLoader.RegisterLoennAsset(self.Name, asset, path);
        }
    }

    private System.Collections.IEnumerator CantEnterRoutine(LevelEnter self) {
        yield return 1f;
        Postcard postcard;
        self.Add(postcard = new Postcard(Dialog.Get("SNOWBERRY_PLAYTEST_MAP_POSTCARD"), "event:/ui/main/postcard_csides_in", "event:/ui/main/postcard_csides_out"));
        new DynamicData(self).Set("postcard", postcard);
        yield return postcard.DisplayRoutine();
        SaveData.Instance.CurrentSession_Safe = new Session(AreaKey.Default);
        SaveData.Instance.LastArea_Safe = AreaKey.Default;

        Monocle.Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaQuit);
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Celeste.Mod;
using MonoMod.Utils;
using NLua;
using Snowberry.Editor.Placements;

namespace Snowberry.Editor.LoennInterop;

public static class LoennPluginLoader {
    internal static string curMod = null;

    public static Dictionary<string, KeyValuePair<string, string>> Dialog = new();

    internal static void LoadPlugins() {
        Snowberry.LogInfo("Loading Selene for Loenn plugins");
        // note: we don't load in live mode because it breaks everything, instead we have to pass files through selene
        // but we do make it a global
        // also setup other globals needed by plugins
        Everest.LuaLoader.Context.DoString("""
            selene = require("Selene/selene/lib/selene/init")
            selene.load(nil, false)
            selene.parser = require("Selene/selene/lib/selene/parser")

            celesteRender = {}
            unpack = table.unpack

            math = math or {}
            math.atan2 = math.atan2 or require("#Snowberry.Editor.LoennInterop.LoennShims").atan2

            _MAP_VIEWER = {
                name = "snowberry"
            }
            """);

        Snowberry.LogInfo("Trying to load Loenn plugins");

        Dictionary<string, LuaTable> plugins = new();
        HashSet<string> triggers = [], effects = [];

        if(!Everest.Content.Mods.SelectMany(x => x.List).Any(asset => asset.PathVirtual.Replace('\\', '/').StartsWith("Loenn/")))
            ReCrawlForLua();

        foreach(IGrouping<ModContent, ModAsset> modAssets in Everest.Content.Mods
                    .SelectMany(mod => mod.List.Select(asset => (mod, asset)))
                    .Where(pair => pair.asset.PathVirtual.StartsWith("Loenn"))
                    .GroupBy(pair => pair.mod, pair => pair.asset)) {
            curMod = modAssets.Key.Name;
            foreach(var asset in modAssets) {
                var path = asset.PathVirtual.Replace('\\', '/');
                if(path.StartsWith("Loenn/entities/", StringComparison.Ordinal)
                   || path.StartsWith("Loenn/triggers/", StringComparison.Ordinal)
                   || path.StartsWith("Loenn/effects/", StringComparison.Ordinal)) {
                    try {
                        var pluginTables = RunAsset(asset, path);
                        bool any = false;
                        if (pluginTables != null)
                            foreach (var p in pluginTables) {
                                if (p is LuaTable pluginTable) {
                                    List<LuaTable> pluginsFromScript = [pluginTable];
                                    // returning multiple plugins at once
                                    if (pluginTable["name"] == null)
                                        pluginsFromScript = pluginTable.Values.OfType<LuaTable>().ToList();

                                    foreach (var table in pluginsFromScript) {
                                        if (table["name"] is string name) {
                                            plugins[name] = table;
                                            if (path.StartsWith("Loenn/triggers/"))
                                                triggers.Add(name);
                                            else if (path.StartsWith("Loenn/effects/"))
                                                effects.Add(name);

                                            Snowberry.LogInfo($"Loaded Loenn plugin for \"{name}\"");
                                            any = true;
                                        } else {
                                            Snowberry.Log(LogLevel.Warn, $"A nameless plugin was found at \"{path}\"");
                                        }
                                    }
                                }
                            }

                        if (!any) {
                            Snowberry.LogInfo($"No plugins were loaded from \"{curMod}: {path}\"");
                        }
                    } catch (Exception e) {
                        string ex = e.ToString();
                        if (ex.Contains("error in error handling")) {
                            Snowberry.Log(LogLevel.Error, $"Could not load Loenn plugin at \"{path}\" because of internal Lua errors. No more Lua entities will be loaded. Try restarting the game.");
                            break;
                        }

                        Snowberry.Log(LogLevel.Warn, $"Failed to load Loenn plugin at \"{path}\"");
                        Snowberry.Log(LogLevel.Warn, $"Reason: {ex}");
                    }
                } else if (path.StartsWith("Loenn/lang/") && path.EndsWith("/en_gb.lang")) {
                    string text;
                    using(var reader = new StreamReader(asset.Stream))
                        text = reader.ReadToEnd();

                    foreach(var entry in text.Split('\n').Select(k => k.Split('#')[0])) {
                        if(!string.IsNullOrWhiteSpace(entry)) {
                            var split = entry.Split('=');
                            if(split.Length == 2 && !string.IsNullOrWhiteSpace(split[0]) && !string.IsNullOrWhiteSpace(split[1])) {
                                Dialog[split[0]] = new KeyValuePair<string, string>(split[1].Trim(), asset.Source.Mod.Name);
                            }
                        }
                    }
                }
            }
        }

        curMod = null;

        Snowberry.LogInfo($"Found {plugins.Count} Loenn plugins");
        Snowberry.Log(LogLevel.Info, $"Loaded {Dialog.Count} dialog entries from language files for Loenn plugins.");

        foreach(var plugin in plugins){
            if(effects.Contains(plugin.Key)) {
                PluginInfo.Stylegrounds[plugin.Key] = new LoennStylegroundPluginInfo(plugin.Key, plugin.Value);
            }else{
                bool isTrigger = triggers.Contains(plugin.Key);
                PluginInfo.Entities[plugin.Key] = new LoennEntityPluginInfo(plugin.Key, plugin.Value, isTrigger);

                if (plugin.Value["placements"] is LuaTable placements)
                    if (placements.Keys.OfType<string>().Contains("name")) {
                        Dictionary<string, object> options = new();
                        if (placements["data"] is LuaTable data)
                            foreach (var item in data.Keys.OfType<string>())
                                options[item] = data[item];

                        string placementName = placements["name"] as string ?? "";
                        placementName = placementName.Replace(" ", ""); // thank you Flaglines and Such. very cool
                        placementName = Dialog.TryGetValue($"{(isTrigger ? "triggers" : "entities")}.{plugin.Key}.placements.name.{placementName}", out var name) ? $"{name.Key} [{name.Value}]" : $"{plugin.Key}.{placements["name"]}";
                        EntityPlacementProvider.Create(placementName, plugin.Key, options, isTrigger);
                    } else if (placements.Keys.Count >= 1) {
                        foreach (var i in placements.Keys) {
                            Dictionary<string, object> options = new();
                            // thank you Eevee Helper, very cool
                            if (placements[i] is LuaTable ptable && (i is "default" || ptable.Keys.OfType<string>().Contains("name"))) {
                                if (ptable["data"] is LuaTable data)
                                    foreach (var item in data.Keys.OfType<string>())
                                        options[item] = data[item];

                                string placementName = ptable["name"] as string ?? "default";
                                placementName = placementName.Replace(" ", ""); // lol
                                placementName = Dialog.TryGetValue($"{(isTrigger ? "triggers" : "entities")}.{plugin.Key}.placements.name.{placementName}", out var name) ? $"{name.Key} [{name.Value}]" : $"{plugin.Key}.{ptable["name"] ?? "default"}";
                                EntityPlacementProvider.Create(placementName, plugin.Key, options, isTrigger);
                            }
                        }
                    }
            }
        }
    }

    internal static object[] RunAsset(ModAsset asset, string path){
        string text;
        using(var reader = new StreamReader(asset.Stream))
            text = reader.ReadToEnd();

        // `require` searchers are broken, yaaaaaaay
        text = $"""
                    local snowberry_orig_require = require
                    local require = function(name)
                        return snowberry_orig_require("#Snowberry.Editor.LoennInterop.LoennShims").EverestRequire(name)
                    end
                    {text}
                    """;

        if (Everest.LuaLoader.Context.GetFunction("selene.parse")?.Call(text)?.FirstOrDefault() is string proc)
            return Everest.LuaLoader.Context.DoString(proc, asset.Source.Name + ":" + path);

        Snowberry.Log(LogLevel.Error, $"Failed to parse Selene syntax in {path}");
        return null;
    }

    private static void ReCrawlForLua() {
        HashSet<string> rootBlacklist = new DynamicData(typeof(Everest.Content)).Get<HashSet<string>>("BlacklistRootFolders");
        rootBlacklist.Remove("Loenn");
        rootBlacklist.Remove("Ahorn"); // may contain textures reused by plugins
        foreach (var mod in Everest.Content.Mods)
            DynamicData.For(mod).Invoke("Crawl");
    }
}
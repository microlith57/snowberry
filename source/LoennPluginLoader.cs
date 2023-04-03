using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Celeste.Mod;
using MonoMod.Utils;
using NLua;
using Snowberry.Editor;

namespace Snowberry;

public static class LoennPluginLoader {

    private static readonly List<ModAsset> assets = new();

    internal static void LoadPlugins() {
        Snowberry.LogInfo("Trying to load Loenn plugins");

        Dictionary<string, LuaTable> plugins = new();
        HashSet<string> triggers = new();

        CrawlForLua();

        foreach (var asset in assets.Where(k => k.PathVirtual.StartsWith("Loenn/entities/") || k.PathVirtual.StartsWith("Loenn/triggers/"))) {
            try {
                string text;
                using (var reader = new StreamReader(asset.Stream)) {
                    text = reader.ReadToEnd();
                }

                object[] pluginTables = Everest.LuaLoader.Context.DoString(text, asset.PathVirtual);
                foreach (var p in pluginTables) {
                    var pluginTable = p as LuaTable;
                    string name = (string)pluginTable["name"];
                    plugins[name] = pluginTable;
                    if (asset.PathVirtual.StartsWith("Loenn/triggers/"))
                        triggers.Add(name);
                    Snowberry.LogInfo($"Loaded Loenn plugin for \"{pluginTable["name"]}\"");
                }
            } catch (Exception e) {
                string ex = e.ToString();
                if (ex.Contains("error in error handling")) {
                    Snowberry.Log(LogLevel.Error, $"Could not load Loenn plugin at \"{asset.PathVirtual}\" because of internal Lua errors. No more Lua entities will be loaded. Try restarting the game.");
                    break;
                }

                Snowberry.Log(LogLevel.Warn, $"Failed to load Loenn plugin at \"{asset.PathVirtual}\"");
                Snowberry.Log(LogLevel.Verbose, $"Reason: {ex}");
            }
        }

        Snowberry.LogInfo($"Found {plugins.Count} Loenn plugins");

        foreach(var plugin in plugins) {
            bool isTrigger = triggers.Contains(plugin.Key);
            LoennPluginInfo info = new LoennPluginInfo(plugin.Key, plugin.Value, isTrigger);
            PluginInfo.Entities[plugin.Key] = info;

            if (plugin.Value["placements"] is LuaTable placements)
                if (placements.Keys.OfType<string>().Any(k => k.Equals("data"))) {
                    Dictionary<string, object> options = new();
                    if(placements["data"] is LuaTable data)
                        foreach (var item in data.Keys.OfType<string>())
                            options[item] = data[item];

                    string placementName = placements["name"] as string ?? "";
                    placementName = plugin.Key + " [Loenn]";//LoennText.TryGetValue($"{(isTrigger ? "triggers" : "entities")}.{plugin.Key}.placements.name.{placementName}", out var name) ? $"{name.Key} ({name.Value})" : "Loenn: " + plugin.Key;
                    Placements.Create(placementName, plugin.Key, options);
                } else if (placements.Keys.Count >= 1 && placements[1] is LuaTable) {
                    for (int i = 1; i < placements.Keys.Count + 1; i++) {
                        Dictionary<string, object> options = new();
                        if (placements[i] is LuaTable ptable && ptable["data"] is LuaTable data) {
                            foreach (var item in data.Keys.OfType<string>()) {
                                options[item] = data[item];
                            }

                            string placementName = ptable["name"] as string;
                            placementName = plugin.Key + " [Loenn]";//LoennText.TryGetValue($"entities.{plugin.Key}.placements.name.{placementName}", out var name) ? $"{name.Key} ({name.Value})" : $"Loenn: {plugin.Key} :: {ptable["name"]}";
                            Placements.Create(placementName, plugin.Key, options);
                        }
                    }
                }
        }
    }

    internal static void RegisterLoennAsset(ModAsset asset, string path) {
        assets.Add(asset);
    }

    internal static void CrawlForLua() {
        assets.Clear();
        try {
            Snowberry.CrawlForLua = true;

            foreach (var mod in Everest.Content.Mods)
                DynamicData.For(mod).Invoke("Crawl");
        } finally {
            Snowberry.CrawlForLua = false;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Celeste;
using Celeste.Mod;
using Monocle;
using MonoMod.Utils;
using NLua;
using NLua.Exceptions;
using Snowberry.Editor;

namespace Snowberry;

public static class LoennPluginLoader {

    private static readonly Dictionary<string, List<ModAsset>> assetsByMod = new();
    private static readonly Dictionary<string, LuaTable> reqCache = new();
    private static string curMod = null;

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
            """);

        Snowberry.LogInfo("Trying to load Loenn plugins");

        reqCache.Clear();

        Dictionary<string, LuaTable> plugins = new();
        HashSet<string> triggers = new();

        CrawlForLua();

        foreach(var modAssets in assetsByMod) {
            curMod = modAssets.Key;
            foreach(var asset in modAssets.Value) {
                var path = asset.PathVirtual.Replace('\\', '/');
                if(path.StartsWith("Loenn/entities/") || path.StartsWith("Loenn/triggers/")) {
                    try {
                        var pluginTables = RunAsset(asset, path);
                        bool any = false;
                        foreach (var p in pluginTables) {
                            if (p is LuaTable pluginTable) {
                                List<LuaTable> pluginsFromScript = new(){ pluginTable };
                                // returning multiple plugins at once
                                if (pluginTable["name"] == null)
                                    pluginsFromScript = pluginTable.Values.OfType<LuaTable>().ToList();

                                foreach (var table in pluginsFromScript) {
                                    if (table["name"] is string name) {
                                        plugins[name] = table;
                                        if (path.StartsWith("Loenn/triggers/"))
                                            triggers.Add(name);
                                        Snowberry.LogInfo($"Loaded Loenn plugin for \"{name}\"");
                                        any = true;
                                    } else {
                                        Snowberry.Log(LogLevel.Warn, $"A nameless entity was found at \"{path}\"");
                                    }
                                }
                            }
                        }

                        if (!any) {
                            Snowberry.LogInfo($"No plugins were loaded from \"{path}\"");
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
                }
            }
        }

        curMod = null;

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

    private static object[] RunAsset(ModAsset asset, string path){
        string text;
        using(var reader = new StreamReader(asset.Stream)){
            text = reader.ReadToEnd();
        }

        // `require` searchers are broken, yaaaaaaay
        text = $"""
                    local snowberry_orig_require = require
                    local require = function(name)
                        return snowberry_orig_require("#Snowberry.LoennPluginLoader").EverestRequire(name)
                    end
                    {text}
                    """;

        if (Everest.LuaLoader.Context.GetFunction("selene.parse")?.Call(text)?.FirstOrDefault() is string proc) {
            return Everest.LuaLoader.Context.DoString(proc, path);
        }

        Snowberry.Log(LogLevel.Error, $"Failed to parse Selene syntax in {path}");
        return null;
    }

    // invoked via lua
    public static object EverestRequire(string name) {
        // name could be "mods", "structs.rectangle", etc

        // if you want something, check LoennHelpers/
        try {
            var h = Everest.LuaLoader.Context.DoString($"return require(\"LoennHelpers/{name.Replace(".", "/")}\")").FirstOrDefault();
            if (h != null)
                return h;
        } catch (LuaScriptException e) {
            if(!e.ToString().Contains("not found:")) {
                Snowberry.Log(LogLevel.Verbose, $"Failed to load at {name}");
                Snowberry.Log(LogLevel.Verbose, $"Reason: {e}");
            }
        }

        return "\n\tCould not find Loenn library: " + name;
    }

    internal static void RegisterLoennAsset(string mod, ModAsset asset, string path) {
        if (assetsByMod.TryGetValue(mod, out var value))
            value.Add(asset);
        else
            (assetsByMod[mod] = new List<ModAsset>()).Add(asset);
    }

    internal static void CrawlForLua() {
        assetsByMod.Clear();
        try {
            Snowberry.CrawlForLua = true;

            foreach (var mod in Everest.Content.Mods)
                DynamicData.For(mod).Invoke("Crawl");
        } finally {
            Snowberry.CrawlForLua = false;
        }
    }

    private static LuaTable EmptyTable() {
        return Everest.LuaLoader.Context.DoString("return {}").FirstOrDefault() as LuaTable;
    }

    // invoked via lua
    public static object LuaGetImage(string textureName, string atlasName) {
        atlasName ??= "Gameplay";
        Atlas atlas = atlasName.ToLowerInvariant().Equals("gui") ? GFX.Gui : atlasName.ToLowerInvariant().Equals("misc") ? GFX.Misc : GFX.Game;

        if (!atlas.Has(textureName))
            return null;

        var meta = EmptyTable();

        // We render these so we can pick whatever format we like
        meta["image"] = textureName;
        meta["atlas"] = atlasName;

        MTexture texture = atlas[textureName];
        meta["width"] = meta["realWidth"] = texture.Width;
        meta["height"] = meta["realHeight"] = texture.Height;
        meta["offsetX"] = meta["offsetY"] = 0;

        return meta;
    }

    private static long toLong(object o) {
        return o switch {
            long l => l,
            int i => i,
            float f => (long)f,
            double d => (long)d,
            short s => s,
            byte b => b,
            _ => 0
        };
    }

    // invoked via lua
    public static object lshift(object o, object by) {
        return toLong(o) << (int)toLong(by);
    }

    // invoked via lua
    public static LuaTable RequireFromMods(string filename, string modName) {
        string curModName = string.IsNullOrEmpty(modName) ? curMod : modName;
        if (curModName == null || filename == null)
            return null;

        string target = $"Loenn/{filename.Replace('.', '/')}";

        try {
            if (reqCache.TryGetValue(target, out var library))
                return library;

            if (assetsByMod.TryGetValue(curModName, out var libFiles))
                foreach (var asset in libFiles.Where(asset => asset.PathVirtual.Replace('\\', '/') == target))
                    return reqCache[target] = RunAsset(asset, target)?.FirstOrDefault() as LuaTable;

            return reqCache[target] = null;
        } catch (Exception e) {
            Snowberry.Log(LogLevel.Error, $"Error running Loenn library {modName}/{filename}: {e}");
            return reqCache[target] = null;
        }
    }
}
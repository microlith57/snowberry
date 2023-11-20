using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod;
using JetBrains.Annotations;
using Monocle;
using NLua;
using NLua.Exceptions;

namespace Snowberry.Editor.LoennInterop;

public static class LoennShims {

    // keep track of `require`'d files
    internal static readonly Dictionary<string, LuaTable> reqCache = new();

    // implementation utilities
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
    private static double toDouble(object o) {
        return o switch {
            long l => l,
            int i => i,
            float f => f,
            double d => d,
            short s => s,
            byte b => b,
            _ => 0
        };
    }
    private static LuaTable EmptyTable() =>
        Everest.LuaLoader.Context.DoString("return {}").FirstOrDefault() as LuaTable;

    [UsedImplicitly] // invoked via lua
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

    [UsedImplicitly] // invoked via lua
    public static object LuaGetImage(string textureName, string atlasName) {
        atlasName ??= "Gameplay";
        Atlas atlas = atlasName.ToLowerInvariant().Equals("gui") ? GFX.Gui : atlasName.ToLowerInvariant().Equals("misc") ? GFX.Misc : GFX.Game;

        if (textureName.StartsWith("@Internal@/"))
            textureName = "plugins/Snowberry/" + textureName["@Internal@/".Length..];

        // we *have to* return nil/null for certain entities (e.g. MHH Flag Decals) to conditionally choose textures
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

    [UsedImplicitly] // invoked via lua
    public static object lshift(object o, object by) {
        return toLong(o) << (int)toLong(by);
    }

    [UsedImplicitly] // invoked via lua
    public static double atan2(object l, object r) {
        return Math.Atan2(toDouble(l), toDouble(r));
    }

    [UsedImplicitly] // invoked via lua
    public static LuaTable RequireFromMods(string filename, string modName) {
        string curModName = string.IsNullOrEmpty(modName) ? LoennPluginLoader.curMod : modName;
        if (curModName == null || filename == null)
            return null;

        string targetFile = $"Loenn/{filename.Replace('.', '/')}";
        string targetId = $"{curModName}::{targetFile}";

        try {
            if (reqCache.TryGetValue(targetId, out var library))
                return library;

            foreach (var asset in Everest.Content.Mods
                         .Where(mod => mod.Name == curModName)
                         .SelectMany(mod => mod.List)
                         .Where(asset => asset.Type == typeof(AssetTypeLua))
                         .Where(asset => asset.PathVirtual.Replace('\\', '/') == targetFile))
                return reqCache[targetId] = LoennPluginLoader.RunAsset(asset, targetFile)?.FirstOrDefault() as LuaTable;

            return reqCache[targetId] = null;
        } catch (Exception e) {
            Snowberry.Log(LogLevel.Error, $"Error running Loenn library {modName}/{filename}: {e}");
            return reqCache[targetId] = null;
        }
    }

    [UsedImplicitly] // invoked via lua
    public static LuaTable FindLoadedMod(string modName) {
        foreach (var module in Everest.Modules) {
            if (module.Metadata.Name == modName) {
                var ret = EmptyTable();
                ret["Name"] = modName;
                ret["Version"] = module.Metadata.VersionString;

                return ret;
            }
        }

        return null;
    }

    [UsedImplicitly] // invoked via lua
    public static VirtualMap<MTexture> Autotile(string layer, object key, float width, float height) {
        bool fg = layer.Equals("tilesFg", StringComparison.InvariantCultureIgnoreCase);
        char keyC = key.ToString()[0];
        return (fg ? GFX.FGAutotiler : GFX.BGAutotiler).GenerateBoxStable(keyC, (int)(width / 8f), (int)(height / 8f)).TileGrid.Tiles;
    }
}
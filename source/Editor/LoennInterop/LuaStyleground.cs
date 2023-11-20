using System.Collections.Generic;
using NLua;

namespace Snowberry.Editor.LoennInterop;

public class LoennStyleground : Styleground {

    // conceptually much simpler than `LuaEntity`, because modded stylegrounds do not (yet) have any behaviour
    private readonly LuaTable plugin;

    public readonly Dictionary<string, object> Attrs = new();

    public LoennStyleground(string name, LoennStylegroundPluginInfo info, LuaTable plugin) {
        Name = name;
        Info = info;
        this.plugin = plugin;
    }
}
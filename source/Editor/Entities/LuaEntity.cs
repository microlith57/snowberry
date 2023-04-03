using System.Collections.Generic;
using NLua;

namespace Snowberry.Editor.Entities;

public class LuaEntity : Entity {

    private LuaTable Plugin;

    public Dictionary<string, object> Values = new();

    public LuaEntity(string name, LoennPluginInfo info, LuaTable plugin, bool isTrigger) {
        Name = name;
        Info = info;
        Plugin = plugin;
        IsTrigger = isTrigger;
    }

    public override bool IsTrigger { get; }
}
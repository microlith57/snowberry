using NLua;

namespace Snowberry.Editor.LoennInterop;

public class LoennStylegroundPluginInfo : PluginInfo {

    protected readonly LuaTable Plugin;

    public LoennStylegroundPluginInfo(string name, LuaTable plugin) : base(name, typeof(LoennStyleground), null, CelesteEverest.INSTANCE) {
        this.Plugin = plugin;
    }

    public override T Instantiate<T>() {
        if(typeof(T).IsAssignableFrom(typeof(LoennStyleground)))
            return new LoennStyleground(name, this, Plugin) as T;
        return null;
    }
}
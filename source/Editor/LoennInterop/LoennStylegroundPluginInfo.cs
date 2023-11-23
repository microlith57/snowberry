using System.Collections.Generic;
using System.Linq;
using NLua;

namespace Snowberry.Editor.LoennInterop;

public class LoennStylegroundPluginInfo : PluginInfo, DefaultedPluginInfo {

    protected readonly LuaTable Plugin;

    public Dictionary<string, object> Defaults = new();

    public LoennStylegroundPluginInfo(string name, LuaTable plugin) : base(name, typeof(LoennStyleground), null, CelesteEverest.INSTANCE) {
        this.Plugin = plugin;

        if (plugin["defaultData"] is LuaTable data) {
            foreach (var item in data.Keys.OfType<string>())
                if (!Styleground.IllegalOptionNames.Contains(item)) {
                    Options[item] = new LoennEntityOption(item, data[item].GetType(), Tooltip(item, name));
                    Defaults.TryAdd(item, data[item]);
                }
        }
    }

    public override T Instantiate<T>() {
        if(typeof(T).IsAssignableFrom(typeof(LoennStyleground)))
            return new LoennStyleground(name, this, Plugin) as T;
        return null;
    }

    public bool TryGetDefault(string key, out object value) => Defaults.TryGetValue(key, out value);

    private static string Tooltip(string key, string effectName) =>
        LoennPluginLoader.Dialog.TryGetValue($"style.effects.{effectName}.description.{key}", out var k) ? k.Key : null;

    public string Title() =>
        LoennPluginLoader.Dialog.TryGetValue($"style.effects.{name}.name", out var k) ? k.Key + $" [{k.Value}]" : null;
}
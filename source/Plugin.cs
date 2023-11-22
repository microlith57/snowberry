using Celeste.Mod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Snowberry.Editor;
using Snowberry.UI;

namespace Snowberry;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
[MeansImplicitUse]
public class PluginAttribute(string Name) : Attribute {
    internal readonly string Name = Name;
}

[AttributeUsage(AttributeTargets.Field)]
[MeansImplicitUse]
public class OptionAttribute(string optionName) : Attribute {
    internal readonly string Name = optionName;
}

public abstract class Plugin {
    public PluginInfo Info { get; internal set; }
    public string Name { get; internal set; }

    // overriden by generic plugins
    public virtual void Set(string option, object value) {
        if (Info.Options.TryGetValue(option, out PluginOption f)) {
            try {
                // TODO: this is stupid
                //  - really StrToObject should (and does!) handle all of thesec
                object v;
                if (f.FieldType == typeof(char) && value is not char)
                    v = value.ToString()[0];
                else if (f.FieldType == typeof(Color) && value is not Color)
                    v = Monocle.Calc.HexToColor(value.ToString());
                else if (f.FieldType == typeof(Tileset) && value is not Tileset)
                    v = Tileset.ByKey(value.ToString()[0], false);
                else
                    v = value is string str ? StrToObject(f.FieldType, str) : Convert.ChangeType(value, f.FieldType);
                f.SetValue(this, v);
            } catch (ArgumentException e) {
                Snowberry.Log(LogLevel.Warn,
                    $"Tried to set field {option} to an invalid value {value} ({value?.GetType().FullName ?? "null"})");
                Snowberry.Log(LogLevel.Warn, e.ToString());
            }
        }
    }

    public virtual object Get(string option) =>
        Info.Options.TryGetValue(option, out PluginOption f) ? ObjectToStr(f.GetValue(this)) : null;

    public string GetTooltipFor(string option) =>
        Info.Options.TryGetValue(option, out PluginOption f) ? f.Tooltip : null;

    public virtual (UIElement, int height)? CreateOptionUi(string optionName) => null;

    public static object StrToObject(Type targetType, string raw){
        if(targetType.IsEnum)
            try {
                return Enum.Parse(targetType, raw);
            } catch {
                return null;
            }

        if(targetType == typeof(Color))
            return Monocle.Calc.HexToColor(raw);
        if(targetType == typeof(char))
            return raw[0];
        if(targetType == typeof(Tileset))
            return Tileset.ByKey(raw[0], false);
        if(targetType == typeof(bool))
            return raw.Equals("true", StringComparison.InvariantCultureIgnoreCase);

        try {
            return Convert.ChangeType(raw, targetType);
        } catch (Exception e) {
            Snowberry.Log(LogLevel.Error,
                $"""
                 Attempted invalid conversion of string "{raw}" into type "{targetType.FullName}"!
                 {e}
                 """);
            return Util.Default(targetType);
        }
    }

    public static object ObjectToStr(object obj) => obj switch {
        Color color => color.IntoRgbString(),
        Enum => obj.ToString(),
        char ch => ch.ToString(),
        Tileset ts => ts.Key.ToString(),
        _ => obj
    };
}

public interface DictBackedPlugin {
    public Dictionary<string, object> Attrs { get; }
}
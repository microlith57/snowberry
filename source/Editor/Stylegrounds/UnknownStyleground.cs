using System.Collections.Generic;

namespace Snowberry.Editor.Stylegrounds;

// Any styleground that doesn't have it's own plugin.
public class UnknownStyleground : Styleground, DictBackedPlugin {

    public Dictionary<string, object> Attrs { get; } = new();

    public override void Set(string option, object value) =>
        Attrs[option] = value;

    public override object Get(string option) =>
        Attrs[option];
}
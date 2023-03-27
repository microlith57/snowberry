using System.Collections.Generic;

namespace Snowberry.Editor.Entities;

// Any entity that doesn't have its own plugin.
public class UnknownEntity : Entity{
    public readonly Dictionary<string, object> Attrs = new();
    public bool LoadedFromTrigger = false;

    public override bool IsTrigger => LoadedFromTrigger;

    public override void Set(string option, object value) {
        Attrs[option] = value;
    }

    public override object Get(string option) {
        return Attrs[option];
    }
}
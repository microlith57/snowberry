using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;
using Snowberry.Editor.Stylegrounds;
using System.Text.RegularExpressions;
using Snowberry.Editor.Recording;
using Snowberry.Editor.Tools;
using static Celeste.BinaryPacker;

namespace Snowberry.Editor;

public class Styleground : Plugin {
    public Map Map;

    public string Tags = "";

    public Vector2 Position;

    public Vector2 Scroll = Vector2.One;

    public Vector2 Speed;

    public float WindMultiplier;

    public Color RawColor = Color.White;

    public float Alpha = 1;

    public bool LoopX = true;

    public bool LoopY = true;

    public bool? DreamingOnly;

    public bool FlipX;

    public bool FlipY;

    public string FadeX;

    public string FadeY;

    public string OnlyIn = "*";

    public string ExcludeFrom = "";

    public string Flag = "";

    public string NotFlag = "";

    public string ForceFlag = "";

    public bool InstantIn = true;

    public bool InstantOut;

    public virtual bool Additive => false;

    public Color Color => RawColor * Alpha;

    public static readonly List<string> IllegalOptionNames = [
        "name", "tag", "x", "y", "only", "exclude", "flag", "notflag", "always",
        "scrollx", "scrolly", "speedx", "speedy", "color", "alpha", "flipx", "flipy", "loopx", "loopy", "fadex", "fadey",
        "wind", "dreaming", "instantIn", "instantOut"
    ];

    // Render on the Snowberry background
    public virtual void Render(Room room) { }

    public virtual string Title() => Name;

    public bool IsVisible(Room room) {
        if (!string.IsNullOrWhiteSpace(Flag)) {
            if (Editor.Instance?.CurrentTool is PlaytestTool pt && RecInProgress.Get<FlagsRecorder>() is { /* non-null */ } flags) {
                if (flags.GetFlagAt(Flag, pt.Now) is not true)
                    return false;
            } else
                return false;
        }

        string roomName = room?.Name ?? "";

        if (ExcludeFrom != null && MatchRoomName(ExcludeFrom, roomName)) {
            return false;
        }

        if (OnlyIn != null && !MatchRoomName(OnlyIn, roomName)) {
            return false;
        }

        return true;
    }

    public static bool MatchRoomName(string predicate, string name) =>
        name != null && predicate
            .Split(',')
            .Any(text => text.Equals(name) ||
                (text.Contains('*') && Regex.IsMatch(name, "^" + Regex.Escape(text).Replace("\\*", ".*") + "$")));

    public static Styleground Create(string name, Map map, Element data, Element applyData = null) {
        // try both lowercased and exact
        bool exists = PluginInfo.Stylegrounds.TryGetValue(name.ToLowerInvariant(), out PluginInfo plugin)
                        || PluginInfo.Stylegrounds.TryGetValue(name, out plugin);
        Styleground styleground;
        if (exists) {
            styleground = plugin.Instantiate<Styleground>();
        } else {
            Map.MissingObjectReports[name] = Map.MissingObjectReports.TryGetValue(name, out var u) ? u + 1 : 1;
            // remove builtin or illegal option names before passing them along
            Dictionary<string, object> sanitized = new(data.Attributes);
            foreach(string badName in IllegalOptionNames)
                sanitized.Remove(badName);
            styleground = new UnknownStyleground {
                Name = name,
                Info = new UnknownPluginInfo(name)
            };
        }

        styleground.Map = map;

        if (data?.Attributes != null) {
            applyData ??= new Element();
            applyData.Attributes ??= new();

            foreach (var (key, value) in applyData.Attributes)
                if (!IllegalOptionNames.Contains(key))
                    styleground.Set(key, value);

            foreach (var (key, value) in data.Attributes)
                if (!IllegalOptionNames.Contains(key))
                    styleground.Set(key, value);

            // currently this is just the same thing map data does
            // its terrible

            if (data.HasAttr("tag"))
                styleground.Tags = data.Attr("tag");

            if (applyData.HasAttr("tag"))
                styleground.Tags = applyData.Attr("tag");

            if (data.HasAttr("x"))
                styleground.Position.X = data.AttrFloat("x");
            else if (applyData.HasAttr("x"))
                styleground.Position.X = applyData.AttrFloat("x");

            if (data.HasAttr("y"))
                styleground.Position.Y = data.AttrFloat("y");
            else if (applyData.HasAttr("y"))
                styleground.Position.Y = applyData.AttrFloat("y");

            if (data.HasAttr("scrollx"))
                styleground.Scroll.X = data.AttrFloat("scrollx");
            else if (applyData.HasAttr("scrollx"))
                styleground.Scroll.X = applyData.AttrFloat("scrollx");

            if (data.HasAttr("scrolly"))
                styleground.Scroll.Y = data.AttrFloat("scrolly");
            else if (applyData.HasAttr("scrolly"))
                styleground.Scroll.Y = applyData.AttrFloat("scrolly");

            if (data.HasAttr("speedx"))
                styleground.Speed.X = data.AttrFloat("speedx");
            else if (applyData.HasAttr("speedx"))
                styleground.Speed.X = applyData.AttrFloat("speedx");

            if (data.HasAttr("speedy"))
                styleground.Speed.Y = data.AttrFloat("speedy");
            else if (applyData.HasAttr("speedy"))
                styleground.Speed.Y = applyData.AttrFloat("speedy");

            styleground.RawColor = Color.White;
            if (data.HasAttr("color"))
                styleground.RawColor = Calc.HexToColor(data.Attr("color"));
            else if (applyData.HasAttr("color"))
                styleground.RawColor = Calc.HexToColor(applyData.Attr("color"));

            if (data.HasAttr("alpha"))
                styleground.Alpha = data.AttrFloat("alpha");
            else if (applyData.HasAttr("alpha"))
                styleground.Alpha = applyData.AttrFloat("alpha");

            if (data.HasAttr("flipx"))
                styleground.FlipX = data.AttrBool("flipx");
            else if (applyData.HasAttr("flipx"))
                styleground.FlipX = applyData.AttrBool("flipx");

            if (data.HasAttr("flipy"))
                styleground.FlipY = data.AttrBool("flipy");
            else if (applyData.HasAttr("flipy"))
                styleground.FlipY = applyData.AttrBool("flipy");

            if (data.HasAttr("loopx"))
                styleground.LoopX = data.AttrBool("loopx");
            else if (applyData.HasAttr("loopx"))
                styleground.LoopX = applyData.AttrBool("loopx");

            if (data.HasAttr("loopy"))
                styleground.LoopY = data.AttrBool("loopy");
            else if (applyData.HasAttr("loopy"))
                styleground.LoopY = applyData.AttrBool("loopy");

            if (data.HasAttr("fadex"))
                styleground.FadeX = data.Attr("fadex");
            else if (applyData.HasAttr("fadex"))
                styleground.FadeX = applyData.Attr("fadex");

            if (data.HasAttr("fadey"))
                styleground.FadeY = data.Attr("fadey");
            else if (applyData.HasAttr("fadey"))
                styleground.FadeY = applyData.Attr("fadey");

            if (data.HasAttr("wind"))
                styleground.WindMultiplier = data.AttrFloat("wind");
            else if (applyData.HasAttr("wind"))
                styleground.WindMultiplier = applyData.AttrFloat("wind");

            string exclude = null;
            if (data.HasAttr("exclude"))
                exclude = data.Attr("exclude");
            else if (applyData.HasAttr("exclude"))
                exclude = applyData.Attr("exclude");

            if (exclude != null)
                styleground.ExcludeFrom = exclude;

            string only = null;
            if (data.HasAttr("only"))
                only = data.Attr("only");
            else if (applyData.HasAttr("only"))
                only = applyData.Attr("only");

            if (only != null)
                styleground.OnlyIn = only;

            string flag = null;
            if (data.HasAttr("flag"))
                flag = data.Attr("flag");
            else if (applyData.HasAttr("flag"))
                flag = applyData.Attr("flag");

            if (flag != null)
                styleground.Flag = flag;

            string notFlag = null;
            if (data.HasAttr("notflag"))
                notFlag = data.Attr("notflag");
            else if (applyData.HasAttr("notflag"))
                notFlag = applyData.Attr("notflag");

            if (notFlag != null)
                styleground.NotFlag = notFlag;

            string alwaysFlag = null;
            if (data.HasAttr("always"))
                alwaysFlag = data.Attr("always");
            else if (applyData.HasAttr("always"))
                alwaysFlag = applyData.Attr("always");

            if (alwaysFlag != null)
                styleground.ForceFlag = alwaysFlag;

            bool? dreaming = null;
            if (data.HasAttr("dreaming"))
                dreaming = data.AttrBool("dreaming");
            else if (applyData.HasAttr("dreaming"))
                dreaming = applyData.AttrBool("dreaming");

            if (dreaming.HasValue)
                styleground.DreamingOnly = dreaming;

            if (data.HasAttr("instantIn"))
                styleground.InstantIn = data.AttrBool("instantIn");
            else if (applyData.HasAttr("instantIn"))
                styleground.InstantIn = applyData.AttrBool("instantIn");

            if (data.HasAttr("instantOut"))
                styleground.InstantOut = data.AttrBool("instantOut");
            else if (applyData.HasAttr("instantOut"))
                styleground.InstantOut = applyData.AttrBool("instantOut");
        }

        return styleground;
    }
}
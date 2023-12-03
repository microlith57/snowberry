using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Snowberry.Editor;

public partial class Decal : Placeable {
    public Room Room { get; set; }
    public Vector2 Position { get; set; }

    private readonly MTexture tex;

    public Vector2 Scale = new(1);
    public float Rotation = 0;
    public Color Color = Color.White;
    public bool Fg = false;

    public string Texture { get; private set; }

    public Rectangle Bounds => new((int)(Position.X - Math.Abs(tex.Width * Scale.X) / 2), (int)(Position.Y - Math.Abs(tex.Height * Scale.Y) / 2), (int)Math.Abs(tex.Width * Scale.X), (int)Math.Abs(tex.Height * Scale.Y));

    internal Decal(Room room, string texture, bool fg) {
        Room = room;
        Texture = texture;
        Fg = fg;
        tex = LookupTex(texture);
    }

    internal Decal(Room room, DecalData data, bool fg) {
        Room = room;
        Fg = fg;

        Texture = data.Texture;
        tex = LookupTex(Texture);
        Position = data.Position + Room.Position * 8;
        Scale = data.Scale;
        Rotation = data.Rotation;
        Color = Calc.HexToColorWithAlpha(data.ColorHex);
    }

    public void Render() {
        tex.DrawCentered(Position, Color, Scale, Rotation);
    }

    public void AddToRoom(Room room) {
        Room = room;
        UndoRedo.BeginAction("add decal", Room.SnapshotDecalInclusion(this));
        (Fg ? room.FgDecals : room.BgDecals).Add(this);
        UndoRedo.CompleteAction();
    }

    private static MTexture LookupTex(string tex) =>
        // grab first variant of decal
        GFX.Game.GetAtlasSubtextures(Sanitize(tex, false))[0];

    public static string Sanitize(string tex, bool hasPfix){
        // see Celeste.Decal.orig_ctor
        // remove any extention like .png
        var ext = Path.GetExtension(tex);
        var plainPath = ext.Length > 0 ? tex[..^ext.Length] : tex;
        // put it in decals/ if necessary
        var pfixPath = hasPfix ? plainPath : "decals/" + plainPath;
        // fix any backslashes
        var ctxPath = pfixPath.Replace("\\", "/");
        // remove any numeric suffix
        return StripDigits().Replace(ctxPath, "");
    }

    public UndoRedo.Snapshotter SnapshotPosition() => new PositionSnapshotter(this);

    private record PositionSnapshotter(Decal d) : UndoRedo.Snapshotter<Vector2> {
        public Vector2 Snapshot() => d.Position;
        public void Apply(Vector2 t) => d.Position = t;
    }

    [GeneratedRegex("\\d+$", RegexOptions.Compiled)]
    private static partial Regex StripDigits();
}
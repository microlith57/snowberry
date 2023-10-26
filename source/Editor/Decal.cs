using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Snowberry.Editor;

public class Decal : Placeable {

    private static readonly Regex StripDigits = new Regex("\\d+$", RegexOptions.Compiled);

    public Room Room { get; set; }
    public Vector2 Position { get; set; }

    private readonly MTexture tex;

    public Vector2 Scale = new(1);
    public float Rotation = 0;
    public Color Color = Color.White;
    public bool Fg = false;

    public string Texture { get; private set; }

    public Rectangle Bounds => new((int)(Position.X - Math.Abs(tex.Width * Scale.X) / 2), (int)(Position.Y - Math.Abs(tex.Height * Scale.Y) / 2), (int)Math.Abs(tex.Width * Scale.X), (int)Math.Abs(tex.Height * Scale.Y));

    internal Decal(Room room, string texture) {
        Room = room;
        this.Texture = texture;
        this.tex = LookupTex(texture);
    }

    internal Decal(Room room, DecalData data) {
        Room = room;

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
        (Fg ? room.FgDecals : room.BgDecals).Add(this);
    }

    private static MTexture LookupTex(string tex) =>
        // grab first variant of decal
        GFX.Game.GetAtlasSubtextures(Sanitize(tex, false))[0];

    public static string Sanitize(string tex, bool hasPfix){
        // see Celeste.Decal.orig_ctor
        // remove any extention like .png
        string ext = Path.GetExtension(tex);
        string plainPath = (ext.Length > 0 ? tex.Substring(0, tex.Length - ext.Length) : tex);
        // put it in decals/ if necessary
        string pfixPath = hasPfix ? plainPath : "decals/" + plainPath;
        // fix any backslashes
        string ctxPath = pfixPath.Replace("\\", "/");
        // remove any numeric suffix
        return StripDigits.Replace(ctxPath, "");
    }

    public UndoRedo.Snapshotter<Vector2> SPosition() => new(() => Position, p => Position = p, this);
}
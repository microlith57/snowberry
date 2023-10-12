using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Snowberry.Editor.Entities;

[Plugin("gondola")]
public class Plugin_Gondola : Entity {
    [Option("active")] public bool Active = true;

    private Image top = new Image(GFX.Game["objects/gondola/top"]);
    private Image front = new Image(GFX.Game["objects/gondola/front"]);
    private Image back = new Image(GFX.Game["objects/gondola/back"]);
    private Image lever = new Image(GFX.Game["objects/gondola/lever00"]);
    private Image anchorLeft = new Image(GFX.Game["objects/gondola/cliffsideLeft"]);
    private Image anchorRight = new Image(GFX.Game["objects/gondola/cliffsideRight"]);

    public override int MinNodes => 1;
    public override int MaxNodes => 1;

    public Vector2 Start => Position;
    public Vector2 Destination => Nodes.Count > 0 ? Nodes[0] : Position;
    public Vector2 GondolaPosition => Active ? Start : Position;

    public Plugin_Gondola() {
        top.Origin = new Vector2(top.Width / 2f, 12f);
        front.Origin = new Vector2(front.Width / 2f, 12f);
        back.Origin = new Vector2(back.Width / 2f, 12f);
        lever.Origin = new Vector2(lever.Width / 2f, 12f);
        anchorLeft.JustifyOrigin(0f, 1f);
        anchorRight.JustifyOrigin(0, 0.5f);
        anchorRight.Scale.X = -1f;
    }

    // A mix of hardcoding and relying on the images so no-one is happy
    protected override IEnumerable<Rectangle> Select() =>
    new Rectangle[] {
        RectOnAbsolute(new Vector2(front.Width - 6f, front.Height - 23f),
            front.Position - front.Origin + new Vector2(3f, 23f)),
        RectOnAbsolute(new Vector2(anchorRight.Width, anchorRight.Height - 22f),
            anchorRight.Position - anchorRight.Origin - new Vector2(anchorRight.Width, -8f))
    };

    public override void Render() {
        base.Render();

        // Update lever visibility
        lever.Visible = Active;

        // Update positions
        top.Position = GondolaPosition - new Vector2(0, 52f);
        front.Position = top.Position;
        back.Position = top.Position;
        lever.Position = top.Position;
        anchorLeft.Position = Start + new Vector2(-124f, 0);
        anchorRight.Position = Destination + new Vector2(144f, -104f);

        // Update rotation
        top.Rotation = Calc.Angle(Start, Destination);

        back.Render();
        front.Render();
        lever.Render();

        // Celeste uses (Destination - Start).SafeNormalise() instead of Vector2.UnitX.Rotate(top.Rotation)
        Draw.Line(anchorLeft.Position + new Vector2(40f, -12f),
            top.Position - 6f * Vector2.UnitX.Rotate(top.Rotation), Color.Black, 2);
        Draw.Line(top.Position + 6f * Vector2.UnitX.Rotate(top.Rotation),
            anchorRight.Position + new Vector2(-40f, -3f), Color.Black, 2);

        top.Render();
        anchorLeft.Render();
        anchorRight.Render();
    }

    public static void AddPlacements() {
        Placements.Create("Gondola", "gondola");
    }
}
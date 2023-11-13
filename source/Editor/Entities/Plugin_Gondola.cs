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

    private MTexture top = GFX.Game["objects/gondola/top"];
    private MTexture front = GFX.Game["objects/gondola/front"];
    private MTexture back = GFX.Game["objects/gondola/back"];
    private MTexture lever = GFX.Game["objects/gondola/lever00"];
    private MTexture anchorLeft = GFX.Game["objects/gondola/cliffsideLeft"];
    private MTexture anchorRight = GFX.Game["objects/gondola/cliffsideRight"];

    public override int MinNodes => 1;
    public override int MaxNodes => 1;

    public Vector2 Start => Position;
    public Vector2 Destination => Nodes.Count > 0 ? Nodes[0] : Position;
    public Vector2 GondolaPosition => (Active ? Start : Position) - new Vector2(0, 52f);

    // A mix of hardcoding and relying on the textures so no-one is happy
    protected override IEnumerable<Rectangle> Select() =>
    new Rectangle[] {
        RectOnAbsolute(new Vector2(front.Width - 6f, front.Height - 23f),
            GondolaPosition + new Vector2(3f - top.Width / 2f, 11f)),
        RectOnAbsolute(new Vector2(anchorRight.Width, anchorRight.Height - 22f),
            Destination - new Vector2(-144f + anchorRight.Width, 104f - 8f + anchorRight.Height / 2f))
    };

    public override void Render() {
        base.Render();

        back.Draw(GondolaPosition, new Vector2(back.Width / 2f, 12f));
        front.Draw(GondolaPosition, new Vector2(front.Width / 2f, 12f));
        if(Active) lever.Draw(GondolaPosition, new Vector2(lever.Width / 2f, 12f));


        float topRotation = Calc.Angle(Start, Destination);

        // Celeste uses (Destination - Start).SafeNormalise() instead of Vector2.UnitX.Rotate(top.Rotation)
        Draw.Line(Start + new Vector2(-84f, -12f),
            GondolaPosition - 6f * Vector2.UnitX.Rotate(topRotation), Color.Black, 2);
        Draw.Line(GondolaPosition + 6f * Vector2.UnitX.Rotate(topRotation),
            Destination + new Vector2(104f, -101f), Color.Black, 2);


        top.Draw(GondolaPosition, new Vector2(top.Width / 2f, 12f), Color.White, 1, topRotation);

        anchorLeft.DrawJustified(Start + new Vector2(-124f, 0), new Vector2(0, 1f));
        anchorRight.DrawJustified(Destination + new Vector2(144f, -104f), new Vector2(0, 0.5f), Color.White, new Vector2(-1f, 1f));
    }

    public static void AddPlacements() {
        Placements.EntityPlacementProvider.Create("Gondola", "gondola");
    }
}
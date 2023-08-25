using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities;

[Plugin("blackGem")]
public class Plugin_CrystalHeart : Entity {

    [Option("fake")] public bool Fake = false;
    [Option("removeCameraTriggers")] public bool RemoveCameraTriggers = false;
    [Option("fakeHeartDialog")] public string FakeHeartDialog = "CH9_FAKE_HEART";
    [Option("keepGoingDialog")] public string KeepGoingDialog = "CH9_KEEP_GOING";

    public override void Render() {
        base.Render();

        FromSprite("heartgem0", "idle")?.DrawCentered(Position, Color.White, new Vector2(1, 1))
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(16, 16), justify: new(0.5f, 0.5f));
    }

    public static void AddPlacements() {
        Placements.Create("Crystal Heart", "blackGem");
    }
}
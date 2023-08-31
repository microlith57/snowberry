using System.Collections.Generic;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Snowberry.Editor.Entities;

[Plugin("fakeHeart")]
public class Plugin_FakeHeart : Entity {

    public static readonly MTexture[] sprites = {
        GFX.Game["collectables/heartGem/0/00"],
        GFX.Game["collectables/heartGem/1/00"],
        GFX.Game["collectables/heartGem/2/00"]
    };

    [Option("color")] public heartColors Color = heartColors.Random;

    public override void Render() {
        base.Render();

        MTexture heartSprite;
        switch (Color) {
            case heartColors.Normal:
                heartSprite = sprites[0];
                break;
            case heartColors.BSide:
                heartSprite = sprites[1];
                break;
            case heartColors.CSide:
                heartSprite = sprites[2];
                break;
            default:
                heartSprite = sprites[Calc.Random.Next(0, 3)];
                break;
        }

        heartSprite.DrawCentered(Position);
    }

    protected override IEnumerable<Rectangle> Select() {
        yield return RectOnRelative(new(18), justify: new(0.5f));
    }

    public static void AddPlacements() {
        Placements.Create("Fake Heart", "fakeHeart");
    }

    public enum heartColors {
        Normal = 0,
        BSide = 1,
        CSide = 2,
        Random = -1
    }
}
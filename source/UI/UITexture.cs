using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.UI;

public class UIImage : UIElement {

    public MTexture Texture;
    public float Scale;

    public UIImage(MTexture texture, float scale = 1) {
        Texture = texture;
        Scale = scale;
        AdjustBounds();
    }

    public override void Render(Vector2 position = default) {
        base.Render(position);

        Texture.Draw(position, new(0), Color.White, Scale);
    }

    public UIImage ScaleToFit(Vector2 space) {
        Scale = Math.Min(1, Math.Min(space.X / Texture.Width, space.Y / Texture.Height));
        AdjustBounds();
        return this;
    }

    private void AdjustBounds(){
        Width = (int)(Texture.Width * Scale);
        Height = (int)(Texture.Height * Scale);
    }
}
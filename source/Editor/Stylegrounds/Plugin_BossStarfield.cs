using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Snowberry.Editor.Stylegrounds;

[Plugin("bossStarField")]
public class Plugin_BossStarfield : Styleground {
    // wrapping room area: 48x30.5 = 1464
    // 200/1464 = ~0.1366 lines per tile

    private VertexPositionColor[] verts;

    private static readonly Color[] colors = [
        Calc.HexToColor("030c1b"),
        Calc.HexToColor("0b031b"),
        Calc.HexToColor("1b0319"),
        Calc.HexToColor("0f0301")
    ];

    public override void Render(Room room) {
        base.Render(room);

        Calc.PushRandom((room.Name + "bossstarfield").GetHashCode());

        int count = (int)Math.Ceiling((200 / 1464f) * room.Width * room.Height);
        verts = new VertexPositionColor[6 * (count + 1)];

        Vector2 pos = room.Position * 8;
        Editor.Instance.Camera.Matrix.Decompose(out var scale, out _, out var translation);
        var screen = new Rectangle((int)(pos.X * scale.X + translation.X), (int)(pos.Y * scale.Y + translation.Y), (int)(room.Width * 8 * scale.X), (int)(room.Height * 8 * scale.Y));
        DrawUtil.WithinScissorRectangle(screen, () => {
            Vector3 pos3 = new Vector3(pos.X, pos.Y, 0);
            Color color1 = Color.Black * (Color.A / 255f);
            int bgWidth = room.Width * 8 + 10, bgHeight = room.Height * 8 + 10;
            verts[0].Color = color1;
            verts[0].Position = pos3 + new Vector3(-10f, -10f, 0.0f);
            verts[1].Color = color1;
            verts[1].Position = pos3 + new Vector3(bgWidth, -10f, 0.0f);
            verts[2].Color = color1;
            verts[2].Position = pos3 + new Vector3(bgWidth, bgHeight, 0.0f);
            verts[3].Color = color1;
            verts[3].Position = pos3 + new Vector3(-10f, -10f, 0.0f);
            verts[4].Color = color1;
            verts[4].Position = pos3 + new Vector3(bgWidth, bgHeight, 0.0f);
            verts[5].Color = color1;
            verts[5].Position = pos3 + new Vector3(-10f, bgHeight, 0.0f);
            for(int i = 0; i < count; ++i){
                int idx = (i + 1) * 6;
                float speed = Calc.Random.Range(500f, 1200f);
                Vector2 position = new Vector2(Calc.Random.Range(0, bgWidth + 54), Calc.Random.Range(0, bgHeight + 54));

                float mapSpeed = Calc.ClampedMap(speed, 0.0f, 1200f, 1f, 64f);
                float mapSpeedNeg = Calc.ClampedMap(speed, 0.0f, 1200f, 3f, 0.6f);
                Vector2 direction = room.Width >= room.Height ? new Vector2(-1, 0) : new Vector2(0, -1);
                Vector2 dirPerp = direction.Perpendicular();
                position.X = mod(position.X - pos.X * 0.9f, bgWidth + 54) - 32f;
                position.Y = mod(position.Y - pos.Y * 0.9f, bgHeight + 54) - 32f;
                Vector2 vector2_2 = position - direction * mapSpeed * 0.5f - dirPerp * mapSpeedNeg;
                Vector2 vector2_3 = position + direction * mapSpeed * 1f - dirPerp * mapSpeedNeg;
                Vector2 vector2_4 = position + direction * mapSpeed * 0.5f + dirPerp * mapSpeedNeg;
                Vector2 vector2_5 = position - direction * mapSpeed * 1f + dirPerp * mapSpeedNeg;
                Color color2 = Calc.Random.Choose(colors) * (Color.A / 255f);
                verts[idx].Color = color2;
                verts[idx].Position = pos3 + new Vector3(vector2_2, 0);
                verts[idx + 1].Color = color2;
                verts[idx + 1].Position = pos3 + new Vector3(vector2_3, 0);
                verts[idx + 2].Color = color2;
                verts[idx + 2].Position = pos3 + new Vector3(vector2_4, 0);
                verts[idx + 3].Color = color2;
                verts[idx + 3].Position = pos3 + new Vector3(vector2_2, 0);
                verts[idx + 4].Color = color2;
                verts[idx + 4].Position = pos3 + new Vector3(vector2_4, 0);
                verts[idx + 5].Color = color2;
                verts[idx + 5].Position = pos3 + new Vector3(vector2_5, 0);
            }

            Calc.PopRandom();

            DrawUtil.DrawVerticesWithScissoring(Editor.Instance.Camera.Matrix, verts, verts.Length);
        }, matrix: Editor.Instance.Camera.Matrix);

    }

    private float mod(float x, float m) => (x % m + m) % m;
}
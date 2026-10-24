using GrimmsVenture.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GrimmsVenture.Rendering;

/// <summary>Arena circular procedural: piso xadrez, borda, pilares e rochas flutuantes.</summary>
public sealed class ArenaRenderer
{
    readonly Texture2D _checker;

    public ArenaRenderer(GraphicsDevice gd) => _checker = BuildChecker(gd);

    static Texture2D BuildChecker(GraphicsDevice gd)
    {
        const int size = 128, cell = 16;
        var tex = new Texture2D(gd, size, size);
        var data = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                data[y * size + x] = ((x / cell + y / cell) % 2 == 0)
                    ? new Color(92, 98, 120)
                    : new Color(74, 80, 100);
        tex.SetData(data);
        return tex;
    }

    public void DrawOpaque(Renderer3D r)
    {
        float R = GameConstants.ArenaRadius;

        // abismo
        r.Draw(r.Quad, Matrix.CreateScale(800f) * Matrix.CreateTranslation(0, -3f, 0), new Color(14, 14, 24));

        // borda (topo abaixo do piso para não dar z-fighting)
        r.Draw(r.Cylinder,
            Matrix.CreateScale(R + 1.5f, 2f, R + 1.5f) * Matrix.CreateTranslation(0, -1.05f, 0),
            new Color(64, 56, 76));

        // piso
        r.Draw(r.Cylinder,
            Matrix.CreateScale(R, 1f, R) * Matrix.CreateTranslation(0, -0.5f, 0),
            Color.White, texture: _checker);

        // pilares com luz no topo
        const int pillars = 12;
        for (int i = 0; i < pillars; i++)
        {
            float ang = i * MathHelper.TwoPi / pillars;
            float x = MathF.Cos(ang) * (R + 2.5f);
            float z = MathF.Sin(ang) * (R + 2.5f);
            r.Draw(r.Box, Matrix.CreateScale(2.2f, 16f, 2.2f) * Matrix.CreateTranslation(x, 8f, z), new Color(92, 86, 112));
            r.Draw(r.Sphere, Matrix.CreateScale(0.8f) * Matrix.CreateTranslation(x, 16.8f, z), new Color(255, 170, 80), unlit: true);
        }

        // rochas flutuantes (profundidade visual)
        for (int i = 0; i < 8; i++)
        {
            float ang = i * MathHelper.TwoPi / 8f + 0.4f;
            float rad = 78f + (i % 3) * 10f;
            float h = 12f + (i * 7 % 5) * 5f;
            float s = 4f + (i % 4) * 2f;
            Matrix m = Matrix.CreateScale(s, s * 0.7f, s) * Matrix.CreateRotationY(ang * 2f)
                       * Matrix.CreateTranslation(MathF.Cos(ang) * rad, h, MathF.Sin(ang) * rad);
            r.Draw(r.Box, m, new Color(70, 64, 86));
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GrimmsVenture.Rendering;

/// <summary>Gera primitivas 3D proceduralmente (sem Content Pipeline). Winding é corrigido automaticamente.</summary>
public static class MeshFactory
{
    sealed class Builder
    {
        readonly List<VertexPositionNormalTexture> _v = new();
        readonly List<ushort> _i = new();

        public int Vertex(Vector3 p, Vector3 n, Vector2 uv)
        {
            _v.Add(new VertexPositionNormalTexture(p, n, uv));
            return _v.Count - 1;
        }

        public void Tri(int a, int b, int c)
        {
            _i.Add((ushort)a);
            _i.Add((ushort)b);
            _i.Add((ushort)c);
        }

        public Mesh Build(GraphicsDevice gd)
        {
            // garante CCW coerente com as normais (independe de como o gerador ordenou)
            for (int k = 0; k < _i.Count; k += 3)
            {
                var v0 = _v[_i[k]];
                var v1 = _v[_i[k + 1]];
                var v2 = _v[_i[k + 2]];
                Vector3 geo = Vector3.Cross(v1.Position - v0.Position, v2.Position - v0.Position);
                Vector3 n = v0.Normal + v1.Normal + v2.Normal;
                if (Vector3.Dot(geo, n) < 0f)
                    (_i[k + 1], _i[k + 2]) = (_i[k + 2], _i[k + 1]);
            }
            return new Mesh(gd, _v.ToArray(), _i.ToArray());
        }
    }

    /// <summary>Cubo 1x1x1 centrado na origem.</summary>
    public static Mesh Box(GraphicsDevice gd)
    {
        var b = new Builder();
        Vector3[] normals = { Vector3.Right, Vector3.Left, Vector3.Up, Vector3.Down, Vector3.Backward, Vector3.Forward };
        foreach (Vector3 n in normals)
        {
            Vector3 helper = MathF.Abs(n.Y) > 0.9f ? Vector3.Right : Vector3.Up;
            Vector3 t1 = Vector3.Normalize(Vector3.Cross(n, helper));
            Vector3 t2 = Vector3.Cross(n, t1);
            Vector3 c = n * 0.5f;

            int a = b.Vertex(c - t1 * 0.5f - t2 * 0.5f, n, new Vector2(0, 0));
            int bb = b.Vertex(c + t1 * 0.5f - t2 * 0.5f, n, new Vector2(1, 0));
            int cc = b.Vertex(c + t1 * 0.5f + t2 * 0.5f, n, new Vector2(1, 1));
            int d = b.Vertex(c - t1 * 0.5f + t2 * 0.5f, n, new Vector2(0, 1));
            b.Tri(a, bb, cc);
            b.Tri(a, cc, d);
        }
        return b.Build(gd);
    }

    /// <summary>Esfera de raio 1 centrada na origem.</summary>
    public static Mesh Sphere(GraphicsDevice gd, int rings, int segments)
    {
        var b = new Builder();
        for (int lat = 0; lat <= rings; lat++)
        {
            float theta = lat * MathHelper.Pi / rings;
            float sinT = MathF.Sin(theta), cosT = MathF.Cos(theta);
            for (int lon = 0; lon <= segments; lon++)
            {
                float phi = lon * MathHelper.TwoPi / segments;
                Vector3 n = new(sinT * MathF.Cos(phi), cosT, sinT * MathF.Sin(phi));
                b.Vertex(n, n, new Vector2((float)lon / segments, (float)lat / rings));
            }
        }
        for (int lat = 0; lat < rings; lat++)
        {
            for (int lon = 0; lon < segments; lon++)
            {
                int a = lat * (segments + 1) + lon;
                int c = a + segments + 1;
                b.Tri(a, a + 1, c);
                b.Tri(a + 1, c + 1, c);
            }
        }
        return b.Build(gd);
    }

    /// <summary>Cilindro de raio 1 e altura 1 centrado na origem (eixo Y), com tampas.</summary>
    public static Mesh Cylinder(GraphicsDevice gd, int segments, float capUvScale)
    {
        var b = new Builder();

        for (int i = 0; i <= segments; i++)
        {
            float ang = i * MathHelper.TwoPi / segments;
            float x = MathF.Cos(ang), z = MathF.Sin(ang);
            Vector3 n = new(x, 0, z);
            float u = (float)i / segments;
            b.Vertex(new Vector3(x, -0.5f, z), n, new Vector2(u, 1));
            b.Vertex(new Vector3(x, 0.5f, z), n, new Vector2(u, 0));
        }
        for (int i = 0; i < segments; i++)
        {
            int a = i * 2;
            b.Tri(a, a + 1, a + 2);
            b.Tri(a + 1, a + 3, a + 2);
        }

        AddCap(b, segments, 0.5f, Vector3.Up, capUvScale);
        AddCap(b, segments, -0.5f, Vector3.Down, capUvScale);
        return b.Build(gd);
    }

    static void AddCap(Builder b, int segments, float y, Vector3 normal, float uvScale)
    {
        int center = b.Vertex(new Vector3(0, y, 0), normal, new Vector2(0.5f, 0.5f) * uvScale);
        int first = -1;
        for (int i = 0; i <= segments; i++)
        {
            float ang = i * MathHelper.TwoPi / segments;
            float x = MathF.Cos(ang), z = MathF.Sin(ang);
            int v = b.Vertex(new Vector3(x, y, z), normal, new Vector2(x * 0.5f + 0.5f, z * 0.5f + 0.5f) * uvScale);
            if (first < 0) first = v;
            if (i > 0) b.Tri(center, v - 1, v);
        }
    }

    /// <summary>Quad 1x1 no plano XZ (normal para cima).</summary>
    public static Mesh Quad(GraphicsDevice gd, float uvScale)
    {
        var b = new Builder();
        int a = b.Vertex(new Vector3(-0.5f, 0, -0.5f), Vector3.Up, new Vector2(0, 0));
        int bb = b.Vertex(new Vector3(0.5f, 0, -0.5f), Vector3.Up, new Vector2(uvScale, 0));
        int c = b.Vertex(new Vector3(0.5f, 0, 0.5f), Vector3.Up, new Vector2(uvScale, uvScale));
        int d = b.Vertex(new Vector3(-0.5f, 0, 0.5f), Vector3.Up, new Vector2(0, uvScale));
        b.Tri(a, bb, c);
        b.Tri(a, c, d);
        return b.Build(gd);
    }
}

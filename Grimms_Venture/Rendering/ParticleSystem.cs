using Microsoft.Xna.Framework;

namespace GrimmsVenture.Rendering;

public struct Particle
{
    public Vector3 Pos, Vel;
    public float Life, MaxLife, Size;
    public Color Color;
}

/// <summary>Pool fixo de partículas (sem alocação por frame). Desenhadas como esferas low-poly.</summary>
public sealed class ParticleSystem
{
    readonly Particle[] _p = new Particle[512];
    readonly Random _rng = new(7);
    int _next;

    public void Clear() => Array.Clear(_p);

    public void Burst(Vector3 pos, Color color, int count, float speed, float size, float life = 0.5f)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 dir = new(Rand(), Rand(), Rand());
            dir = dir.LengthSquared() > 0.0001f ? Vector3.Normalize(dir) : Vector3.Up;

            _p[_next] = new Particle
            {
                Pos = pos,
                Vel = dir * speed * (0.4f + 0.6f * (float)_rng.NextDouble()),
                Life = life * (0.6f + 0.4f * (float)_rng.NextDouble()),
                MaxLife = life,
                Size = size,
                Color = color
            };
            _next = (_next + 1) % _p.Length;
        }
    }

    public void Update(float dt)
    {
        float drag = MathF.Exp(-3f * dt);
        for (int i = 0; i < _p.Length; i++)
        {
            ref Particle p = ref _p[i];
            if (p.Life <= 0f) continue;
            p.Life -= dt;
            p.Pos += p.Vel * dt;
            p.Vel *= drag;
            p.Vel.Y -= 4f * dt;
        }
    }

    public void Draw(Renderer3D r)
    {
        for (int i = 0; i < _p.Length; i++)
        {
            Particle p = _p[i];
            if (p.Life <= 0f) continue;
            float t = MathHelper.Clamp(p.Life / p.MaxLife, 0f, 1f);
            Matrix world = Matrix.CreateScale(p.Size * (0.4f + 0.6f * t)) * Matrix.CreateTranslation(p.Pos);
            r.Draw(r.SphereLow, world, p.Color, t, unlit: true);
        }
    }

    float Rand() => (float)(_rng.NextDouble() * 2.0 - 1.0);
}

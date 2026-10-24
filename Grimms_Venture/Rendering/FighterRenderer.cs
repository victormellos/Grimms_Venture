using GrimmsVenture.Combat;
using Microsoft.Xna.Framework;

namespace GrimmsVenture.Rendering;

/// <summary>
/// Placeholder visual: boneco montado com primitivas + pose procedural.
/// Quando entrarem modelos reais, só este arquivo precisa ser substituído.
/// </summary>
public static class FighterRenderer
{
    static readonly Color Skin = new(236, 190, 150);

    // ---------------------------------------------------------------- opaque pass

    public static void DrawBody(Renderer3D r, Fighter f)
    {
        bool ko = f.State == FighterState.KO;
        float bob = ko ? 0f : MathF.Sin(f.Time * 3f + f.PlayerIndex) * (f.IsAirborne ? 0.08f : 0.03f);

        // KO: deita de costas (frente do modelo aponta para cima)
        Matrix body = ko
            ? Matrix.CreateWorld(f.Position + Vector3.Up * 0.35f, Vector3.Up, -f.Forward)
            : Matrix.CreateWorld(f.Position + Vector3.Up * bob, f.Forward, Vector3.Up);

        float flash = MathHelper.Clamp(f.HitFlash / 0.15f, 0f, 1f);
        Color main = Color.Lerp(f.Color, Color.White, flash);
        Color dark = Color.Lerp(new Color(f.Color.ToVector3() * 0.45f), Color.White, flash);
        Color skin = Color.Lerp(Skin, Color.White, flash);

        GetArmPose(f, out Vector3 armL, out Vector3 armR);
        GetLegPose(f, out Vector3 legL, out Vector3 legR);

        void BoxPart(Vector3 c, Vector3 size, Color col)
            => r.Draw(r.Box, Matrix.CreateScale(size) * Matrix.CreateTranslation(c) * body, col);

        void Ball(Vector3 c, float radius, Color col)
            => r.Draw(r.Sphere, Matrix.CreateScale(radius) * Matrix.CreateTranslation(c) * body, col);

        void Limb(Vector3 joint, Vector3 dir, float len, float thick, Color col)
        {
            dir = Vector3.Normalize(dir);
            Vector3 fwd = Vector3.Cross(dir, Vector3.Right);
            if (fwd.LengthSquared() < 1e-4f) fwd = Vector3.Forward;
            Matrix rot = Matrix.CreateWorld(Vector3.Zero, fwd, dir); // eixo Y local -> dir
            Matrix m = Matrix.CreateScale(thick, len, thick) * rot
                       * Matrix.CreateTranslation(joint + dir * (len * 0.5f)) * body;
            r.Draw(r.Box, m, col);
        }

        // tronco, quadril, capa, cabeça
        BoxPart(new Vector3(0, 1.35f, 0), new Vector3(0.85f, 0.9f, 0.45f), main);
        BoxPart(new Vector3(0, 0.88f, 0), new Vector3(0.70f, 0.25f, 0.40f), dark);
        BoxPart(new Vector3(0, 1.20f, 0.32f), new Vector3(0.75f, 1.10f, 0.05f), dark);
        Ball(new Vector3(0, 1.98f, 0), 0.30f, skin);
        BoxPart(new Vector3(0, 2.05f, -0.02f), new Vector3(0.62f, 0.10f, 0.62f), dark); // faixa/cabelo

        // braços + punhos
        Vector3 shoulderL = new(-0.55f, 1.70f, 0), shoulderR = new(0.55f, 1.70f, 0);
        Limb(shoulderL, armL, 0.8f, 0.22f, main);
        Limb(shoulderR, armR, 0.8f, 0.22f, main);
        Ball(shoulderL + Vector3.Normalize(armL) * 0.82f, 0.15f, skin);
        Ball(shoulderR + Vector3.Normalize(armR) * 0.82f, 0.15f, skin);

        // pernas + botas
        Vector3 hipL = new(-0.20f, 0.80f, 0), hipR = new(0.20f, 0.80f, 0);
        Limb(hipL, legL, 0.85f, 0.26f, dark);
        Limb(hipR, legR, 0.85f, 0.26f, dark);
        Ball(hipL + Vector3.Normalize(legL) * 0.85f, 0.17f, main);
        Ball(hipR + Vector3.Normalize(legR) * 0.85f, 0.17f, main);
    }

    // ---------------------------------------------------------------- transparent pass

    public static void DrawEffects(Renderer3D r, Fighter f, bool debug)
    {
        // sombra no chão (ajuda MUITO a julgar altura)
        float h = f.Position.Y;
        float alpha = MathHelper.Clamp(0.55f - h * 0.02f, 0.12f, 0.55f);
        float rad = 0.9f + h * 0.02f;
        r.Draw(r.Cylinder,
            Matrix.CreateScale(rad, 0.02f, rad) * Matrix.CreateTranslation(f.Position.X, 0.03f, f.Position.Z),
            Color.Black, alpha, unlit: true);

        float pulse = 0.5f + 0.5f * MathF.Sin(f.Time * 14f);

        switch (f.State)
        {
            case FighterState.Charging:
                r.Draw(r.Sphere, Matrix.CreateScale(1.5f + 0.25f * pulse) * Matrix.CreateTranslation(f.HurtCenter),
                    f.Color, 0.28f, unlit: true);
                r.Draw(r.Sphere, Matrix.CreateScale(2.1f + 0.35f * pulse) * Matrix.CreateTranslation(f.HurtCenter),
                    Color.White, 0.12f, unlit: true);
                break;
            case FighterState.Guarding:
                r.Draw(r.Sphere, Matrix.CreateScale(1.6f) * Matrix.CreateTranslation(f.HurtCenter),
                    new Color(120, 200, 255), 0.25f, unlit: true);
                break;
            case FighterState.Dashing:
                r.Draw(r.Sphere, Matrix.CreateScale(1.1f, 1.4f, 1.1f) * Matrix.CreateTranslation(f.HurtCenter),
                    f.Color, 0.22f, unlit: true);
                break;
        }

        // efeito do golpe corpo a corpo
        if (f.IsAttackActive && f.CurrentAttack != null)
        {
            r.Draw(r.Sphere, Matrix.CreateScale(f.CurrentAttack.Radius * 0.75f) * Matrix.CreateTranslation(f.MeleeCenter),
                Color.White, 0.45f, unlit: true);
        }

        if (debug)
        {
            r.Draw(r.Sphere, Matrix.CreateScale(f.HurtRadius) * Matrix.CreateTranslation(f.HurtCenter),
                new Color(60, 255, 90), 0.25f, unlit: true);
            if (f.IsAttackActive && f.CurrentAttack != null)
                r.Draw(r.Sphere, Matrix.CreateScale(f.CurrentAttack.Radius) * Matrix.CreateTranslation(f.MeleeCenter),
                    new Color(255, 60, 60), 0.35f, unlit: true);
        }
    }

    public static void DrawBlasts(Renderer3D r, IReadOnlyList<KiBlastProjectile> blasts)
    {
        foreach (KiBlastProjectile b in blasts)
        {
            r.Draw(r.Sphere, Matrix.CreateScale(b.Radius * 1.7f) * Matrix.CreateTranslation(b.Position),
                b.Owner.Color, 0.30f, unlit: true);
            r.Draw(r.Sphere, Matrix.CreateScale(b.Radius * 0.8f) * Matrix.CreateTranslation(b.Position),
                Color.White, 1f, unlit: true);
        }
    }

    // ---------------------------------------------------------------- poses (espaço local)

    static void GetArmPose(Fighter f, out Vector3 l, out Vector3 r)
    {
        Vector3 restL = new(-0.30f, -1f, 0f), restR = new(0.30f, -1f, 0f);
        l = restL;
        r = restR;

        switch (f.State)
        {
            case FighterState.Idle:
            case FighterState.Moving:
            {
                float sway = MathF.Sin(f.Time * 2.5f) * 0.08f;
                l = new Vector3(-0.30f, -1f, -0.10f + sway);
                r = new Vector3(0.30f, -1f, -0.10f - sway);
                if (f.State == FighterState.Moving && !f.IsAirborne)
                {
                    float s = MathF.Sin(f.Time * 10f) * 0.6f;
                    l.Z = s;
                    r.Z = -s;
                }
                break;
            }
            case FighterState.Dashing:
                l = new Vector3(-0.2f, -0.5f, 0.9f);
                r = new Vector3(0.2f, -0.5f, 0.9f);
                break;
            case FighterState.Guarding:
                l = new Vector3(0.55f, 0.5f, -0.8f);
                r = new Vector3(-0.55f, 0.5f, -0.8f);
                break;
            case FighterState.Charging:
                l = new Vector3(-0.9f, -0.5f, 0.1f);
                r = new Vector3(0.9f, -0.5f, 0.1f);
                break;
            case FighterState.HitStun:
                l = new Vector3(-0.5f, 0.2f, 0.9f);
                r = new Vector3(0.5f, 0.2f, 0.9f);
                break;
            case FighterState.Attacking:
                if (f.CurrentAttack != null) AttackArms(f, f.CurrentAttack, restL, restR, out l, out r);
                break;
        }
    }

    static void AttackArms(Fighter f, AttackData a, Vector3 restL, Vector3 restR, out Vector3 l, out Vector3 r)
    {
        float t = f.StateTime;
        float ext;
        if (t < a.Startup) ext = -0.35f * (t / MathF.Max(a.Startup, 0.001f));
        else if (t < a.Startup + a.Active) ext = 1f;
        else ext = MathHelper.Lerp(1f, 0f, MathHelper.Clamp((t - a.Startup - a.Active) / MathF.Max(a.Recovery, 0.001f), 0f, 1f));

        Vector3 punchL = new(-0.1f, 0.1f, -1f), punchR = new(0.1f, 0.1f, -1f);
        l = restL;
        r = restR;

        switch (a.Kind)
        {
            case AttackKind.Light:
                if (a.LightIndex % 2 == 0) r = Vector3.Lerp(restR, punchR, ext);
                else l = Vector3.Lerp(restL, punchL, ext);
                break;

            case AttackKind.Heavy:
                if (t < a.Startup)
                {
                    float p = t / MathF.Max(a.Startup, 0.001f); // levanta os braços
                    l = Vector3.Lerp(restL, new Vector3(-0.2f, 1f, 0.3f), p);
                    r = Vector3.Lerp(restR, new Vector3(0.2f, 1f, 0.3f), p);
                }
                else
                {
                    Vector3 smashL = new(-0.1f, -0.3f, -1f), smashR = new(0.1f, -0.3f, -1f);
                    l = Vector3.Lerp(restL, smashL, ext);
                    r = Vector3.Lerp(restR, smashR, ext);
                }
                break;

            case AttackKind.KiBlast:
            {
                float e = MathHelper.Clamp(ext, 0f, 1f);
                float pull = t < a.Startup ? t / MathF.Max(a.Startup, 0.001f) : 1f;
                e = MathF.Max(e, pull);
                l = Vector3.Lerp(restL, punchL, e);
                r = Vector3.Lerp(restR, punchR, e);
                break;
            }
        }
    }

    static void GetLegPose(Fighter f, out Vector3 l, out Vector3 r)
    {
        l = new Vector3(-0.10f, -1f, 0f);
        r = new Vector3(0.10f, -1f, 0f);
        if (f.State == FighterState.KO) return;

        if (f.IsAirborne)
        {
            l = new Vector3(-0.08f, -1f, 0.35f);
            r = new Vector3(0.08f, -1f, 0.20f);
        }
        else if (f.State == FighterState.Moving)
        {
            float s = MathF.Sin(f.Time * 10f) * 0.6f;
            l = new Vector3(-0.1f, -1f, s);
            r = new Vector3(0.1f, -1f, -s);
        }
        else if (f.State == FighterState.Charging)
        {
            l = new Vector3(-0.35f, -1f, 0f);
            r = new Vector3(0.35f, -1f, 0f);
        }
    }
}

using GrimmsVenture.Core;
using GrimmsVenture.Rendering;
using Microsoft.Xna.Framework;

namespace GrimmsVenture.Combat;

/// <summary>Resolve hits, projéteis, colisão entre corpos, hit-stop e partículas.</summary>
public sealed class CombatSystem
{
    readonly List<KiBlastProjectile> _blasts = new();

    public IReadOnlyList<KiBlastProjectile> Blasts => _blasts;
    public ParticleSystem Particles { get; } = new();
    public float HitStop { get; private set; }

    /// <summary>Disparado quando algo pede tremor de câmera (0..1).</summary>
    public event Action<float>? ShakeRequested;

    public void Reset()
    {
        _blasts.Clear();
        Particles.Clear();
        HitStop = 0f;
    }

    public void TickHitStop(float dt) => HitStop = MathF.Max(0f, HitStop - dt);

    public void Update(float dt, Fighter f1, Fighter f2)
    {
        UpdateBlasts(dt, f1, f2);
        ResolveBodies(f1, f2);
    }

    // ------------------------------------------------------------------ melee

    public bool TryMelee(Fighter attacker, Fighter defender, AttackData attack, Vector3 center)
    {
        if (defender.State == FighterState.KO) return false;
        float dist = Vector3.Distance(center, defender.HurtCenter);
        if (dist > attack.Radius + defender.HurtRadius) return false;

        Vector3 hitPoint = Vector3.Lerp(center, defender.HurtCenter, 0.5f);
        Vector3 dir = defender.HurtCenter - attacker.HurtCenter;
        ApplyHit(attacker, defender, attack, hitPoint, dir);
        return true;
    }

    // ------------------------------------------------------------------ projectiles

    public void SpawnKiBlast(Fighter owner, Fighter target, AttackData data)
    {
        Vector3 origin = owner.Position + Vector3.Up * 1.4f + owner.Forward * 0.9f;
        Vector3 toTarget = target.HurtCenter - origin;
        Vector3 dir = toTarget.LengthSquared() > 0.0001f ? Vector3.Normalize(toTarget) : owner.Forward;

        _blasts.Add(new KiBlastProjectile
        {
            Owner = owner,
            Data = data,
            Position = origin,
            Velocity = dir * data.ProjectileSpeed,
            Radius = data.ProjectileRadius
        });

        Particles.Burst(origin, owner.Color, 8, 5f, 0.15f);
    }

    void UpdateBlasts(float dt, Fighter f1, Fighter f2)
    {
        for (int i = _blasts.Count - 1; i >= 0; i--)
        {
            KiBlastProjectile b = _blasts[i];
            b.Position += b.Velocity * dt;
            b.Life -= dt;

            Fighter target = b.Owner == f1 ? f2 : f1;
            bool remove = false;

            if (target.State != FighterState.KO &&
                Vector3.Distance(b.Position, target.HurtCenter) <= b.Radius + target.HurtRadius)
            {
                ApplyHit(b.Owner, target, b.Data, b.Position, b.Velocity);
                Explode(b.Position, b.Owner.Color);
                remove = true;
            }
            else if (b.Position.Y < 0.1f)
            {
                Explode(b.Position, b.Owner.Color);
                remove = true;
            }
            else if (b.Life <= 0f ||
                     new Vector2(b.Position.X, b.Position.Z).Length() > GameConstants.ArenaRadius + 30f)
            {
                remove = true;
            }

            if (remove) _blasts.RemoveAt(i);
        }
    }

    void Explode(Vector3 pos, Color color)
    {
        Particles.Burst(pos, Color.White, 12, 10f, 0.2f, 0.45f);
        Particles.Burst(pos, color, 20, 8f, 0.28f, 0.6f);
        ShakeRequested?.Invoke(0.25f);
    }

    // ------------------------------------------------------------------ shared hit logic

    void ApplyHit(Fighter attacker, Fighter defender, AttackData a, Vector3 hitPoint, Vector3 direction)
    {
        direction = direction.LengthSquared() > 0.0001f ? Vector3.Normalize(direction) : attacker.Forward;
        Vector3 knockback = direction * a.Knockback;
        float damage = a.Damage * attacker.Stats.DamageMultiplier;

        if (defender.State == FighterState.Guarding)
        {
            float kiDrain = defender.Stats.BlockKiDrain * (a.Damage / 40f);
            defender.ReceiveBlock(damage * defender.Stats.BlockChipFraction, knockback * 0.4f, kiDrain);
            Particles.Burst(hitPoint, new Color(120, 200, 255), 8, 6f, 0.12f);
            HitStop = MathF.Max(HitStop, 0.03f);
            ShakeRequested?.Invoke(0.12f);
            return;
        }

        damage *= attacker.ComboScale;
        defender.ReceiveHit(damage, knockback, a.HitStun);
        attacker.RegisterHit(a.HitStun);
        attacker.AddKi(a.KiGain);

        Particles.Burst(hitPoint, Color.White, 10, 9f, 0.15f);
        Particles.Burst(hitPoint, attacker.Color, 14, 7f, 0.2f);

        bool ko = defender.State == FighterState.KO;
        HitStop = MathF.Max(HitStop, ko ? 0.30f : a.HitStop);
        ShakeRequested?.Invoke(ko ? 0.9f : a.Shake);
    }

    // ------------------------------------------------------------------ bodies

    void ResolveBodies(Fighter a, Fighter b)
    {
        if (a.State == FighterState.KO || b.State == FighterState.KO) return;

        Vector3 d = b.HurtCenter - a.HurtCenter;
        float dist = d.Length();
        float min = GameConstants.MinFighterSeparation;
        if (dist >= min) return;

        Vector3 n = dist > 0.001f ? d / dist : Vector3.Right;
        Vector3 push = n * ((min - dist) * 0.5f);
        a.Position -= push;
        b.Position += push;
    }
}

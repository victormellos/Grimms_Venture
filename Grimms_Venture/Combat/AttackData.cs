namespace GrimmsVenture.Combat;

public enum AttackKind { Light, Heavy, KiBlast }

/// <summary>Dados puros de um golpe (frame data em segundos). Fácil de mover para JSON depois.</summary>
public sealed class AttackData
{
    public string Name { get; init; } = "";
    public AttackKind Kind { get; init; }
    public int LightIndex { get; init; } = -1;

    public float Startup { get; init; }
    public float Active { get; init; }
    public float Recovery { get; init; }

    public float Damage { get; init; }
    public float Range { get; init; } = 1.5f;
    public float Radius { get; init; } = 0.8f;
    public float HitStun { get; init; } = 0.4f;
    public float Knockback { get; init; } = 5f;
    public float LungeSpeed { get; init; }

    public float KiCost { get; init; }
    public float KiGain { get; init; } = 3f;

    public float HitStop { get; init; } = 0.05f;
    public float Shake { get; init; } = 0.2f;

    public float ProjectileSpeed { get; init; }
    public float ProjectileRadius { get; init; } = 0.5f;

    public bool IsProjectile => Kind == AttackKind.KiBlast;
    public float Total => Startup + Active + Recovery;
}

public static class AttackLibrary
{
    public static readonly AttackData[] Light =
    {
        new() { Name = "Light 1", Kind = AttackKind.Light, LightIndex = 0, Startup = 0.08f, Active = 0.08f, Recovery = 0.14f,
                Damage = 30f, HitStun = 0.42f, Knockback = 4f, LungeSpeed = 14f, HitStop = 0.04f, Shake = 0.12f },
        new() { Name = "Light 2", Kind = AttackKind.Light, LightIndex = 1, Startup = 0.08f, Active = 0.08f, Recovery = 0.14f,
                Damage = 35f, HitStun = 0.42f, Knockback = 4f, LungeSpeed = 14f, HitStop = 0.04f, Shake = 0.12f },
        new() { Name = "Light 3", Kind = AttackKind.Light, LightIndex = 2, Startup = 0.10f, Active = 0.10f, Recovery = 0.32f,
                Damage = 50f, HitStun = 0.40f, Knockback = 14f, LungeSpeed = 14f, HitStop = 0.08f, Shake = 0.30f }
    };

    public static readonly AttackData Heavy = new()
    {
        Name = "Heavy", Kind = AttackKind.Heavy,
        Startup = 0.30f, Active = 0.12f, Recovery = 0.45f,
        Damage = 110f, Range = 1.8f, Radius = 1.0f,
        HitStun = 0.60f, Knockback = 22f, LungeSpeed = 16f,
        KiGain = 8f, HitStop = 0.12f, Shake = 0.55f
    };

    public static readonly AttackData KiBlast = new()
    {
        Name = "Ki Blast", Kind = AttackKind.KiBlast,
        Startup = 0.25f, Active = 0f, Recovery = 0.35f,
        Damage = 70f, HitStun = 0.50f, Knockback = 10f,
        KiCost = 15f, KiGain = 2f, HitStop = 0.06f, Shake = 0.35f,
        ProjectileSpeed = 32f, ProjectileRadius = 0.5f
    };
}

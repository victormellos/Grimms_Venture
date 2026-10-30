using Microsoft.Xna.Framework;

namespace GrimmsVenture.Combat;

/// <summary>
/// GRIMM — heavyweight terrestre: so punhos e chute, sem ki (dash e guarda de graca).
/// Cada personagem do jogo mora num arquivo destes em Combat/Characters/.
/// Para criar outro: copie Template.cs e registre em Roster.All.
/// </summary>
public static class Grimm
{
    // ------------------------------ MOVES (frame data em segundos) ------------------------------
    public static readonly AttackData[] Punches =
    {
        new() { Name = "Jab",   Kind = AttackKind.Light, LightIndex = 0, Startup = 0.06f, Active = 0.07f, Recovery = 0.10f,
                Damage = 45f, HitStun = 0.40f, Knockback = 5f, LungeSpeed = 15f, KiGain = 0f, HitStop = 0.05f, Shake = 0.15f },
        new() { Name = "Straight", Kind = AttackKind.Light, LightIndex = 1, Startup = 0.06f, Active = 0.07f, Recovery = 0.12f,
                Damage = 52f, HitStun = 0.40f, Knockback = 6f, LungeSpeed = 15f, KiGain = 0f, HitStop = 0.05f, Shake = 0.18f },
        new() { Name = "Cross", Kind = AttackKind.Light, LightIndex = 2, Startup = 0.08f, Active = 0.08f, Recovery = 0.22f,
                Damage = 75f, HitStun = 0.45f, Knockback = 16f, LungeSpeed = 15f, KiGain = 0f, HitStop = 0.09f, Shake = 0.35f }
    };

    public static readonly AttackData Kick = new()
    {
        Name = "Kick", Kind = AttackKind.Heavy,
        Startup = 0.20f, Active = 0.12f, Recovery = 0.28f,
        Damage = 140f, Range = 2.0f, Radius = 1.1f,
        HitStun = 0.65f, Knockback = 24f, LungeSpeed = 17f,
        KiGain = 0f, HitStop = 0.14f, Shake = 0.60f
    };

    // ------------------------------ FICHA ------------------------------
    public static readonly CharacterDefinition Def = new()
    {
        Name = "GRIMM",
        Tagline = "HEAVYWEIGHT BRAWLER",
        ModelAsset = "Models/Grimm",
        LightLabel = "PUNCH COMBO",
        HeavyLabel = "KICK",
        Color = new Color(220, 70, 50),
        AltColor = new Color(50, 180, 200),
        Stats = new FighterStats
        {
            MaxHealth = 2000f,       // DOBRO do padrao (1000)
            CanFly = false,          // pulos grandes + pulo duplo no lugar de voar
            Gravity = 26f,
            JumpSpeed = 13f,
            MaxAirJumps = 1,         // pulo duplo
            AirJumpSpeed = 11f,
            HasKi = false            // sem ki: sem carregar, sem ki blast, dash/guarda de graca
        },
        Attacks = new AttackSet
        {
            Light = Punches,
            Heavy = Kick,
            KiBlast = null           // ele NAO tem ki blast
        }
        // Clips: usa o mapa padrao (ClipNames.Default) — as actions do FBX dele
        // (Idle, Walk, Punch1..3, Kick...) batem com os nomes de la.
    };
}

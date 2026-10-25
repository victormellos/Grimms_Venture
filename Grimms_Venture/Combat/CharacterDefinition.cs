using Microsoft.Xna.Framework;

namespace GrimmsVenture.Combat;

public sealed class CharacterDefinition
{
    public string Name { get; init; } = "";
    public string Tagline { get; init; } = "";
    public Color Color { get; init; } = Color.White;      // cor do P1
    public Color AltColor { get; init; } = Color.White;   // cor do P2 (permite partida espelhada)
    public FighterStats Stats { get; init; } = new();
    public AttackSet Attacks { get; init; } = new();
    public string? ModelAsset { get; init; }              // ex.: "Models/Grimm" (null = boneco procedural)
}

/// <summary>Elenco. Para adicionar personagens, é só incluir em All.</summary>
public static class Roster
{
    public static readonly CharacterDefinition Grimm = new()
    {
        Name = "GRIMM",
        Tagline = "HEAVYWEIGHT BRAWLER",
        ModelAsset = "Models/Grimm",
        Color = new Color(220, 70, 50),
        AltColor = new Color(50, 180, 200),
        Stats = new FighterStats
        {
            MaxHealth = 2000f,       // DOBRO do padrão (1000)
            CanFly = false,          // pulos grandes + pulo duplo no lugar de voar
            Gravity = 26f,
            JumpSpeed = 13f,
            MaxAirJumps = 1,         // pulo duplo
            AirJumpSpeed = 11f,
            HasKi = false            // sem ki: sem carregar, sem ki blast, dash/guarda de graça
        },
        Attacks = new AttackSet
        {
            Light = GrimmMoveset.Punches,
            Heavy = GrimmMoveset.Kick,
            KiBlast = null           // ele NÃO tem ki blast
        }
    };

    public static readonly IReadOnlyList<CharacterDefinition> All = new[] { Grimm };
}
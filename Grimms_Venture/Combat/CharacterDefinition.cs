using Microsoft.Xna.Framework;

namespace GrimmsVenture.Combat;

public sealed class CharacterDefinition
{
    public string Name { get; init; } = "";
    public Color Color { get; init; } = Color.White;
    public FighterStats Stats { get; init; } = new();
}

/// <summary>Elenco inicial (placeholders originais). Novos personagens entram aqui.</summary>
public static class Roster
{
    public static readonly CharacterDefinition Grimm = new()
    {
        Name = "GRIMM",
        Color = new Color(220, 70, 50),
        Stats = new FighterStats()
    };

    public static readonly CharacterDefinition Wren = new()
    {
        Name = "WREN",
        Color = new Color(50, 180, 200),
        Stats = new FighterStats
        {
            MaxHealth = 950f,
            MoveSpeed = 10f,
            DashSpeed = 32f,
            DamageMultiplier = 0.92f
        }
    };

    public static readonly IReadOnlyList<CharacterDefinition> All = new[] { Grimm, Wren };
}

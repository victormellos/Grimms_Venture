using Microsoft.Xna.Framework;

namespace GrimmsVenture.Combat;

public sealed class CharacterDefinition
{
    public string Name { get; init; } = "";
    public string Tagline { get; init; } = "";
    public Color Color { get; init; } = Color.White;      // cor do P1
    public Color AltColor { get; init; } = Color.White;   // cor do P2 (permite partida espelhada)
    public FighterStats Stats { get; init; } = new();
}

/// <summary>Elenco. Para adicionar personagens, é só incluir em All.</summary>
public static class Roster
{
    public static readonly CharacterDefinition Grimm = new()
    {
        Name = "GRIMM",
        Tagline = "BALANCED FIGHTER",
        Color = new Color(220, 70, 50),
        AltColor = new Color(50, 180, 200),
        Stats = new FighterStats()
    };

    public static readonly IReadOnlyList<CharacterDefinition> All = new[] { Grimm };
}

using System;
using System.Collections.Generic;
using System.Linq;
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
    public string LightLabel { get; init; } = "LIGHT ATTACK";   // texto na dica de controles
    public string HeavyLabel { get; init; } = "HEAVY ATTACK";
    public string? ModelAsset { get; init; }              // ex.: "Models/Grimm" (null = boneco procedural)

    /// <summary>
    /// Estado/golpe -> nome da action no FBX. Raramente precisa mudar:
    /// so se o teu modelo tiver animacoes com nomes diferentes do padrao.
    /// </summary>
    public IReadOnlyDictionary<string, string> Clips { get; init; } = ClipNames.Default;
}

/// <summary>
/// Mapa padrao estado/golpe -> clip do FBX. Se o Blender nomear as actions
/// assim, nenhuma ficha precisa tocar em Clips.
/// </summary>
public static class ClipNames
{
    public static readonly IReadOnlyDictionary<string, string> Default = new Dictionary<string, string>
    {
        // estados
        ["Idle"]    = "Idle",
        ["Walk"]    = "Walk",
        ["Air"]     = "Air",
        ["Guard"]   = "Guard",
        ["Charge"]  = "Charge",
        ["HitStun"] = "HitStun",
        ["KO"]      = "KO",
        // golpes (chave = Name do AttackData)
        ["Jab"]      = "Punch1",
        ["Straight"] = "Punch2",
        ["Cross"]    = "Punch3",
        ["Kick"]     = "Kick",
        ["Light 1"]  = "Punch1",
        ["Light 2"]  = "Punch2",
        ["Light 3"]  = "Punch3",
        ["Heavy"]    = "Kick",
        ["Ki Blast"] = "Kick"
    };
}

/// <summary>
/// Elenco do jogo. Personagem novo = 1 arquivo em Combat/Characters/ + 1 linha aqui.
/// </summary>
public static class Roster
{
    public static readonly IReadOnlyList<CharacterDefinition> All = new[]
    {
        Grimm.Def,
        Nova.Def
    };

    public static CharacterDefinition ByName(string name)
        => All.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
           ?? All[0];
}

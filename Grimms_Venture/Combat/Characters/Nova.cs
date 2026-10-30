using Microsoft.Xna.Framework;

namespace GrimmsVenture.Combat;

/// <summary>
/// NOVA — personagem de demonstracao do sistema: voa, usa ki, kit padrao.
/// E o "esqueleto minimo" de um personagem novo: so a ficha, moveset da biblioteca.
/// Nao tem modelo 3D (ModelAsset = null): renderiza no boneco procedural colorido.
/// </summary>
public static class Nova
{
    public static readonly CharacterDefinition Def = new()
    {
        Name = "NOVA",
        Tagline = "PSYCHIC ACROBAT",
        ModelAsset = null,                 // sem FBX: boneco procedural
        LightLabel = "PSI COMBO",
        HeavyLabel = "FORCE PUSH",
        Color = new Color(160, 90, 240),
        AltColor = new Color(250, 170, 60),
        Stats = new FighterStats
        {
            MaxHealth = 850f,              // vidro: menos da metade do Grimm
            MoveSpeed = 11f,               // mais rapido
            VerticalSpeed = 8f,
            CanFly = true,                 // voo livre
            HasKi = true,                  // barra de ki, carrega, ki blast
            DamageMultiplier = 0.85f,      // bate mais fraco (mas nao para de bater)
            DashDuration = 0.16f           // dash curto e nervoso
        },
        Attacks = new AttackSet
        {
            Light = AttackLibrary.Light,   // combo padrao de 3
            Heavy = AttackLibrary.Heavy,
            KiBlast = AttackLibrary.KiBlast
        }
    };
}

// ============================================================================
//  TEMPLATE DE PERSONAGEM — como criar um personagem novo:
//
//  1) Copie este arquivo como SeuPersonagem.cs (mesma pasta)
//  2) Descomente, preencha e ajuste o que quiser (tudo tem default)
//  3) Registre em Roster.All (arquivo Combat/CharacterDefinition.cs):
//         public static readonly IReadOnlyList<CharacterDefinition> All = new[]
//         {
//             Grimm.Def,
//             Nova.Def,
//             MeuPersonagem.Def   // <- adiciona esta linha
//         };
//  4) (Opcional) modelo 3D: exporte um FBX com as actions nomeadas igual ao
//     mapa padrao (veja ClipNames em CharacterDefinition.cs), salve em
//     Content/Models/SeuNome.fbx, adicione no Content.mgcb e ponha
//     ModelAsset = "Models/SeuNome". Sem FBX = boneco procedural colorido.
//
//  ----------------------------------------------------------------------------
//  using Microsoft.Xna.Framework;
//
//  namespace GrimmsVenture.Combat;
//
//  public static class MeuPersonagem
//  {
//      // (Opcional) moveset proprio — copie de Grimm.cs e mude os numeros.
//      // Sem isso, use AttackLibrary.Light / .Heavy / .KiBlast (kit padrao).
//
//      public static readonly CharacterDefinition Def = new()
//      {
//          Name = "MEU",                     // aparece na selecao e no HUD
//          Tagline = "MINHA TAGLINE",        // subtitulo na selecao
//          ModelAsset = null,                // "Models/MeuNome" se tiver FBX
//          LightLabel = "LIGHT ATTACK",      // dica de controles
//          HeavyLabel = "HEAVY ATTACK",
//          Color = new Color(255, 255, 255),      // cor do P1
//          AltColor = new Color(120, 120, 120),   // cor do P2 (mirror match)
//          Stats = new FighterStats
//          {
//              MaxHealth = 1000f,
//              CanFly = true,                // false = pulo + pulo duplo
//              HasKi = true,                 // false = sem barra/custo (estilo Grimm)
//              MoveSpeed = 9f,
//              Gravity = 26f,                // so para nao-voadores
//              JumpSpeed = 13f, MaxAirJumps = 1, AirJumpSpeed = 11f,
//              DamageMultiplier = 1f
//          },
//          Attacks = new AttackSet
//          {
//              Light = AttackLibrary.Light,     // combo de 3
//              Heavy = AttackLibrary.Heavy,      // ou null pra nao ter
//              KiBlast = AttackLibrary.KiBlast
//          },
//          // (Opcional) animacoes com nomes diferentes no FBX:
//          // Clips = new Dictionary<string, string>(ClipNames.Default)
//          // {
//          //     ["Jab"] = "Soco1"   // golpe "Jab" toca a action "Soco1"
//          // }
//      };
//  }
// ============================================================================

# Adicionando personagens

Personagem novo = **1 arquivo novo + 1 linha no Roster**. Nada mais.
A selecao de personagem, HUD, controles e luta ja entendem qualquer ficha.

## 1. A ficha (obrigatorio)

1. Copie `Grimms_Venture/Combat/Characters/Template.cs` como `SeuNome.cs`.
2. Descomente o bloco e preencha (tudo tem default razoavel).
3. Registre em `Combat/CharacterDefinition.cs`:

    public static readonly IReadOnlyList<CharacterDefinition> All = new[]
    {
        Grimm.Def,
        Nova.Def,
        MeuPersonagem.Def   // <- adiciona esta linha
    };

Pronto. Ele ja aparece na tela de selecao (setinha esquerda/direita troca).

## 2. Atributos (FighterStats)

- `MaxHealth` — vida (padrao 1000; Grimm tem 2000)
- `MoveSpeed` / `VerticalSpeed` — velocidade (9 / 7)
- `CanFly` — `true` = voo livre; `false` = pulo (+ `Gravity`, `JumpSpeed`, `MaxAirJumps`, `AirJumpSpeed`)
- `HasKi` — `false` = sem barra/custo: dash e guarda de graca (estilo Grimm)
- `DamageMultiplier` — escala TODO dano do personagem (1 = normal)
- `DashSpeed` / `DashDuration` / `DashKiCost` — dash (30 / 0.20 / 6)

## 3. Golpes (AttackSet / AttackData)

- `Light` = array (combo em cadeia, na ordem), `Heavy` e `KiBlast` = 1 golpe ou `null`.
- Cada golpe tem frame data (`Startup/Active/Recovery`), dano, knockback, lunge etc.
- **O `Name` do golpe tambem e a chave da animacao** (veja abaixo).

## 4. Animacoes (clips)

O mapa padrao (`ClipNames.Default`, em `CharacterDefinition.cs`) traduz estado/golpe
-> action do FBX: `Idle, Walk, Air, Guard, Charge, HitStun, KO` e, para golpes,
`Punch1/Punch2/Punch3/Kick` (as chaves `Jab/Straight/Cross/Kick` e `Light 1/2/3/Heavy/Ki Blast` ja apontam pra elas).

Se o teu FBX tiver actions com OUTROS nomes, sobrescreva na ficha:

    Clips = new Dictionary<string, string>(ClipNames.Default)
    {
        ["Jab"] = "Soco1"   // golpe "Jab" toca a action "Soco1"
    }

## 5. Modelo 3D (opcional)

- `ModelAsset = "Models/SeuNome"` (ou `null` = boneco procedural com `Color`/`AltColor`).
- No Blender: nomeie as actions conforme o item 4, pese o modelo no rig e exporte
  igual ao Grimm: `Selected Objects ON, Add Leaf Bones OFF, Bake Animation ON,
  All Actions ON`, salvando em `Content/Models/SeuNome.fbx`.
- Adicione no `Content.mgcb`:

    #begin Models/SeuNome.fbx
    /importer:FbxImporter
    /processor:SkinnedModelProcessor
    /build:Models/SeuNome.fbx

- Escala e tamanho sao automaticos (o renderer mede o modelo ao carregar).

## Checklist do personagem novo

- [ ] `Characters/SeuNome.cs` criado e preenchido
- [ ] `Roster.All` tem `SeuNome.Def`
- [ ] (se tiver FBX) arquivo em `Content/Models` + linha no `Content.mgcb`
- [ ] `dotnet build Grimms_Venture.slnx` sem erros
- [ ] Testado: selecao, andar, bater, tomar dano, KO

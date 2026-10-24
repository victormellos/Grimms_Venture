# Arquitetura

## Princípios
1. **Lógica separada de renderização.** `Combat/` e `Controls/` não desenham nada; `Rendering/` não decide nada.
   Isso facilita testes, port para console e (futuramente) rollback netcode.
2. **Input abstrato.** Tudo que a lógica lê é `PlayerInput` (struct). Gamepad e teclado só preenchem esse struct.
3. **Dados de golpe separados do código.** `AttackData` é um objeto puro (frame data em segundos);
   migrar para JSON é trivial.
4. **Passo fixo de 60 Hz** (`IsFixedTimeStep`), simulação determinística o suficiente para evoluir.
5. **Sem assets externos por enquanto.** Meshes e fonte são gerados em código; o Content Pipeline entra quando houver modelos reais.

## Fluxo de um frame
```
InputManager.Update()           -> PlayerInput[2] + inputs globais
ScreenManager.Update()
  VersusScreen.Update()
    Simulate(): HitStop? -> Fighter.Update x2 -> CombatSystem.Update (projéteis, corpos)
    CameraRig.Update()
ScreenManager.Draw()
  VersusScreen.Draw(): para cada viewport -> Renderer3D (opaco -> transparente); depois HUD (SpriteBatch)
```

## Fighter (máquina de estados)
`Idle, Moving, Dashing, Attacking, Guarding, Charging, HitStun, KO`

- Sempre encara o oponente (lock-on); movimento relativo a esse eixo.
- Voo livre: sem gravidade enquanto vivo; knockback via `_impulse` com arrasto exponencial.
- Input buffer de 0.18 s; dash cancela em golpe após 0.10 s; cadeia de 3 golpes leves com janela de 0.45 s.
- Hit detection: esfera do golpe (`MeleeCenter`, `Radius`) contra esfera de hurt do alvo.

## Câmeras
- **Shared:** câmera lateral que enquadra os dois (distância dinâmica, não vira de lado de repente).
- **P1/P2 (chase):** atrás do ombro de cada lutador, olhando entre ele e o oponente. Usadas na tela dividida.
- Shake por "trauma" (quadrático), disparado por hits/explosões.

## Como estender
| Quero... | Onde mexer |
|---|---|
| Novo personagem | `Combat/CharacterDefinition.cs` (Roster) |
| Novo golpe | `Combat/AttackData.cs` (AttackLibrary) + estado em `Fighter.TryStartAttack` |
| Modelo 3D real | Substituir `Rendering/FighterRenderer.cs` (mantém a mesma interface) |
| Nova arena | Novo renderer parecido com `ArenaRenderer` + limites em `GameConstants` |
| Fonte/UI bonita | Trocar `UI/Ui.cs` por `SpriteFont` (Content Pipeline) |

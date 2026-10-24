# Grimm's Venture

Arena fighter 3D com versus local (tela compartilhada ou dividida), gamepad-first.
Feito em **MonoGame (DesktopGL) + C#**. Esta é a **base do primeiro protótipo jogável**:
tudo é procedural (sem assets externos), então roda assim que o projeto compila.

## Requisitos
- .NET SDK 8.0 ou superior
- GPU com OpenGL 3.x

## Rodar
```bash
dotnet restore
dotnet run
```
Na primeira vez o NuGet baixa o `MonoGame.Framework.DesktopGL`.

## Controles

| Ação | Gamepad | Teclado P1 | Teclado P2 |
|---|---|---|---|
| Mover / strafe | Stick esq. (ou D-Pad) | W A S D | Setas |
| Subir / descer | RT / LT | E / Q | U / M |
| Golpe leve (combo de 3) | A | F | J |
| Golpe forte | X | G | K |
| Ki Blast (15 ki) | Y | H | L |
| Dash (6 ki, sem direção = vai no oponente) | B | Left Shift | Right Shift |
| Guarda (segurar) | LB | R | I |
| Carregar ki (segurar) | RB | T | O |

- **W / stick para cima = aproximar do oponente** (movimento é relativo ao lock-on).
- **Slot P1/P2:** usa o gamepad correspondente se conectado; senão, o teclado do slot.
- **Tab / Back:** alterna câmera (compartilhada → dividida vertical → dividida horizontal).
- **F1:** debug (hitboxes, estados). **F11:** tela cheia. **Esc / Start:** pausa.
- Dash cancela em golpe; golpes leves encadeiam em 3 hits; combo reduz o dano progressivamente.

## Estrutura
```
Core/        GameContext, Screen, ScreenManager, constantes
Controls/    PlayerInput + fontes (gamepad/teclado) + InputManager
Combat/      Fighter (máquina de estados), AttackData/AttackLibrary, CombatSystem, Roster
Rendering/   Mesh/MeshFactory, Renderer3D, CameraRig (shared + chase), FighterRenderer, Arena, Partículas
UI/          Ui (fonte bitmap embutida) e Hud
Screens/     TitleScreen, VersusScreen
docs/        ARCHITECTURE.md, ROADMAP.md
```
Detalhes em `docs/ARCHITECTURE.md`. Plano de evolução em `docs/ROADMAP.md`.

## Notas sobre console
O código de jogo é o mesmo; o que muda é a camada de plataforma. Os alvos de console do MonoGame
exigem aprovação nos programas de desenvolvedor de cada fabricante. Até lá, desenvolva e teste no PC com gamepad.

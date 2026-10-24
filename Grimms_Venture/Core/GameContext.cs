using GrimmsVenture.Controls;
using GrimmsVenture.Rendering;
using GrimmsVenture.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GrimmsVenture.Core;

/// <summary>Serviços compartilhados entre as telas (evita singletons globais).</summary>
public sealed class GameContext
{
    public Game Game { get; init; } = null!;
    public GraphicsDevice GraphicsDevice { get; init; } = null!;
    public SpriteBatch SpriteBatch { get; init; } = null!;
    public InputManager Input { get; init; } = null!;
    public Renderer3D Renderer { get; init; } = null!;
    public ArenaRenderer Arena { get; init; } = null!;
    public Ui Ui { get; init; } = null!;
    public Hud Hud { get; init; } = null!;
    public ScreenManager Screens { get; init; } = null!;
    public float Fps { get; set; }
}

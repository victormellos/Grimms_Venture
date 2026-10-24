using Microsoft.Xna.Framework;

namespace GrimmsVenture.Controls;

/// <summary>Estado de input de um jogador num frame, independente do dispositivo.</summary>
public struct PlayerInput
{
    /// <summary>X: strafe (direita +). Y: em direção ao oponente (+).</summary>
    public Vector2 Move;
    /// <summary>+1 subir, -1 descer.</summary>
    public float Vertical;

    public bool DashPressed;
    public bool LightPressed;
    public bool HeavyPressed;
    public bool KiBlastPressed;
    public bool GuardHeld;
    public bool ChargeHeld;
}

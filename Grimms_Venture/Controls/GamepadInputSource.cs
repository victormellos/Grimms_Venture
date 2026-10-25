using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GrimmsVenture.Controls;

/// <summary>
/// Mapeamento: Stick esq = mover | RT/LT = subir/descer | A = golpe leve | X = golpe forte
/// Y = ki blast | B = dash | LB = guarda | RB = carregar ki
/// </summary>
public sealed class GamepadInputSource : IInputSource
{
    static readonly Buttons[] Watched =
    {
        Buttons.A, Buttons.B, Buttons.X, Buttons.Y,
        Buttons.Start, Buttons.Back, Buttons.BigButton,
        Buttons.LeftShoulder, Buttons.RightShoulder,
        Buttons.LeftStick, Buttons.RightStick,
        Buttons.DPadUp, Buttons.DPadDown, Buttons.DPadLeft, Buttons.DPadRight
    };

    readonly PlayerIndex _index;
    GamePadState _prev, _cur;

    bool _wasConnected;

    public GamepadInputSource(PlayerIndex index) => _index = index;

    /// <summary>PlayStation ou Xbox/outro (define os rotulos dos botoes na tela).</summary>
    public PadStyle Style { get; private set; } = PadStyle.Xbox;

    static PadStyle Detect(GamePadCapabilities caps)
    {
        // GUID do SDL: vendor em little-endian nos chars 8..11 (Sony = 0x054C -> "4c05")
        string id = caps.Identifier ?? "";
        if (id.Length >= 12 && id.Substring(8, 4).Equals("4c05", StringComparison.OrdinalIgnoreCase))
            return PadStyle.PlayStation;

        string n = (caps.DisplayName ?? "").ToLowerInvariant();
        string[] sony = { "playstation", "dualshock", "dualsense", "sony", "ps3", "ps4", "ps5", "wireless controller" };
        foreach (string t in sony)
            if (n.Contains(t)) return PadStyle.PlayStation;
        return PadStyle.Xbox;
    }

    public string Name => $"GAMEPAD {(int)_index + 1}";
    public bool IsAvailable => _cur.IsConnected;
    public bool AnyPressed { get; private set; }
    public bool JoinPressed { get; private set; }

    bool Pressed(Buttons b) => _cur.IsButtonDown(b) && _prev.IsButtonUp(b);

    public PlayerInput Poll()
    {
        _prev = _cur;
        _cur = GamePad.GetState(_index, GamePadDeadZone.Circular);

        AnyPressed = false;
        JoinPressed = false;

        var input = new PlayerInput();
        if (!_cur.IsConnected) { _wasConnected = false; return input; }
        if (!_wasConnected) { _wasConnected = true; Style = Detect(GamePad.GetCapabilities(_index)); }

        foreach (Buttons b in Watched)
            if (Pressed(b)) { AnyPressed = true; break; }
        if (!AnyPressed)
            AnyPressed = (_cur.Triggers.Right > 0.5f && _prev.Triggers.Right <= 0.5f)
                      || (_cur.Triggers.Left > 0.5f && _prev.Triggers.Left <= 0.5f);

        JoinPressed = Pressed(Buttons.Start);

        Vector2 move = _cur.ThumbSticks.Left;
        if (_cur.DPad.Left == ButtonState.Pressed) move.X -= 1f;
        if (_cur.DPad.Right == ButtonState.Pressed) move.X += 1f;
        if (_cur.DPad.Up == ButtonState.Pressed) move.Y += 1f;
        if (_cur.DPad.Down == ButtonState.Pressed) move.Y -= 1f;
        if (move.LengthSquared() > 1f) move.Normalize();

        input.Move = move;
        input.Vertical = _cur.Triggers.Right - _cur.Triggers.Left;
        input.LightPressed = Pressed(Buttons.A);
        input.HeavyPressed = Pressed(Buttons.X);
        input.KiBlastPressed = Pressed(Buttons.Y);
        input.DashPressed = Pressed(Buttons.B);
        input.GuardHeld = _cur.IsButtonDown(Buttons.LeftShoulder);
        input.ChargeHeld = _cur.IsButtonDown(Buttons.RightShoulder);
        return input;
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GrimmsVenture.Controls;

/// <summary>
/// Mapeamento: Stick esq = mover | RT/LT = subir/descer | A = golpe leve | X = golpe forte
/// Y = ki blast | B = dash | LB = guarda | RB = carregar ki
/// </summary>
public sealed class GamepadInputSource : IInputSource
{
    readonly PlayerIndex _index;
    GamePadState _prev, _cur;

    public GamepadInputSource(PlayerIndex index) => _index = index;

    public string Name => $"GAMEPAD {(int)_index + 1}";
    public bool IsAvailable => _cur.IsConnected;

    bool Pressed(Buttons b) => _cur.IsButtonDown(b) && _prev.IsButtonUp(b);

    public PlayerInput Poll()
    {
        _prev = _cur;
        _cur = GamePad.GetState(_index, GamePadDeadZone.Circular);

        var input = new PlayerInput();
        if (!_cur.IsConnected) return input;

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

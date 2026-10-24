using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GrimmsVenture.Controls;

/// <summary>
/// Cada slot (P1/P2) usa o gamepad correspondente se estiver conectado;
/// caso contrário cai para o esquema de teclado do slot.
/// Também expõe inputs globais (menus, pausa, troca de câmera, debug).
/// </summary>
public sealed class InputManager
{
    readonly GamepadInputSource[] _gamepads =
    {
        new(PlayerIndex.One),
        new(PlayerIndex.Two)
    };

    readonly KeyboardInputSource[] _keyboards =
    {
        new(KeyboardMap.Player1(), "KEYBOARD P1"),
        new(KeyboardMap.Player2(), "KEYBOARD P2")
    };

    KeyboardState _kbPrev, _kbCur;
    readonly GamePadState[] _padPrev = new GamePadState[4];
    readonly GamePadState[] _padCur = new GamePadState[4];

    public PlayerInput[] Players { get; } = new PlayerInput[2];
    public string[] DeviceNames { get; } = { "", "" };

    public bool MenuUp { get; private set; }
    public bool MenuDown { get; private set; }
    public bool MenuConfirm { get; private set; }
    public bool MenuBack { get; private set; }
    public bool EscapePressed { get; private set; }
    public bool PauseRequested { get; private set; }
    public bool ToggleViewPressed { get; private set; }
    public bool DebugPressed { get; private set; }
    public bool FullscreenPressed { get; private set; }

    public void Update()
    {
        for (int i = 0; i < 2; i++)
        {
            PlayerInput pad = _gamepads[i].Poll();
            PlayerInput key = _keyboards[i].Poll();
            bool usePad = _gamepads[i].IsAvailable;
            Players[i] = usePad ? pad : key;
            DeviceNames[i] = usePad ? _gamepads[i].Name : _keyboards[i].Name;
        }

        UpdateGlobal();
    }

    void UpdateGlobal()
    {
        _kbPrev = _kbCur;
        _kbCur = Keyboard.GetState();

        bool KeyPressed(Keys k) => _kbCur.IsKeyDown(k) && _kbPrev.IsKeyUp(k);

        MenuUp = KeyPressed(Keys.Up) || KeyPressed(Keys.W);
        MenuDown = KeyPressed(Keys.Down) || KeyPressed(Keys.S);
        MenuConfirm = KeyPressed(Keys.Enter) || KeyPressed(Keys.Space);
        MenuBack = KeyPressed(Keys.Back);
        EscapePressed = KeyPressed(Keys.Escape);
        PauseRequested = EscapePressed;
        ToggleViewPressed = KeyPressed(Keys.Tab);
        DebugPressed = KeyPressed(Keys.F1);
        FullscreenPressed = KeyPressed(Keys.F11);

        for (int i = 0; i < 4; i++)
        {
            _padPrev[i] = _padCur[i];
            _padCur[i] = GamePad.GetState((PlayerIndex)i);

            GamePadState c = _padCur[i];
            GamePadState p = _padPrev[i];
            if (!c.IsConnected) continue;

            bool BtnPressed(Buttons b) => c.IsButtonDown(b) && p.IsButtonUp(b);

            MenuUp |= BtnPressed(Buttons.DPadUp) || (c.ThumbSticks.Left.Y > 0.6f && p.ThumbSticks.Left.Y <= 0.6f);
            MenuDown |= BtnPressed(Buttons.DPadDown) || (c.ThumbSticks.Left.Y < -0.6f && p.ThumbSticks.Left.Y >= -0.6f);
            MenuConfirm |= BtnPressed(Buttons.A) || BtnPressed(Buttons.Start);
            MenuBack |= BtnPressed(Buttons.B);
            PauseRequested |= BtnPressed(Buttons.Start);
            ToggleViewPressed |= BtnPressed(Buttons.Back);
        }
    }
}

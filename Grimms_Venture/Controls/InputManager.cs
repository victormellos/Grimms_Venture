using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GrimmsVenture.Controls;

/// <summary>
/// Sistema de "join" local: os slots P1/P2 começam vazios.
/// P1 é o primeiro dispositivo (teclado ou gamepad) a apertar QUALQUER botão;
/// P2 entra com START no gamepad (ou tecla do mapa P2 no teclado).
/// Dispositivos: 0 = KEYBOARD P1 | 1 = KEYBOARD P2 | 2..5 = GAMEPAD 1..4.
/// Também expõe inputs globais (menus, pausa, troca de câmera, debug).
/// </summary>
public sealed class InputManager
{
    readonly IInputSource[] _devices;
    readonly PlayerInput[] _deviceInput = new PlayerInput[6];
    readonly int[] _claimed = { -1, -1 }; // dispositivo de cada slot; -1 = ninguém entrou

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

    public InputManager()
    {
        _devices = new IInputSource[]
        {
            new KeyboardInputSource(KeyboardMap.Player1(), "KEYBOARD P1"),
            new KeyboardInputSource(KeyboardMap.Player2(), "KEYBOARD P2"),
            new GamepadInputSource(PlayerIndex.One),
            new GamepadInputSource(PlayerIndex.Two),
            new GamepadInputSource(PlayerIndex.Three),
            new GamepadInputSource(PlayerIndex.Four)
        };
    }

    public bool SlotJoined(int slot) => _claimed[slot] >= 0;

    /// <summary>Dispositivo do slot (teclado + mapa, ou gamepad + estilo) para mostrar os controles certos.</summary>
    public DeviceInfo GetDevice(int slot)
    {
        int d = _claimed[slot];
        if (d < 0) return default;
        return _devices[d] switch
        {
            KeyboardInputSource k => new DeviceInfo(DeviceKind.Keyboard, PadStyle.Xbox, k.Map),
            GamepadInputSource g => new DeviceInfo(DeviceKind.Gamepad, g.Style, null),
            _ => default
        };
    }

    /// <summary>P1 (slot 0) entra com qualquer botão; P2 (slot 1) entra com START.</summary>
    public bool TryJoin(int slot)
    {
        for (int d = 0; d < _devices.Length; d++)
        {
            if (d == _claimed[0] || d == _claimed[1]) continue; // dispositivo já ocupado
            IInputSource dev = _devices[d];
            if (!dev.IsAvailable) continue;
            if (slot == 0 ? !dev.AnyPressed : !dev.JoinPressed) continue;

            _claimed[slot] = d;
            return true;
        }
        return false;
    }

    /// <summary>Volta ao estado "ninguém entrou" (ao voltar ao menu principal).</summary>
    public void ResetJoins()
    {
        _claimed[0] = _claimed[1] = -1;
    }

    public void Update()
    {
        for (int d = 0; d < _devices.Length; d++)
            _deviceInput[d] = _devices[d].Poll();

        for (int i = 0; i < 2; i++)
        {
            int d = _claimed[i];
            if (d >= 0)
            {
                Players[i] = _deviceInput[d];
                DeviceNames[i] = _devices[d].Name;
            }
            else
            {
                Players[i] = default;
                DeviceNames[i] = "";
            }
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
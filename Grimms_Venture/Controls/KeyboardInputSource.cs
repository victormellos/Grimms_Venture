using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GrimmsVenture.Controls;

public sealed class KeyboardMap
{
    public Keys Left, Right, Forward, Back, Up, Down;
    public Keys Light, Heavy, KiBlast, Dash, Guard, Charge;

    // P1 usa o lado esquerdo do teclado
    public static KeyboardMap Player1() => new()
    {
        Left = Keys.A, Right = Keys.D, Forward = Keys.W, Back = Keys.S,
        Up = Keys.E, Down = Keys.Q,
        Light = Keys.F, Heavy = Keys.G, KiBlast = Keys.H,
        Dash = Keys.LeftShift, Guard = Keys.R, Charge = Keys.T
    };

    // P2 usa setas + lado direito
    public static KeyboardMap Player2() => new()
    {
        Left = Keys.Left, Right = Keys.Right, Forward = Keys.Up, Back = Keys.Down,
        Up = Keys.U, Down = Keys.M,
        Light = Keys.J, Heavy = Keys.K, KiBlast = Keys.L,
        Dash = Keys.RightShift, Guard = Keys.I, Charge = Keys.O
    };
}

public sealed class KeyboardInputSource : IInputSource
{
    readonly KeyboardMap _map;
    readonly string _name;
    KeyboardState _prev, _cur;

    public KeyboardInputSource(KeyboardMap map, string name)
    {
        _map = map;
        _name = name;
    }

    public string Name => _name;
    public bool IsAvailable => true;
    public bool AnyPressed { get; private set; }
    public bool JoinPressed => AnyPressed; // no teclado não existe START: qualquer tecla do mapa entra

    bool Pressed(Keys k) => _cur.IsKeyDown(k) && _prev.IsKeyUp(k);
    bool Down(Keys k) => _cur.IsKeyDown(k);

    public PlayerInput Poll()
    {
        _prev = _cur;
        _cur = Keyboard.GetState();

        AnyPressed = Pressed(_map.Left) || Pressed(_map.Right)
                  || Pressed(_map.Forward) || Pressed(_map.Back)
                  || Pressed(_map.Up) || Pressed(_map.Down)
                  || Pressed(_map.Light) || Pressed(_map.Heavy) || Pressed(_map.KiBlast)
                  || Pressed(_map.Dash) || Pressed(_map.Guard) || Pressed(_map.Charge);

        Vector2 move = Vector2.Zero;
        if (Down(_map.Left)) move.X -= 1f;
        if (Down(_map.Right)) move.X += 1f;
        if (Down(_map.Forward)) move.Y += 1f;
        if (Down(_map.Back)) move.Y -= 1f;
        if (move.LengthSquared() > 1f) move.Normalize();

        float vertical = 0f;
        if (Down(_map.Up)) vertical += 1f;
        if (Down(_map.Down)) vertical -= 1f;

        return new PlayerInput
        {
            Move = move,
            Vertical = vertical,
            LightPressed = Pressed(_map.Light),
            HeavyPressed = Pressed(_map.Heavy),
            KiBlastPressed = Pressed(_map.KiBlast),
            DashPressed = Pressed(_map.Dash),
            GuardHeld = Down(_map.Guard),
            ChargeHeld = Down(_map.Charge)
        };
    }
}
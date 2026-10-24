using Microsoft.Xna.Framework;

namespace GrimmsVenture.Core;

public sealed class ScreenManager
{
    Screen? _current;

    public void Set(Screen screen)
    {
        _current = screen;
        screen.Enter();
    }

    public void Update(GameTime gt) => _current?.Update(gt);
    public void Draw(GameTime gt) => _current?.Draw(gt);
}

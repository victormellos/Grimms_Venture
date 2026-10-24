using Microsoft.Xna.Framework;

namespace GrimmsVenture.Core;

public abstract class Screen
{
    protected GameContext Ctx { get; }
    protected Screen(GameContext ctx) => Ctx = ctx;

    public virtual void Enter() { }
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(GameTime gameTime);
}

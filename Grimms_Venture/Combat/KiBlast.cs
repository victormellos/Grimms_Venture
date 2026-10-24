using Microsoft.Xna.Framework;

namespace GrimmsVenture.Combat;

public sealed class KiBlastProjectile
{
    public Fighter Owner = null!;
    public AttackData Data = null!;
    public Vector3 Position;
    public Vector3 Velocity;
    public float Radius;
    public float Life = 3f;
}

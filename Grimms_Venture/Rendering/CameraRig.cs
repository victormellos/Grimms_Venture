using GrimmsVenture.Combat;
using Microsoft.Xna.Framework;

namespace GrimmsVenture.Rendering;

/// <summary>
/// Mantém 3 câmeras: Shared (uma câmera lateral que enquadra os dois) e
/// P1/P2 (câmeras de perseguição com lock-on, usadas na tela dividida).
/// </summary>
public sealed class CameraRig
{
    public Camera Shared { get; } = new();
    public Camera P1 { get; } = new();
    public Camera P2 { get; } = new();

    Vector3 _lastSide = Vector3.Backward;
    bool _snap = true;

    public void Snap() => _snap = true;

    public void AddShake(float amount)
    {
        Shared.AddShake(amount);
        P1.AddShake(amount);
        P2.AddShake(amount);
    }

    public void Update(float dt, Fighter f1, Fighter f2)
    {
        UpdateShared(dt, f1, f2);
        UpdateChase(P1, dt, f1, f2);
        UpdateChase(P2, dt, f2, f1);
        _snap = false;

        Shared.Update(dt);
        P1.Update(dt);
        P2.Update(dt);
    }

    float Smooth(float dt, float rate) => _snap ? 1f : 1f - MathF.Exp(-rate * dt);

    void UpdateShared(float dt, Fighter f1, Fighter f2)
    {
        Vector3 a = f1.HurtCenter, b = f2.HurtCenter;
        Vector3 mid = (a + b) * 0.5f;

        Vector3 axis = b - a;
        axis.Y = 0f;
        float sep = axis.Length();
        axis = sep > 0.01f ? axis / sep : Vector3.Right;

        Vector3 side = Vector3.Normalize(Vector3.Cross(axis, Vector3.Up));
        if (Vector3.Dot(side, _lastSide) < 0f) side = -side; // evita virar de lado de repente
        _lastSide = side;

        float dist = MathHelper.Clamp(9f + sep * 0.9f + MathF.Abs(a.Y - b.Y) * 0.5f, 12f, 45f);
        Vector3 desiredPos = mid + side * dist + Vector3.Up * (3f + dist * 0.15f);
        desiredPos.Y = MathF.Max(desiredPos.Y, 1.2f);

        float k = Smooth(dt, 6f);
        Shared.Position = Vector3.Lerp(Shared.Position, desiredPos, k);
        Shared.Target = Vector3.Lerp(Shared.Target, mid, k);
    }

    void UpdateChase(Camera cam, float dt, Fighter self, Fighter foe)
    {
        Vector3 toFoe = foe.HurtCenter - self.HurtCenter;
        float flat = new Vector2(toFoe.X, toFoe.Z).Length();

        float dist = MathHelper.Clamp(5f + flat * 0.25f, 5f, 11f);
        Vector3 desiredPos = self.Position - self.Forward * dist + self.Right * 1.1f + Vector3.Up * 2.4f;
        desiredPos.Y = MathF.Max(desiredPos.Y, 1f);

        Vector3 desiredTarget = Vector3.Lerp(self.HurtCenter, foe.HurtCenter, 0.55f);

        float k = Smooth(dt, 8f);
        cam.Position = Vector3.Lerp(cam.Position, desiredPos, k);
        cam.Target = Vector3.Lerp(cam.Target, desiredTarget, k);
    }
}

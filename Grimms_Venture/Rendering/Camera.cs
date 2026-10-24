using Microsoft.Xna.Framework;

namespace GrimmsVenture.Rendering;

public sealed class Camera
{
    readonly Random _rng = new(1337);
    float _trauma;

    public Vector3 Position = new(0, 8, 20);
    public Vector3 Target = new(0, 2, 0);
    public float FovDegrees = 60f;
    public float Near = 0.1f;
    public float Far = 400f;

    public Matrix View { get; private set; } = Matrix.Identity;

    public void AddShake(float amount) => _trauma = MathF.Min(1f, _trauma + amount);

    public void Update(float dt)
    {
        _trauma = MathF.Max(0f, _trauma - dt * 1.8f);
        Vector3 shake = Vector3.Zero;
        if (_trauma > 0f)
        {
            float m = _trauma * _trauma * 0.8f;
            shake = new Vector3(R(), R(), R()) * m;
        }
        View = Matrix.CreateLookAt(Position + shake, Target + shake, Vector3.Up);
    }

    public Matrix GetProjection(float aspect)
        => Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FovDegrees), aspect, Near, Far);

    float R() => (float)(_rng.NextDouble() * 2.0 - 1.0);
}

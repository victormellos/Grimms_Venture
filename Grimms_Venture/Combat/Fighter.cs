using GrimmsVenture.Controls;
using GrimmsVenture.Core;
using Microsoft.Xna.Framework;

namespace GrimmsVenture.Combat;

public enum FighterState { Idle, Moving, Dashing, Attacking, Guarding, Charging, HitStun, KO }

/// <summary>
/// Lutador: máquina de estados + física simples. Sem dependência de renderização.
/// Posição = pés. Sempre encara o oponente (lock-on estilo arena 3D).
/// </summary>
public sealed class Fighter
{
    const float BufferWindow = 0.18f;
    const float ChainWindow = 0.45f;

    public int PlayerIndex { get; }
    public CharacterDefinition Character { get; }
    public FighterStats Stats => Character.Stats;
    public string Name => Character.Name;
    public Color Color { get; }

    public Vector3 Position;
    public Vector3 Forward { get; private set; }
    public Vector3 Right => Vector3.Cross(Forward, Vector3.Up);
    public Vector3 HurtCenter => Position + Vector3.Up * 1.1f;
    public float HurtRadius => 0.85f;
    public bool IsAirborne => Position.Y > 0.15f;

    public float Health { get; private set; }
    public float Ki { get; private set; }
    public FighterState State { get; private set; } = FighterState.Idle;
    public float StateTime { get; private set; }
    public float Time { get; private set; }
    public AttackData? CurrentAttack { get; private set; }
    public Vector3 MeleeCenter { get; private set; }
    public int ComboCount { get; private set; }
    public float HitFlash { get; private set; }

    public bool IsAttackActive =>
        CurrentAttack is { IsProjectile: false } a &&
        StateTime >= a.Startup && StateTime < a.Startup + a.Active;

    public float ComboScale => MathF.Max(0.4f, MathF.Pow(0.92f, ComboCount));

    Vector3 _moveVel, _impulse, _desiredMove, _dashDir;
    float _hitStunDuration;
    float _lightBuf, _heavyBuf, _kiBuf, _dashBuf;
    float _chainTimer, _comboTimer;
    int _nextLight;
    bool _attackHit, _projectileSpawned;

    public Fighter(int playerIndex, CharacterDefinition character, Vector3 spawn, Vector3 facing, Color? color = null)
    {
        PlayerIndex = playerIndex;
        Character = character;
        Color = color ?? character.Color;
        Position = spawn;
        Forward = SafeNormalize(new Vector3(facing.X, 0f, facing.Z), Vector3.Forward);
        Health = Stats.MaxHealth;
        Ki = Stats.MaxKi * 0.5f;
    }

    // ------------------------------------------------------------------ update

    public void Update(float dt, in PlayerInput input, Fighter foe, CombatSystem combat)
    {
        Time += dt;
        StateTime += dt;
        HitFlash = MathF.Max(0f, HitFlash - dt);
        TickTimers(dt, input);
        _desiredMove = Vector3.Zero;

        if (State != FighterState.KO) FaceOpponent(foe);

        switch (State)
        {
            case FighterState.KO:
                _desiredMove = Vector3.Down * 14f;
                break;
            case FighterState.HitStun:
                if (StateTime >= _hitStunDuration) SetState(FighterState.Idle);
                break;
            case FighterState.Attacking:
                UpdateAttack(foe, combat);
                break;
            case FighterState.Dashing:
                UpdateDash(foe);
                break;
            case FighterState.Guarding:
                UpdateGuard(dt, input);
                break;
            case FighterState.Charging:
                UpdateCharge(dt, input);
                break;
            default:
                UpdateNeutral(dt, input, foe);
                break;
        }

        Integrate(dt);
    }

    void TickTimers(float dt, in PlayerInput input)
    {
        _lightBuf = input.LightPressed ? BufferWindow : MathF.Max(0f, _lightBuf - dt);
        _heavyBuf = input.HeavyPressed ? BufferWindow : MathF.Max(0f, _heavyBuf - dt);
        _kiBuf = input.KiBlastPressed ? BufferWindow : MathF.Max(0f, _kiBuf - dt);
        _dashBuf = input.DashPressed ? BufferWindow : MathF.Max(0f, _dashBuf - dt);

        _chainTimer = MathF.Max(0f, _chainTimer - dt);
        if (_chainTimer <= 0f && State != FighterState.Attacking) _nextLight = 0;

        if (_comboTimer > 0f)
        {
            _comboTimer -= dt;
            if (_comboTimer <= 0f) ComboCount = 0;
        }
    }

    void FaceOpponent(Fighter foe)
    {
        Vector3 d = foe.Position - Position;
        d.Y = 0f;
        if (d.LengthSquared() > 0.0004f) Forward = Vector3.Normalize(d);
    }

    // ------------------------------------------------------------------ states

    void UpdateNeutral(float dt, in PlayerInput input, Fighter foe)
    {
        AddKi(Stats.PassiveKiRegen * dt);

        if (input.GuardHeld && Ki > 0f) { SetState(FighterState.Guarding); return; }
        if (input.ChargeHeld && Ki < Stats.MaxKi) { SetState(FighterState.Charging); return; }
        if (TryStartAttack()) return;
        if (_dashBuf > 0f && Ki >= Stats.DashKiCost)
        {
            _dashBuf = 0f;
            StartDash(input, foe);
            return;
        }

        Vector3 dir = Forward * input.Move.Y + Right * input.Move.X;
        if (dir.LengthSquared() > 1f) dir.Normalize();

        float speed = Stats.MoveSpeed * (IsAirborne ? 1f : 0.85f);
        _desiredMove = dir * speed + Vector3.Up * (input.Vertical * Stats.VerticalSpeed);

        SetState(_desiredMove.LengthSquared() > 0.04f ? FighterState.Moving : FighterState.Idle);
    }

    void StartDash(in PlayerInput input, Fighter foe)
    {
        AddKi(-Stats.DashKiCost);
        Vector3 stick = Forward * input.Move.Y + Right * input.Move.X + Vector3.Up * input.Vertical;
        _dashDir = stick.LengthSquared() > 0.09f
            ? Vector3.Normalize(stick)
            : SafeNormalize(foe.HurtCenter - HurtCenter, Forward); // sem input: avança no oponente
        SetState(FighterState.Dashing);
    }

    void UpdateDash(Fighter foe)
    {
        float t = MathHelper.Clamp(StateTime / Stats.DashDuration, 0f, 1f);
        _impulse = _dashDir * Stats.DashSpeed * (1f - 0.5f * t);

        Vector3 toFoe = foe.HurtCenter - HurtCenter;
        bool arrived = toFoe.Length() < 1.9f && Vector3.Dot(_dashDir, SafeNormalize(toFoe, Forward)) > 0.5f;

        if (StateTime >= Stats.DashCancelTime && TryStartAttack()) return; // dash cancel -> golpe

        if (StateTime >= Stats.DashDuration || arrived)
        {
            _impulse *= 0.35f;
            SetState(FighterState.Idle);
        }
    }

    bool TryStartAttack()
    {
        if (_heavyBuf > 0f) { _heavyBuf = 0f; BeginAttack(AttackLibrary.Heavy); return true; }
        if (_lightBuf > 0f) { _lightBuf = 0f; BeginAttack(AttackLibrary.Light[_nextLight]); return true; }
        if (_kiBuf > 0f && Ki >= AttackLibrary.KiBlast.KiCost)
        {
            _kiBuf = 0f;
            BeginAttack(AttackLibrary.KiBlast);
            return true;
        }
        return false;
    }

    void BeginAttack(AttackData attack)
    {
        AddKi(-attack.KiCost);
        CurrentAttack = attack;
        _attackHit = false;
        _projectileSpawned = false;
        SetState(FighterState.Attacking, force: true);
    }

    void UpdateAttack(Fighter foe, CombatSystem combat)
    {
        AttackData a = CurrentAttack!;
        Vector3 toFoe = foe.HurtCenter - HurtCenter;
        float dist = toFoe.Length();
        Vector3 aim = SafeNormalize(toFoe, Forward);
        MeleeCenter = HurtCenter + aim * a.Range;

        // lunge: o golpe "puxa" o lutador até o alcance (inclui eixo vertical)
        bool committing = StateTime < a.Startup + a.Active;
        if (committing && a.LungeSpeed > 0f && dist > 1.6f)
            _desiredMove = aim * a.LungeSpeed;

        if (a.IsProjectile)
        {
            if (!_projectileSpawned && StateTime >= a.Startup)
            {
                _projectileSpawned = true;
                combat.SpawnKiBlast(this, foe, a);
            }
        }
        else if (!_attackHit && StateTime >= a.Startup && StateTime < a.Startup + a.Active)
        {
            if (combat.TryMelee(this, foe, a, MeleeCenter)) _attackHit = true;
        }

        if (StateTime >= a.Total) FinishAttack(a);
    }

    void FinishAttack(AttackData a)
    {
        if (a.Kind == AttackKind.Light)
        {
            _nextLight = (a.LightIndex + 1) % 3;
            _chainTimer = ChainWindow;
        }
        else
        {
            _nextLight = 0;
        }
        CurrentAttack = null;
        SetState(FighterState.Idle);
    }

    void UpdateGuard(float dt, in PlayerInput input)
    {
        AddKi(-Stats.GuardKiDrain * dt);
        if (!input.GuardHeld || Ki <= 0f) SetState(FighterState.Idle);
    }

    void UpdateCharge(float dt, in PlayerInput input)
    {
        AddKi(Stats.ChargeRate * dt);
        if (!input.ChargeHeld || Ki >= Stats.MaxKi) SetState(FighterState.Idle);
    }

    // ------------------------------------------------------------------ physics

    void Integrate(float dt)
    {
        float k = 1f - MathF.Exp(-Stats.Acceleration * dt);
        _moveVel = Vector3.Lerp(_moveVel, _desiredMove, k);
        _impulse *= MathF.Exp(-Stats.ImpulseDrag * dt);
        Position += (_moveVel + _impulse) * dt;

        float r = MathF.Sqrt(Position.X * Position.X + Position.Z * Position.Z);
        if (r > GameConstants.ArenaRadius)
        {
            float s = GameConstants.ArenaRadius / r;
            Position.X *= s;
            Position.Z *= s;
        }

        if (Position.Y <= GameConstants.GroundY)
        {
            Position.Y = GameConstants.GroundY;
            if (_impulse.Y < 0f) _impulse.Y = 0f;
            if (_moveVel.Y < 0f) _moveVel.Y = 0f;
        }
        if (Position.Y > GameConstants.MaxHeight) Position.Y = GameConstants.MaxHeight;
    }

    // ------------------------------------------------------------------ combat API

    public void ReceiveHit(float damage, Vector3 knockback, float hitStun)
    {
        Health = MathF.Max(0f, Health - damage);
        _impulse = knockback;
        _moveVel = Vector3.Zero;
        CurrentAttack = null;
        HitFlash = 0.15f;

        if (Health <= 0f)
        {
            _impulse *= 1.5f;
            SetState(FighterState.KO, force: true);
            return;
        }

        _hitStunDuration = hitStun;
        SetState(FighterState.HitStun, force: true);
    }

    public void ReceiveBlock(float chipDamage, Vector3 knockback, float kiDrain)
    {
        Health = MathF.Max(1f, Health - chipDamage); // dano de guarda não mata
        _impulse = knockback;
        AddKi(-kiDrain);
        HitFlash = 0.05f;

        if (Ki <= 0f) // guard break
        {
            _hitStunDuration = 0.8f;
            SetState(FighterState.HitStun, force: true);
        }
    }

    public void RegisterHit(float hitStun)
    {
        ComboCount++;
        _comboTimer = hitStun + 0.6f;
    }

    public void AddKi(float amount) => Ki = MathHelper.Clamp(Ki + amount, 0f, Stats.MaxKi);

    // ------------------------------------------------------------------ helpers

    void SetState(FighterState s, bool force = false)
    {
        if (State == s && !force) return;
        State = s;
        StateTime = 0f;
    }

    static Vector3 SafeNormalize(Vector3 v, Vector3 fallback)
        => v.LengthSquared() > 0.000001f ? Vector3.Normalize(v) : fallback;
}

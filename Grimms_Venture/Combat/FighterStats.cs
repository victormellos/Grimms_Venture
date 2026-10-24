namespace GrimmsVenture.Combat;

/// <summary>Atributos de um lutador. Valores em unidades de mundo (metros) e segundos.</summary>
public sealed class FighterStats
{
    public float MaxHealth = 1000f;
    public float MaxKi = 100f;

    public float MoveSpeed = 9f;
    public float VerticalSpeed = 7f;   // só usado por personagens que VOAM
    public float Acceleration = 10f;
    public float ImpulseDrag = 7f;

    public float DashSpeed = 30f;
    public float DashDuration = 0.20f;
    public float DashCancelTime = 0.10f;
    public float DashKiCost = 6f;

    public float GuardKiDrain = 6f;
    public float BlockKiDrain = 8f;
    public float BlockChipFraction = 0.08f;

    public float ChargeRate = 35f;
    public float PassiveKiRegen = 2f;

    public float DamageMultiplier = 1f;

    // ---------------- capacidade de movimento vertical ----------------
    /// <summary>true = voa livre (input vertical move pra cima/baixo). false = pula.</summary>
    public bool CanFly = true;

    /// <summary>Gravidade dos não-voadores (m/s^2).</summary>
    public float Gravity = 26f;

    /// <summary>Impulso do pulo do chão. "Grandes pulos".</summary>
    public float JumpSpeed = 13f;

    /// <summary>Quantos pulos extras são permitidos no ar (pulo duplo = 1).</summary>
    public int MaxAirJumps = 1;

    /// <summary>Impulso do pulo aéreo.</summary>
    public float AirJumpSpeed = 11f;

    // ---------------- recursos ----------------
    /// <summary>false = personagem não usa ki NUNCA (sem barra, sem custo, sem carregar).</summary>
    public bool HasKi = true;
}
namespace GrimmsVenture.Combat;

/// <summary>Atributos de um lutador. Valores em unidades de mundo (metros) e segundos.</summary>
public sealed class FighterStats
{
    public float MaxHealth = 1000f;
    public float MaxKi = 100f;

    public float MoveSpeed = 9f;
    public float VerticalSpeed = 7f;
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
}

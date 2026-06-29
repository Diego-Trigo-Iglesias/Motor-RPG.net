using TextRPG.Core.Models;

namespace TextRPG.Core.Engine;

/// <summary>Resultado de una ronda de combate con soporte para behaviors y efectos.</summary>
public record CombatRound(
    int PlayerDamage,
    int EnemyDamage,
    bool PlayerDodged,
    bool EnemyDodged,
    bool IsCritical,
    int PoisonDamage = 0,
    bool ShieldActive = false,
    int BossChargeCounter = 0,
    bool BossIsUnleashing = false,
    string BehaviorMessage = ""
)
{
    /// <summary>Si el jugador usó Defensa este turno.</summary>
    public bool PlayerDefended { get; init; }
    /// <summary>Si el jugador usó Ataque Fuerte.</summary>
    public bool WasHeavyAttack { get; init; }
    /// <summary>Si el enemigo está enfurecido (Berserker/Boss).</summary>
    public bool EnemyIsEnraged { get; init; }
    /// <summary>Indicador visual para mostrar en UI.</summary>
    public string EnemyBehaviorIcon { get; init; } = "";
}

namespace TextRPG.Core.Engine;

public record CombatRound(
    int PlayerDamage,
    int EnemyDamage,
    bool PlayerDodged,
    bool EnemyDodged,
    bool IsCritical
);

namespace TextRPG.Core.Engine;

public record CombatResult(
    bool PlayerWon,
    int ExpGained,
    int GoldGained,
    int RoundsPlayed
);


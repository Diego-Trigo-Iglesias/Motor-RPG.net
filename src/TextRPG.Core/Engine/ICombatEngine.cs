using TextRPG.Core.Models;

namespace TextRPG.Core.Engine;

public interface ICombatEngine
{
    CombatRound SimulateRound(Player player, Enemy enemy);
    void ApplyRound(Player player, Enemy enemy, CombatRound round);
    CombatResult SimulateFull(Player player, Enemy enemy);
}


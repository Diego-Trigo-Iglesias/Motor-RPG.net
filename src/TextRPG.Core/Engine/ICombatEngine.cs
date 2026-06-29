using TextRPG.Core.Models;

namespace TextRPG.Core.Engine;

public interface ICombatEngine
{
    /// <summary>Simula una ronda con una acción del jugador, aplicando behaviors y efectos.</summary>
    CombatRound SimulateRound(Player player, Enemy enemy, PlayerAction action);
    void ApplyRound(Player player, Enemy enemy, CombatRound round);
    CombatResult SimulateFull(Player player, Enemy enemy);
}

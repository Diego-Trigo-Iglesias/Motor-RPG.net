using TextRPG.Core.Constants;
using TextRPG.Core.Models;

namespace TextRPG.Core.Engine;

public sealed class CombatEngine
{
    private static readonly Random Rng = Random.Shared;

    public CombatRound SimulateRound(Player player, Enemy enemy)
    {
        var isCritical  = Rng.Next(100) < GameConstants.CritChance;
        var playerDodge = Rng.Next(100) < GameConstants.PlayerEvasionChance;
        var enemyDodge  = Rng.Next(100) < GameConstants.EnemyEvasionChance;

        var rawPlayerDamage = Math.Max(1, player.TotalAttack - enemy.Defense + Rng.Next(-2, 4));
        var playerDamage    = enemyDodge  ? 0 : (isCritical ? rawPlayerDamage * 2 : rawPlayerDamage);
        var enemyDamage     = playerDodge ? 0 : Math.Max(1, enemy.Attack - player.TotalDefense + Rng.Next(-2, 4));

        return new CombatRound(playerDamage, enemyDamage, playerDodge, enemyDodge, isCritical);
    }

    public void ApplyRound(Player player, Enemy enemy, CombatRound round)
    {
        enemy.CurrentHp   -= round.PlayerDamage;
        player.CurrentHp  -= round.EnemyDamage;
    }

    public CombatResult SimulateFull(Player player, Enemy enemy)
    {
        int rounds = 0;
        while (player.IsAlive && enemy.IsAlive)
        {
            var round = SimulateRound(player, enemy);
            ApplyRound(player, enemy, round);
            rounds++;
            if (rounds > GameConstants.MaxCombatRounds)
            {
                // Empate tras límite de rondas → el jugador huye sin recompensa
                return new CombatResult(false, 0, 0, rounds);
            }
        }

        return player.IsAlive
            ? new CombatResult(true,  enemy.ExpReward,  enemy.GoldReward,  rounds)
            : new CombatResult(false, enemy.ExpReward / 4, 0, rounds);
    }
}


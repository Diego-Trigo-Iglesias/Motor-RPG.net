using TextRPG.Core.Models;
using TextRPG.Core.Engine;
using TextRPG.Core.Rendering;

namespace TextRPG.Core.Services;

public sealed class ExplorationService
{
    private readonly IRenderer _renderer;
    private readonly ICombatEngine _combat;

    public ExplorationService(IRenderer renderer, ICombatEngine combat)
    {
        _renderer = renderer;
        _combat = combat;
    }

    public bool Explore(Player player, ref int totalBattles, ref int totalVictories)
    {
        var enemy = Enemy.Random(player.Level);
        totalBattles++;

        var prevLevel = player.Level;
        RunCombat(player, enemy, ref totalVictories);

        if (player.Level > prevLevel)
            _renderer.PrintLevelUp(player);

        _renderer.Pause();
        return player.IsAlive;
    }

    private void RunCombat(Player player, Enemy enemy, ref int totalVictories)
    {
        _renderer.PrintCombatHeader(player, enemy);

        int round = 1;
        while (player.IsAlive && enemy.IsAlive)
        {
            var action = _renderer.SelectMenu(
                $"Ronda {round} — Que haces?",
                new Dictionary<string, string>
                {
                    ["fight"] = $"{IconPalette.Fight} Atacar",
                    ["flee"] = $"{IconPalette.Flee} Huir"
                });

            if (action == "flee")
            {
                _renderer.MarkupLine("[yellow]Huyes del combate![/]\n");
                return;
            }

            var combatRound = _combat.SimulateRound(player, enemy);
            _combat.ApplyRound(player, enemy, combatRound);
            _renderer.PrintCombatRound(player, enemy, combatRound, round++);
        }

        if (player.IsAlive && !enemy.IsAlive)
        {
            totalVictories++;
            player.Gold += enemy.GoldReward;
            player.GainExperience(enemy.ExpReward);
            _renderer.PrintVictory(enemy, enemy.ExpReward, enemy.GoldReward);
        }
    }
}



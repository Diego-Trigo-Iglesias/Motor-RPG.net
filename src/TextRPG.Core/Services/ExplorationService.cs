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
                $"Ronda {round} — Qué haces?",
                new Dictionary<string, string>
                {
                    ["fight"] = $"{IconPalette.Fight} Atacar",
                    ["heavy"] = $"{IconPalette.Attack} Ataque fuerte",
                    ["defend"] = $"{IconPalette.Dodge} Defender",
                    ["potion"] = $"{IconPalette.Bullet} Usar poción",
                    ["flee"] = $"{IconPalette.Flee} Huir"
                });

            PlayerAction playerAction = action switch
            {
                "fight" => PlayerAction.Attack,
                "heavy" => PlayerAction.HeavyAttack,
                "defend" => PlayerAction.Defend,
                "potion" => PlayerAction.UsePotion,
                "flee" => PlayerAction.Flee,
                _ => PlayerAction.Attack
            };

            if (playerAction == PlayerAction.Flee)
            {
                _renderer.MarkupLine("[yellow]Huyes del combate![/]\n");
                return;
            }

            if (playerAction == PlayerAction.UsePotion)
            {
                var (used, msg, _) = GameActions.UsePotion(player);
                _renderer.MarkupLine(msg);
                if (used) continue; // No gasta turno
            }

            var combatRound = _combat.SimulateRound(player, enemy, playerAction);
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

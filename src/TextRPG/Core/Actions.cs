/// <summary>
/// Acciones del juego (Desktop). Delega la lógica de dominio en GameActions
/// y se encarga solo de la interacción con el renderer y menús (UI Desktop).
/// </summary>

using TextRPG.Core.Constants;
using TextRPG.Core.Enums;
using TextRPG.Core.Models;
using TextRPG.Core.Engine;
using TextRPG.Rendering;
using TextRPG.Screens;

namespace TextRPG.Core;

/// <summary>Acciones del juego (Desktop): puente entre GameActions (dominio) y la UI Raylib.
public static class Actions
{
    public static void CreatePlayer(GameRenderer r, ref GameState? gs, ref Location? loc, string name, CharacterClass cls)
    {
        gs = GameActions.CreatePlayer(name, cls);
        loc = gs.CurrentLocation;
        r.SetPlayer(gs.Player);
        r.SetLocation(gs.CurrentLocationId, loc.Name, loc.Description);
        r.AddMsg("Bienvenido, " + (gs.Player.Name) + "! Tu aventura comienza...");
        r.AddMsg("Te encuentras en la " + loc.Name + ".");
        r.ShowGame();
        Menus.ShowMain(r, gs.Player, loc);
    }

    public static void StartCombat(GameRenderer r, GameState gs, ref Enemy? enemy, CombatEngine combat)
    {
        enemy = Enemy.Random(gs.Player.Level);
        gs.TotalBattles++;
        r.CombatRound = 0;
        r.UpdateState(gs.Player, enemy);
        r.AddMsg("Te adentras en la oscuridad...");
        r.AddMsg("Un " + enemy.Name + " aparece ante ti!");
        Menus.ShowCombat(r);
    }

    public static bool ExecuteAttack(GameRenderer r, GameState gs, ref Enemy? enemy, CombatEngine combat)
    {
        if (gs.Player == null || enemy == null) return false;

        var round = combat.SimulateRound(gs.Player, enemy);
        combat.ApplyRound(gs.Player, enemy, round);
        r.PlayAtk(round);
        r.UpdateState(gs.Player, enemy);

        if (round.EnemyDodged)
            r.AddMsg(enemy.Name + " esquiva tu ataque!");
        else if (round.IsCritical)
            r.AddMsg("¡GOLPE CRÍTICO! Infliges " + round.PlayerDamage + " de daño.");
        else
            r.AddMsg("Atacas a " + enemy.Name + " por " + round.PlayerDamage + " de daño.");

        if (!enemy.IsAlive)
        {
            gs.TotalVictories++;
            gs.Player.Gold += enemy.GoldReward;
            int prevLvl = gs.Player.Level;
            r.AddMsg("¡Victoria! +" + enemy.ExpReward + " EXP  +" + enemy.GoldReward + " Oro");
            if (gs.Player.Level > prevLvl)
                r.AddMsg("¡SUBISTE AL NIVEL " + gs.Player.Level + "!");
            r.PlayVictory();
            r.UpdateState(gs.Player, null);
            r.ClearMenu();
            return true;
        }

        if (!gs.Player.IsAlive)
        {
            r.AddMsg("Has caído en combate...");
            r.ClearMenu();
            return false;
        }

        if (!round.PlayerDodged)
            r.AddMsg(enemy.Name + " te golpea por " + round.EnemyDamage + " de daño.");
        else
            r.AddMsg("Esquivas el ataque de " + enemy.Name + "!");
        r.ClearMenu();
        return false;
    }

    public static void TravelTo(GameRenderer r, ref GameState gs, ref Location loc, string destId)
    {
        var msg = GameActions.TravelTo(gs, destId);
        loc = gs.CurrentLocation;
        r.SetLocation(gs.CurrentLocationId, loc.Name, loc.Description);
        r.AddMsg(msg);
        r.UpdateState(gs.Player, null);
        Menus.ShowMain(r, gs.Player, loc);
    }

    public static void Heal(GameRenderer r, Player p)
    {
        var result = GameActions.Heal(p);
        if (!result.Success) { r.AddMsg(result.Message); return; }
        r.AddMsg(result.Message);
        r.UpdateState(p, null);
    }

    public static void UsePotion(GameRenderer r, Player p)
    {
        var (used, msg, _) = GameActions.UsePotion(p);
        r.AddMsg(msg);
        r.UpdateState(p, null);
    }

    public static bool BuyItem(GameRenderer r, Player p, int idx)
    {
        var result = GameActions.BuyItem(p, idx);
        if (!result.Success && result.Item == null) return false; // idx inválido
        r.AddMsg(result.Message);
        r.UpdateState(p, null);
        return result.Success;
    }
}

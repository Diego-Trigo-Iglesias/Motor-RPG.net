/// <summary>
/// Acciones del juego sin estado. Cada método es una operación atómica:
/// recibe el estado que necesita, lo modifica, y actualiza el renderer.
/// No contienen lógica de UI ni de input — solo lógica de dominio.
/// Esto permite testear las acciones sin necesidad de interfaz gráfica.
/// </summary>

using TextRPG.Core.Enums;
using TextRPG.Core.Models;
using TextRPG.Core.Engine;
using TextRPG.Rendering;
using TextRPG.Screens;

namespace TextRPG.Core;

/// <summary>Acciones del juego: combate, viaje, tienda, curaciÃ³n, inventario.
public static class Actions
{
    public static void CreatePlayer(GameRenderer r, ref GameState? gs, ref Location? loc, string name, CharacterClass cls)
    {
        var n = string.IsNullOrWhiteSpace(name) ? "Heroe" : name;
        gs = new GameState { Player = Player.Create(n, cls) };
        gs.CurrentLocationId = "village";
        loc = gs.CurrentLocation;
        r.SetPlayer(gs.Player);
        r.SetLocation(gs.CurrentLocationId, loc.Name, loc.Description);
        r.AddMsg("Bienvenido, " + n + "! Tu aventura comienza...");
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
            r.AddMsg("GOLPE CRITICO! Infliges " + round.PlayerDamage + " de dano.");
        else
            r.AddMsg("Atacas a " + enemy.Name + " por " + round.PlayerDamage + " de dano.");

        if (!enemy.IsAlive)
        {
            gs.TotalVictories++;
            gs.Player.Gold += enemy.GoldReward;
            int prevLvl = gs.Player.Level;
            gs.Player.GainExperience(enemy.ExpReward);
            r.AddMsg("Victoria! +" + enemy.ExpReward + " EXP  +" + enemy.GoldReward + " Oro");
            if (gs.Player.Level > prevLvl)
                r.AddMsg("SUBISTE AL NIVEL " + gs.Player.Level + "!");
            r.PlayVictory();
            r.UpdateState(gs.Player, null);
            r.ClearMenu();
            return true; // victoria
        }

        if (!gs.Player.IsAlive)
        {
            r.AddMsg("Has caido en combate...");
            r.ClearMenu();
            return false; // muerte
        }

        // Turno del enemigo
        if (!round.PlayerDodged)
            r.AddMsg(enemy.Name + " te golpea por " + round.EnemyDamage + " de dano.");
        else
            r.AddMsg("Esquivas el ataque de " + enemy.Name + "!");
        r.ClearMenu();
        return false; // combate continÃºa
    }

    public static void TravelTo(GameRenderer r, ref GameState gs, ref Location loc, string destId)
    {
        gs.CurrentLocationId = destId;
        loc = gs.CurrentLocation;
        r.SetLocation(gs.CurrentLocationId, loc.Name, loc.Description);
        r.AddMsg("Viajando a " + loc.Name + "...");
        r.UpdateState(gs.Player, null);
        Menus.ShowMain(r, gs.Player, loc);
    }

    public static void Heal(GameRenderer r, Player p)
    {
        if (p.Gold < 30) { r.AddMsg("No tienes suficiente oro."); return; }
        p.Gold -= 30;
        p.Heal(p.MaxHp);
        r.AddMsg("Descansas en la posada. HP completamente restaurado.");
        r.UpdateState(p, null);
    }

    public static bool BuyItem(GameRenderer r, Player p, int idx)
    {
        var items = new[] { ("Pocion pequena", 10), ("Pocion grande", 25),
                           ("Amuleto de fuerza", 50), ("Armadura ligera", 40) };
        if (idx < 0 || idx >= items.Length) return false;
        var (nom, precio) = items[idx];
        if (p.Gold < precio) { r.AddMsg("No tienes suficiente oro."); return false; }
        p.Gold -= precio;
        switch (idx)
        {
            case 0: p.Heal(30); break;
            case 1: p.Heal(70); break;
            case 2: p.Attack += 5; break;
            case 3: p.Defense += 3; break;
        }
        p.Inventory.Add(nom);
        r.AddMsg("Has comprado: " + nom + ".");
        r.UpdateState(p, null);
        return true;
    }
}


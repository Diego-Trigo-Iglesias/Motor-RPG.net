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
        Menus.ShowCombat(r, enemy);
    }

    /// <summary>Inicia combate de MAZMORRA. El enemigo viene dado por el piso actual.</summary>
    public static void StartDungeonCombat(GameRenderer r, GameState gs, ref Enemy? enemy, CombatEngine combat)
    {
        enemy = GameActions.CreateDungeonEnemy(gs);
        gs.TotalBattles++;
        r.CombatRound = 0;
        r.UpdateState(gs.Player, enemy);
        r.AddMsg("PISO " + gs.DungeonFloor + " — " + GameActions.GetDungeonFloorName(gs.DungeonFloor));
        r.AddMsg("Un " + enemy.Name + " bloquea tu camino!");
        if (enemy.Behavior != EnemyBehavior.Normal)
            r.AddMsg("⚠ " + GetBehaviorDescription(enemy.Behavior) + " ⚠");
        Menus.ShowCombat(r, enemy);
    }

    /// <summary>Ejecuta una acción de combate (ataque normal, fuerte, defensa, poción).</summary>
    public static bool ExecuteAction(GameRenderer r, GameState gs, ref Enemy? enemy, CombatEngine combat, PlayerAction action)
    {
        if (gs.Player == null || enemy == null) return false;

        if (action == PlayerAction.UsePotion)
        {
            var (used, msg, _) = GameActions.UsePotion(gs.Player);
            r.AddMsg(msg);
            r.UpdateState(gs.Player, null);
            Menus.ShowCombat(r, enemy);
            return false; // No termina el combate
        }

        if (action == PlayerAction.Flee)
        {
            r.AddMsg("Huyes del combate!");
            enemy = null;
            r.ClearEnemy();
            r.ClearStatusEffects(gs);
            return false;
        }

        var round = combat.SimulateRound(gs.Player, enemy, action);
        combat.ApplyRound(gs.Player, enemy, round);
        r.CombatRound++;
        r.PlayAtk(round);
        r.UpdateState(gs.Player, enemy);

        //  Mensajes de la ronda 
        string actionName = action switch
        {
            PlayerAction.HeavyAttack => "Ataque Fuerte",
            PlayerAction.Defend => "Defensa",
            _ => "Ataque"
        };

        // Mensaje de comportamiento del enemigo
        if (!string.IsNullOrEmpty(round.BehaviorMessage))
            r.AddMsg(round.BehaviorMessage);

        // Daño de veneno
        if (round.PoisonDamage > 0)
            r.AddMsg($"El veneno te hace {round.PoisonDamage} de daño. ({gs.Player.PoisonTurnsRemaining} turnos restantes)");

        // Resultado del ataque del jugador
        if (round.EnemyDodged)
            r.AddMsg(enemy.Name + " esquiva tu " + actionName + "!");
        else if (round.ShieldActive)
            r.AddMsg(actionName + " golpea el escudo! Daño reducido: " + round.PlayerDamage);
        else if (round.IsCritical)
            r.AddMsg("¡GOLPE CRÍTICO! Infliges " + round.PlayerDamage + " de daño.");
        else
            r.AddMsg(actionName + " causa " + round.PlayerDamage + " de daño a " + enemy.Name + ".");

        // Contraataque del enemigo (si sigue vivo)
        if (enemy.IsAlive && !round.PlayerDodged && round.EnemyDamage > 0)
        {
            string hitDesc = round.PlayerDefended ? " (mitigado por defensa)" : "";
            r.AddMsg(enemy.Name + " te golpea por " + round.EnemyDamage + hitDesc + ".");
        }
        else if (enemy.IsAlive && round.PlayerDodged)
        {
            r.AddMsg("Esquivas el ataque de " + enemy.Name + "!");
        }

        // Efecto de enemigo enfurecido
        if (round.EnemyIsEnraged)
            r.AddMsg("¡" + enemy.Name + " está ENFURECIDO! Su poder aumenta!");

        //  Verificar resultados 
        if (!enemy.IsAlive)
        {
            gs.TotalVictories++;
            int gold = enemy.GoldReward;
            int exp = enemy.ExpReward;
            gs.Player.Gold += gold;
            gs.Player.GainExperience(exp);
            int prevLvl = gs.Player.Level;

            r.AddMsg("");
            r.AddMsg("╔══ ¡VICTORIA! ══╗");
            r.AddMsg("+" + exp + " EXP  +" + gold + " Oro");

            // Bonus por estar en mazmorra
            if (gs.IsInDungeon && gs.DungeonFloor == GameConstants.DungeonFloorCount)
            {
                r.AddMsg("╔══ ¡DERROTASTE AL DRAGÓN ANCESTRAL! ══╗");
            }

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

        r.ClearMenu();
        Menus.ShowCombat(r, enemy);
        return false;
    }

    public static void TravelTo(GameRenderer r, ref GameState gs, ref Location loc, string destId)
    {
        var msg = GameActions.TravelTo(gs, destId);
        loc = gs.CurrentLocation;
        r.SetLocation(gs.CurrentLocationId, loc.Name, loc.Description);
        r.AddMsg(msg);
        r.UpdateState(gs.Player, null);
        if (loc.IsDungeon)
            Menus.ShowDungeonEntrance(r, gs);
        else
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

    // ══════════════════════════════════════════════════
    //   ACCIONES DE MAZMORRA
    // ══════════════════════════════════════════════════

    public static void EnterDungeon(GameRenderer r, GameState gs)
    {
        GameActions.EnterDungeon(gs);
        r.ClearMsgs();
        r.AddMsg("╔═══════════════════════════════╗");
        r.AddMsg("║   TE ADENTRAS EN LA MAZMORRA  ║");
        r.AddMsg("║   No hay vuelta atrás...       ║");
        r.AddMsg("╚═══════════════════════════════╝");
        r.AddMsg("");
        r.PlayTravel("village", "dungeon");
        r.UpdateState(gs.Player, null);
    }

    public static void AdvanceDungeonFloor(GameRenderer r, GameState gs)
    {
        bool hasNext = GameActions.AdvanceDungeonFloor(gs);
        r.ClearMsgs();
        r.AddMsg("Bajando al PISO " + gs.DungeonFloor + "...");
        r.AddMsg(GameActions.GetDungeonFloorName(gs.DungeonFloor));
        r.UpdateState(gs.Player, null);
    }

    public static void ExitDungeon(GameRenderer r, GameState gs, ref Location loc)
    {
        GameActions.ExitDungeon(gs);
        loc = gs.CurrentLocation;
        r.SetLocation(gs.CurrentLocationId, loc.Name, loc.Description);
        r.AddMsg("Escapas de la mazmorra, derrotado...");
        r.UpdateState(gs.Player, null);
    }

    /// <summary>Maneja la victoria sobre el jefe final.</summary>
    public static void WinGame(GameRenderer r, GameState gs)
    {
        GameActions.SetVictory(gs);
        r.ClearMsgs();
        r.AddMsg("╔══════════════════════════════════════╗");
        r.AddMsg("║   HAS VENCIDO AL DRAGÓN ANCESTRAL!   ║");
        r.AddMsg("║   LA MAZMORRA HA SIDO PURIFICADA.    ║");
        r.AddMsg("╚══════════════════════════════════════╝");
        r.AddMsg("");
        r.AddMsg("Estadísticas finales:");
        r.AddMsg("  Nivel alcanzado: " + gs.Player.Level);
        r.AddMsg("  Batallas libradas: " + gs.TotalBattles);
        r.AddMsg("  Victorias: " + gs.TotalVictories);
        r.AddMsg("  Clase: " + gs.Player.Class);
        r.AddMsg("");
        r.AddMsg("¡Gracias por jugar TextRPG!");
        r.ClearMenu();
    }

    // ══════════════════════════════════════════════════
    //   HELPERS
    // ══════════════════════════════════════════════════

    private static string GetBehaviorDescription(EnemyBehavior behavior) => behavior switch
    {
        EnemyBehavior.Shielded  => "Escudo mágico: reduce el daño los primeros 2 turnos",
        EnemyBehavior.Venomous  => "Ataque venenoso: aplica veneno que daña por turno",
        EnemyBehavior.Berserker => "Enfurece al 50% de HP: duplica su ataque",
        EnemyBehavior.Boss      => "JEFE FINAL: carga energía 2 turnos y desata un ataque masivo",
        _                       => ""
    };
}

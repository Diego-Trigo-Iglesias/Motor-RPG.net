/// <summary>
/// Generación de menús de texto. Cada método recibe el GameRenderer y escribe en su log
/// los mensajes y opciones. Los menús son puramente texto - no contienen lógica de juego.
/// Separación de responsabilidades: Menus solo formatea texto, Actions ejecuta, Game coordina.
/// </summary>

using TextRPG.Core;
using TextRPG.Core.Constants;
using TextRPG.Core.Models;
using TextRPG.Core.Engine;
using TextRPG.Rendering;

namespace TextRPG.Screens;

/// <summary>Generación de menús de texto para todas las pantallas del juego.
public static class Menus
{
    public static void ShowTitle(GameRenderer r)
    {
        r.ClearMsgs();
        r.AddMsg("TEXTRPG");
        r.AddMsg("RPG de Texto Premium");
        r.AddMsg("");
        r.AddMsg("[ENTER] Comenzar");
    }

    public static void ShowCreate(GameRenderer r)
    {
        r.ClearMsgs();
        r.AddMsg("Elige tu clase:");
        r.AddMsg("[1] Guerrero - Tanque y resistencia");
        r.AddMsg("[2] Mago - Alto daño, frágil");
        r.AddMsg("[3] Pícaro - Equilibrado, evasión");
    }

    public static void ShowMain(GameRenderer r, Player? p, Location? loc)
    {
        r.ClearMsgs();
        r.AddMsg("¿Qué haces, " + (p?.Name ?? "Héroe") + "?");
        var opts = new System.Collections.Generic.List<string>();
        if (loc?.HasEnemies ?? false) opts.Add("[1] Explorar (buscar enemigos)");
        opts.Add("[2] Viajar a otro lugar");
        if (loc?.HasShop ?? false) opts.Add("[3] Tienda");
        if (loc?.HasHealer ?? false) opts.Add($"[4] Curandero ({GameConstants.HealCost} oro)");
        opts.Add("[5] Inventario y stats");
        if (p != null && GameActions.HasUsablePotion(p))
            opts.Add($"[6] Usar poción ({GameActions.PotionCount(p)} en inventario)");
        opts.Add("[S] Guardar partida");
        opts.Add("[L] Cargar partida");
        r.SetMenu(opts.ToArray());
    }

    /// <summary>Menú de combate con acciones tácticas. Muestra el comportamiento del enemigo.</summary>
    public static void ShowCombat(GameRenderer r, Enemy? enemy = null)
    {
        r.ClearMsgs();
        r.AddMsg("Ronda " + (r.CombatRound + 1) + " — ¿Qué haces?");
        r.AddMsg("");

        var opts = new System.Collections.Generic.List<string>
        {
            "[1] Atacar",
            "[2] Ataque fuerte (1.5x daño, recibes contraataque)",
            "[3] Defender (-50% daño recibido, -50% daño infligido)",
        };

        var playerForPotion = r.GetPlayer();
        if (enemy != null && playerForPotion != null && GameActions.HasUsablePotion(playerForPotion))
            opts.Add($"[4] Usar poción ({GameActions.PotionCount(playerForPotion)} disp.)");

        opts.Add("[5] Huir");

        r.SetMenu(opts.ToArray());
    }

    public static void ShowTravel(GameRenderer r, GameState? gs)
    {
        r.ClearMsgs();
        r.AddMsg("¿A dónde viajas?");
        r.AddMsg("");
        var dests = World.Locations.Where(l => l.Id != gs?.CurrentLocationId).ToList();
        var opts = new System.Collections.Generic.List<string>();
        for (int i = 0; i < dests.Count; i++)
        {
            string marker = dests[i].IsDungeon ? " ⚠ MAZMORRA" : "";
            opts.Add("[" + (i + 1) + "] " + dests[i].Name + marker);
        }
        opts.Add("[B] Volver");
        r.SetMenu(opts.ToArray());
    }

    public static void ShowShop(GameRenderer r, int gold)
    {
        r.ClearMsgs();
        r.AddMsg("TIENDA — Oro: " + gold);
        r.AddMsg("");
        r.SetMenu(ShopCatalog.MenuOptions());
    }

    public static void ShowInventory(GameRenderer r, Player p, GameState? gs)
    {
        r.ClearMsgs();
        r.AddMsg("=== " + p.Name + " ===");
        r.AddMsg("Clase: " + p.Class + " | Nivel: " + p.Level);
        r.AddMsg("HP: " + p.CurrentHp + "/" + p.MaxHp);
        r.AddMsg("ATK: " + p.TotalAttack + " (base " + p.Attack + ") | DEF: " + p.TotalDefense + " (base " + p.Defense + ")");
        r.AddMsg("Oro: " + p.Gold + " | EXP: " + p.Experience + "/" + p.ExperienceToNextLevel);
        if (gs != null) r.AddMsg("Victorias: " + gs.TotalVictories + "/" + gs.TotalBattles);

        // Estado de mazmorra
        if (gs != null && gs.HasWon)
            r.AddMsg("[VERDE] ¡HAS COMPLETADO EL JUEGO! [/]");
        else if (gs != null && gs.IsInDungeon)
            r.AddMsg("[ROJO] EN MAZMORRA — Piso " + gs.DungeonFloor + " [/]");

        // Equipado
        var eq = p.GetEquippedItems();
        if (eq.Count > 0)
        {
            r.AddMsg("[EQUIPADO]");
            foreach (var item in eq)
                r.AddMsg(" * " + item.ToDetailString());
        }
        if (p.Inventory.Count > 0)
        {
            r.AddMsg("Objetos:");
            foreach (var item in p.Inventory)
                r.AddMsg(" * " + item.ToDetailString());
        }
        r.AddMsg("");
        if (GameActions.HasUsablePotion(p))
            r.AddMsg("[U] Usar poción  [ENTER] Volver");
        else
            r.AddMsg("[ENTER] Volver");
        r.ClearMenu();
    }

    public static void ShowSave(GameRenderer r, string[] slots, string msg)
    {
        r.ClearMsgs();
        r.AddMsg("GUARDAR PARTIDA");
        r.AddMsg("");
        if (!string.IsNullOrEmpty(msg)) r.AddMsg("[verde]" + msg + "[/]");
        r.AddMsg("Elige un slot de guardado:");
        r.AddMsg("");
        r.SetMenu(slots);
    }

    public static void ShowLoad(GameRenderer r, string[] slots, string msg)
    {
        r.ClearMsgs();
        r.AddMsg("CARGAR PARTIDA");
        r.AddMsg("");
        if (!string.IsNullOrEmpty(msg)) r.AddMsg("[amarillo]" + msg + "[/]");
        r.AddMsg("Elige un slot para cargar:");
        r.AddMsg("");
        r.SetMenu(slots);
    }

    // ══════════════════════════════════════════════════
    //   MENÚS DE MAZMORRA
    // ══════════════════════════════════════════════════

    public static void ShowDungeonEntrance(GameRenderer r, GameState gs)
    {
        r.ClearMsgs();
        r.AddMsg("════════════════════════════════");
        r.AddMsg("  MAZMORRA PROFUNDA");
        r.AddMsg("  " + GameConstants.DungeonFloorCount + " pisos de oscuridad y muerte.");
        r.AddMsg("");
        r.AddMsg("  ⚠  ADVERTENCIA  ⚠");
        r.AddMsg("  Una vez dentro, NO podrás salir");
        r.AddMsg("  hasta vencer o morir.");
        r.AddMsg("");
        r.AddMsg("  ¿Estás preparado?");
        r.AddMsg("════════════════════════════════");
        r.SetMenu(["[1] Entrar a la mazmorra", "[2] Volver a prepararme"]);
    }

    /// <summary>Menú entre pisos de la mazmorra. Muestra recursos actuales.</summary>
    public static void ShowDungeonFloorTransition(GameRenderer r, GameState gs)
    {
        var p = gs.Player;
        r.ClearMsgs();
        r.AddMsg("═══ PISO " + gs.DungeonFloor + " SUPERADO ═══");
        r.AddMsg("");
        r.AddMsg("Estado actual:");
        r.AddMsg("  HP: " + p.CurrentHp + "/" + p.MaxHp);
        r.AddMsg("  Pociones: " + GameActions.PotionCount(p));
        r.AddMsg("  Oro: " + p.Gold);
        r.AddMsg("");

        if (gs.DungeonFloor >= GameConstants.DungeonFloorCount)
        {
            r.AddMsg("╔══ LLEGASTE AL JEFE FINAL ══╗");
            r.AddMsg("El Dragón Ancestral te espera...");
            r.SetMenu(["[1] Enfrentar al Dragón"]);
        }
        else
        {
            r.AddMsg("Próximo: PISO " + (gs.DungeonFloor + 1));
            var potionCount = GameActions.PotionCount(p);
            if (p.CurrentHp < p.MaxHp / 2)
                r.AddMsg("[AMARILLO]⚠ Tu HP está bajo. Considera prepararte.[/]");

            var opts = new System.Collections.Generic.List<string> { "[1] Continuar al siguiente piso" };
            if (potionCount > 0)
                opts.Add($"[2] Usar poción ({potionCount} disp.)");
            opts.Add("[5] Huir de la mazmorra (pierdes progreso)");
            r.SetMenu(opts.ToArray());
        }
    }

    public static void ShowVictory(GameRenderer r, GameState gs)
    {
        r.ClearMsgs();
        r.AddMsg("╔═══════════════════════════════════╗");
        r.AddMsg("║                                   ║");
        r.AddMsg("║      ¡HAS GANADO EL JUEGO!        ║");
        r.AddMsg("║                                   ║");
        r.AddMsg("╚═══════════════════════════════════╝");
        r.AddMsg("");
        r.AddMsg("  " + gs.Player.Name + " ha purificado la Mazmorra Profunda.");
        r.AddMsg("");
        r.AddMsg("  Estadísticas finales:");
        r.AddMsg("  • Nivel: " + gs.Player.Level);
        r.AddMsg("  • Clase: " + gs.Player.Class);
        r.AddMsg("  • Batallas: " + gs.TotalBattles);
        r.AddMsg("  • Victorias: " + gs.TotalVictories);
        r.AddMsg("  • Oro acumulado: " + gs.Player.Gold);
        r.AddMsg("");
        r.AddMsg("[GRACIAS POR JUGAR!]");
        r.AddMsg("");
        r.SetMenu(["[ENTER] Volver al título"]);
    }
}

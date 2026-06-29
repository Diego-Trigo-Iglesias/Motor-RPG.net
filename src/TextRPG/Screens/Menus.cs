/// <summary>
/// Generación de menús de texto. Cada método recibe el GameRenderer y escribe en su log
/// los mensajes y opciones. Los menús son puramente texto - no contienen lógica de juego.
/// Separación de responsabilidades: Menus solo formatea texto, Actions ejecuta, Game coordina.
/// </summary>

using TextRPG.Core;
using TextRPG.Core.Constants;
using TextRPG.Core.Models;
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

    public static void ShowCombat(GameRenderer r)
    {
        r.ClearMsgs();
        r.AddMsg("Ronda " + (r.CombatRound + 1) + " - Que haces?");
        r.AddMsg("");
        r.SetMenu(["[1] Atacar", "[2] Huir", "[3] Usar poción"]);
    }

    public static void ShowTravel(GameRenderer r, GameState? gs)
    {
        r.ClearMsgs();
        r.AddMsg("A donde viajas?");
        r.AddMsg("");
        var dests = World.Locations.Where(l => l.Id != gs?.CurrentLocationId).ToList();
        var opts = new System.Collections.Generic.List<string>();
        for (int i = 0; i < dests.Count; i++)
            opts.Add("[" + (i + 1) + "] " + dests[i].Name);
        opts.Add("[B] Volver");
        r.SetMenu(opts.ToArray());
    }

    public static void ShowShop(GameRenderer r, int gold)
    {
        r.ClearMsgs();
        r.AddMsg("TIENDA - Oro: " + gold);
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
}

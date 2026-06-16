/// <summary>
/// Generación de menús de texto. Cada método recibe el GameRenderer y escribe en su log
/// los mensajes y opciones. Los menús son puramente texto — no contienen lógica de juego.
/// Separación de responsabilidades: Menus solo formatea texto, Actions ejecuta, Game coordina.
/// </summary>

using TextRPG.Core.Models;
using TextRPG.Rendering;

namespace TextRPG.Screens;

/// <summary>GeneraciÃ³n de menÃºs de texto para todas las pantallas del juego.
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
        r.AddMsg("[2] Mago - Alto dano, fragil");
        r.AddMsg("[3] Picaro - Equilibrado, evasion");
    }

    public static void ShowMain(GameRenderer r, Player? p, Location? loc)
    {
        r.ClearMsgs();
        r.AddMsg("Que haces, " + (p?.Name ?? "Heroe") + "?");
        r.AddMsg("");
        var opts = new System.Collections.Generic.List<string>();
        if (loc?.HasEnemies ?? false) opts.Add("[1] Explorar (buscar enemigos)");
        opts.Add("[2] Viajar a otro lugar");
        if (loc?.HasShop ?? false) opts.Add("[3] Tienda");
        if (loc?.HasHealer ?? false) opts.Add("[4] Curandero (30 oro)");
        opts.Add("[5] Inventario y stats");
        r.SetMenu(opts.ToArray());
    }

    public static void ShowCombat(GameRenderer r)
    {
        r.ClearMsgs();
        r.AddMsg("Ronda " + (r.CombatRound + 1) + " - Que haces?");
        r.AddMsg("");
        r.SetMenu(["[1] Atacar", "[2] Huir"]);
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
        r.SetMenu(["[1] Pocion pequena (10 oro) +30HP",
                   "[2] Pocion grande (25 oro) +70HP",
                   "[3] Amuleto de fuerza (50 oro) +5ATK",
                   "[4] Armadura ligera (40 oro) +3DEF",
                   "[B] Salir"]);
    }

    public static void ShowInventory(GameRenderer r, Player p, GameState? gs)
    {
        r.ClearMsgs();
        r.AddMsg("=== " + p.Name + " ===");
        r.AddMsg("Clase: " + p.Class + " | Nivel: " + p.Level);
        r.AddMsg("HP: " + p.CurrentHp + "/" + p.MaxHp);
        r.AddMsg("ATK: " + p.Attack + " | DEF: " + p.Defense);
        r.AddMsg("Oro: " + p.Gold + " | EXP: " + p.Experience + "/" + p.ExperienceToNextLevel);
        if (gs != null) r.AddMsg("Victorias: " + gs.TotalVictories + "/" + gs.TotalBattles);
        if (p.Inventory.Count > 0)
        {
            r.AddMsg("Objetos:");
            foreach (var item in p.Inventory)
                r.AddMsg(" * " + item);
        }
        r.AddMsg("");
        r.AddMsg("[ENTER] Volver");
        r.ClearMenu();
    }
}


using TextRPG.Core.Constants;
using TextRPG.Core.Enums;
using TextRPG.Core.Models;

namespace TextRPG.Core;

/// <summary>
/// Acciones de dominio puro — sin dependencias de UI.
/// Cada método modifica solo objetos de dominio y devuelve el resultado
/// para que la UI lo interprete. Usado por Desktop y Web por igual.
/// </summary>
public static class GameActions
{
    /// <summary>Intenta comprar un item de la tienda.</summary>
    public static BuyResult BuyItem(Player p, int idx)
    {
        if (idx < 0 || idx >= ShopCatalog.Items.Length)
            return new BuyResult(false, null, "Índice inválido.");

        var item = ShopCatalog.Items[idx];
        if (p.Gold < item.Price)
            return new BuyResult(false, item, "No tienes suficiente oro.");

        p.Gold -= item.Price;

        if (Player.SlotFor(item.Type) != EquipmentSlot.None)
        {
            p.Equip(item);
            return new BuyResult(true, item, "Has equipado: " + item.Name + ".");
        }

        p.Inventory.Add(item);
        return new BuyResult(true, item, "Has comprado: " + item.Name + " (en inventario).");
    }

    /// <summary>Intenta curar al jugador en el curandero.</summary>
    public static HealResult Heal(Player p)
    {
        if (p.Gold < GameConstants.HealCost)
            return new HealResult(false, $"Necesitas {GameConstants.HealCost} oro.");

        p.Gold -= GameConstants.HealCost;
        p.Heal(p.MaxHp);
        return new HealResult(true, "HP completamente restaurado.");
    }

    /// <summary>Viaja a una localización.</summary>
    public static string TravelTo(GameState gs, string destId)
    {
        gs.CurrentLocationId = destId;
        return "Viajando a " + gs.CurrentLocation?.Name + "...";
    }

    /// <summary>Usa una poción del inventario si hay disponible.</summary>
    public static (bool Used, string Message, Item? Potion) UsePotion(Player p)
    {
        var potion = p.Inventory.FirstOrDefault(item => item.HealAmount > 0);
        if (potion == null)
            return (false, "No tienes pociones en el inventario.", null);

        p.Heal(potion.HealAmount);
        p.Inventory.Remove(potion);
        return (true, $"Usaste {potion.Name}! +{potion.HealAmount} HP.", potion);
    }

    /// <summary>Indica si el jugador tiene al menos una poción usable.</summary>
    public static bool HasUsablePotion(Player p) => p.Inventory.Any(item => item.HealAmount > 0);

    /// <summary>Cantidad de pociones en el inventario.</summary>
    public static int PotionCount(Player p) => p.Inventory.Count(item => item.HealAmount > 0);

    // ══════════════════════════════════════════════════
    //   ACCIONES DE MAZMORRA
    // ══════════════════════════════════════════════════

    /// <summary>Entra a la mazmorra: resetea el progreso y establece piso 1.</summary>
    public static void EnterDungeon(GameState gs)
    {
        gs.ResetDungeon();
        gs.DungeonFloor = 1;
        gs.Player.ClearStatusEffects();
    }

    /// <summary>Avanza al siguiente piso de la mazmorra.
    /// Devuelve true si el jugador debe continuar, false si llegó al jefe (piso final).</summary>
    public static bool AdvanceDungeonFloor(GameState gs)
    {
        if (!gs.IsInDungeon) return false;

        if (gs.DungeonFloor >= GameConstants.DungeonFloorCount)
        {
            // Ya está en el jefe — no avanzar más
            return false;
        }

        gs.DungeonFloor++;
        return gs.DungeonFloor < GameConstants.DungeonFloorCount;
    }

    /// <summary>Intenta salir de la mazmorra (pierde progreso).</summary>
    public static void ExitDungeon(GameState gs)
    {
        gs.ResetDungeon();
        gs.CurrentLocationId = "village"; // Vuelve a la aldea
        gs.Player.ClearStatusEffects();
    }

    /// <summary>Crea el enemigo correspondiente al piso actual de la mazmorra.</summary>
    public static Enemy CreateDungeonEnemy(GameState gs) =>
        Enemy.CreateDungeonEnemy(gs.DungeonFloor, gs.Player.Level);

    /// <summary>Obtiene el nombre del piso actual de la mazmorra.</summary>
    public static string GetDungeonFloorName(int floor) => floor switch
    {
        1 => "Entrada de la Mazmorra",
        2 => "Salón de los Escudos",
        3 => "Cámara de las Telarañas",
        4 => "Foso del Berserker",
        5 => "Trono del Dragón",
        _ => "Piso " + floor
    };

    /// <summary>Verifica si el jugador ha cumplido la condición de victoria.</summary>
    public static bool CheckVictory(GameState gs)
    {
        if (gs.HasWon) return true;
        // La victoria se marca manualmente al derrotar al jefe del piso 5
        return false;
    }

    /// <summary>Marca la victoria del jugador.</summary>
    public static void SetVictory(GameState gs)
    {
        gs.HasWon = true;
        gs.ResetDungeon();
    }

    // ══════════════════════════════════════════════════
    //   CREACIÓN DE PERSONAJE
    // ══════════════════════════════════════════════════

    /// <summary>Crea un jugador y el estado inicial del juego.</summary>
    public static GameState CreatePlayer(string name, CharacterClass cls)
    {
        var n = string.IsNullOrWhiteSpace(name) ? "Héroe" : name;
        var gs = new GameState { Player = Player.Create(n, cls) };
        gs.CurrentLocationId = "village";
        return gs;
    }
}

// -- Result types --

public readonly record struct BuyResult(bool Success, Item? Item, string Message);
public readonly record struct HealResult(bool Success, string Message);

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
    /// <summary>Intenta comprar un item de la tienda. Modifica al jugador y devuelve el resultado.</summary>
    public static BuyResult BuyItem(Player p, int idx)
    {
        if (idx < 0 || idx >= ShopCatalog.Items.Length)
            return new BuyResult(false, null, "Indice invalido.");

        var item = ShopCatalog.Items[idx];
        if (p.Gold < item.Price)
            return new BuyResult(false, item, "No tienes suficiente oro.");

        p.Gold -= item.Price;

        if (Player.SlotFor(item.Type) != EquipmentSlot.None)
        {
            p.Equip(item);
            return new BuyResult(true, item, "Has equipado: " + item.Name + ".");
        }

        // Las pociones se almacenan en el inventario; el jugador debe usarlas manualmente.
        p.Inventory.Add(item);
        return new BuyResult(true, item, "Has comprado: " + item.Name + " (en inventario).");
    }

    /// <summary>Intenta curar al jugador. Modifica sus stats y devuelve el resultado.</summary>
    public static HealResult Heal(Player p)
    {
        if (p.Gold < GameConstants.HealCost)
            return new HealResult(false, $"Necesitas {GameConstants.HealCost} oro.");

        p.Gold -= GameConstants.HealCost;
        p.Heal(p.MaxHp);
        return new HealResult(true, "HP completamente restaurado.");
    }

    /// <summary>Viaja a una localización. Modifica el GameState.</summary>
    public static string TravelTo(GameState gs, string destId)
    {
        gs.CurrentLocationId = destId;
        return "Viajando a " + gs.CurrentLocation?.Name + "...";
    }

    /// <summary>Usa una poción del inventario si hay disponible. Devuelve mensaje y si se usó.</summary>
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

using TextRPG.Core.Models;

namespace TextRPG.Core.Constants;

/// <summary>
/// Genera los textos de la tienda desde ItemCatalog, evitando strings duplicados.
/// Cada llamada crea arrays nuevos — son pequeños y sólo se usan al abrir la tienda.
/// </summary>
public static class ShopCatalog
{
    /// <summary>Items disponibles en la tienda, en orden de presentación.</summary>
    public static Item[] Items { get; } =
    [
        ItemCatalog.PotionSmall,
        ItemCatalog.PotionLarge,
        ItemCatalog.AmuletOfStrength,
        ItemCatalog.LightArmor,
        ItemCatalog.IronSword,
    ];

    /// <summary>Genera el array de opciones del menú de tienda (texto plano, sin BBCode).</summary>
    public static string[] MenuOptions()
    {
        var result = new string[Items.Length + 1];
        for (int i = 0; i < Items.Length; i++)
        {
            var item = Items[i];
            result[i] = $"[{i + 1}] {item.Name} ({item.Price} oro) {item.ToStatString()}";
        }
        result[Items.Length] = "[B] Salir";
        return result;
    }

    /// <summary>Genera una línea HTML amigable para el menú de tienda en Web.</summary>
    public static string HtmlMenuItem(int index) // 1-based
    {
        if (index < 1 || index > Items.Length) return "";
        var item = Items[index - 1];
        return $"[{index}] {item.Name} ({item.Price} oro) {item.ToStatString()}";
    }
}

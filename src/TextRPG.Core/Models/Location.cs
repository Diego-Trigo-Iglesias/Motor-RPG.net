using TextRPG.Core.Rendering;

namespace TextRPG.Core.Models;

public record Location(
    string Id,
    string Name,
    string Description,
    string IconKey,
    bool HasShop = false,
    bool HasHealer = false,
    bool HasEnemies = true,
    bool IsDungeon = false
);

public static class World
{
    public static readonly Location[] Locations =
    [
        new("village", "Aldea de Piedragrís",  "Un tranquilo pueblo en el borde del bosque. El olor a pan recién hecho llena el aire.",       IconPalette.Village, HasShop: true, HasHealer: true, HasEnemies: false),
        new("forest",  "Bosque Sombrío",       "Árboles retorcidos bloquean la luz del sol. Crujidos extraños resuenan entre las ramas.",     IconPalette.Forest,  HasEnemies: true),
        new("cave",    "Cueva de los Ecos",    "Estalactitas gotean agua helada. Tus pasos resuenan en la oscuridad.",                         IconPalette.Cave,    HasEnemies: true),
        new("ruins",   "Ruinas del Antiguo Reino", "Columnas caídas y estatuas rotas testimonian una civilización perdida.",                   IconPalette.Ruins,   HasEnemies: true),
        new("delta",   "Delta del Ocaso",       "Ciénagas brumosas donde los árboles retorcidos se reflejan en aguas oscuras. Las luces danzantes extravían a los viajeros.", IconPalette.Ruins, HasEnemies: true),
        new("dungeon", "Mazmorra Profunda",    "El corazón de la oscuridad. Solo los más fuertes sobreviven aquí.",                             IconPalette.Dungeon, HasEnemies: true, IsDungeon: true),
    ];

    public static Location Get(string id) =>
        TryGet(id) ?? Locations[0];

    public static Location? TryGet(string id) =>
        Locations.FirstOrDefault(l => l.Id == id);
}

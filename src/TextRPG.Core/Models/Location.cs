using TextRPG.Core.Rendering;

namespace TextRPG.Core.Models;

public record Location(
    string Id,
    string Name,
    string Description,
    string IconKey,
    bool HasShop = false,
    bool HasHealer = false,
    bool HasEnemies = true
);

public static class World
{
    public static readonly Location[] Locations =
    [
        new("village", "Aldea de Piedragris", "Un tranquilo pueblo en el borde del bosque. El olor a pan recien hecho llena el aire.",            IconPalette.Village, HasShop: true, HasHealer: true, HasEnemies: false),
        new("forest",  "Bosque Sombrio",       "Arboles retorcidos bloquean la luz del sol. Crujidos extranos resuenan entre las ramas.",       IconPalette.Forest,  HasEnemies: true),
        new("cave",    "Cueva de los Ecos",    "Estalactitas gotean agua helada. Tus pasos resuenan en la oscuridad.",                          IconPalette.Cave,    HasEnemies: true),
        new("ruins",   "Ruinas del Antiguo Reino", "Columnas caidas y estatuas rotas testimonian una civilizacion perdida.",                    IconPalette.Ruins,   HasEnemies: true),
        new("dungeon", "Mazmorra Profunda",    "El corazon de la oscuridad. Solo los mas fuertes sobreviven aqui.",                             IconPalette.Dungeon, HasEnemies: true),
    ];

    public static Location Get(string id) =>
        Locations.First(l => l.Id == id);
}


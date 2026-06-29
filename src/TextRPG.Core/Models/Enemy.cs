using TextRPG.Core.Rendering;

namespace TextRPG.Core.Models;

public class Enemy
{
    public string Name { get; set; } = string.Empty;
    public string IconKey { get; set; } = IconPalette.Bullet;
    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int ExpReward { get; set; }
    public int GoldReward { get; set; }

    public bool IsAlive => CurrentHp > 0;

    /// 
    /// Factor de escalado por nivel del jugador.
    /// Cada nivel por encima de 1 aumenta las stats un 10%.
    /// Ej: nivel 5 → 1.4x, nivel 10 → 1.9x, nivel 20 → 2.9x
    /// 
    private const double LevelScaleFactor = 0.10;

    public static readonly Enemy[] Templates =
    [
        new() { Name = "Rata Gigante",   IconKey = IconPalette.Bullet,  MaxHp = 20,  CurrentHp = 20,  Attack = 5,  Defense = 1, ExpReward = 15, GoldReward = 3  },
        new() { Name = "Goblin",         IconKey = IconPalette.Dodge,   MaxHp = 35,  CurrentHp = 35,  Attack = 10, Defense = 3, ExpReward = 30, GoldReward = 7  },
        new() { Name = "Esqueleto",      IconKey = IconPalette.Cross,   MaxHp = 50,  CurrentHp = 50,  Attack = 14, Defense = 5, ExpReward = 50, GoldReward = 12 },
        new() { Name = "Orco Berserker", IconKey = IconPalette.Attack,  MaxHp = 80,  CurrentHp = 80,  Attack = 20, Defense = 8, ExpReward = 80, GoldReward = 20 },
        new() { Name = "Dragon Menor",   IconKey = IconPalette.Critical,MaxHp = 150, CurrentHp = 150, Attack = 30, Defense = 15, ExpReward = 200, GoldReward = 50 },
    ];

    /// 
    /// Crea un enemigo aleatorio escalado al nivel del jugador.
    /// El multiplicador preserva la jerarquía entre enemigos:
    /// un Dragon siempre será más fuerte que una Rata, pero ambos
    /// serán desafiantes a niveles altos.
    /// 
    public static Enemy Random(int playerLevel)
    {
        var pool = Templates.Take(Math.Min(playerLevel + 1, Templates.Length)).ToArray();
        var template = pool[System.Random.Shared.Next(pool.Length)];

        double multiplier = 1.0 + (playerLevel - 1) * LevelScaleFactor;

        return new Enemy
        {
            Name = template.Name,
            IconKey = template.IconKey,
            MaxHp = (int)Math.Round(template.MaxHp * multiplier),
            CurrentHp = (int)Math.Round(template.MaxHp * multiplier),
            Attack = (int)Math.Round(template.Attack * multiplier),
            Defense = (int)Math.Round(template.Defense * multiplier),
            ExpReward = (int)Math.Round(template.ExpReward * multiplier),
            GoldReward = (int)Math.Round(template.GoldReward * multiplier)
        };
    }
}


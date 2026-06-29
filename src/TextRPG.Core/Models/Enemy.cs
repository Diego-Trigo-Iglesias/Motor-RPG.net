using TextRPG.Core.Rendering;

namespace TextRPG.Core.Models;

/// <summary>Comportamiento especial de un enemigo que afecta cómo se desarrolla el combate.</summary>
public enum EnemyBehavior
{
    /// <summary>Sin comportamiento especial. Solo stats.</summary>
    Normal,
    /// <summary>Escudo los primeros 2 turnos: reduce el daño recibido un 50%.</summary>
    Shielded,
    /// <summary>Aplica veneno al golpear: daño 3/turno durante 3 turnos.</summary>
    Venomous,
    /// <summary>Al bajar del 50% HP entra en furia: 1.5× de ataque.</summary>
    Berserker,
    /// <summary>Jefe final. Carga 2 turnos (50% daño) y desata (300% daño) al 3ro. Enfurrece a 30% HP.</summary>
    Boss
}

/// <summary>Enemigo del juego con soporte de comportamientos especiales y escalado por nivel.</summary>
public class Enemy
{
    //  Propiedades base 
    public string Name { get; set; } = string.Empty;
    public string IconKey { get; set; } = IconPalette.Bullet;
    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int ExpReward { get; set; }
    public int GoldReward { get; set; }

    //  Behavior 
    public EnemyBehavior Behavior { get; set; } = EnemyBehavior.Normal;

    //  Estado de comportamiento (se resetea al crear el enemigo) 

    /// <summary>Turnos restantes de escudo (Shielded).</summary>
    public int ShieldTurns { get; set; }

    /// <summary>Contador de carga del jefe: 2=cargando, 1=cargando, 0=ataque masivo.</summary>
    public int BossChargeCounter { get; set; }

    /// <summary>True si el enemigo está enfurecido (Berserker/Boss).</summary>
    public bool IsEnraged { get; set; }

    /// <summary>True si el enemigo acaba de aplicar veneno este turno.</summary>
    public bool AppliedPoisonThisTurn { get; set; }

    public bool IsAlive => CurrentHp > 0;

    /// <summary>Factor de escalado por nivel del jugador: 10% por nivel.</summary>
    private const double LevelScaleFactor = 0.10;

    // ══════════════════════════════════════════════════
    //   MÉTODOS DE COMPORTAMIENTO
    // ══════════════════════════════════════════════════

    /// <summary>Modifica el daño entrante según el comportamiento activo.</summary>
    public int ApplyDefense(int rawDamage)
    {
        if (Behavior == EnemyBehavior.Shielded && ShieldTurns > 0)
        {
            ShieldTurns--;
            return Math.Max(1, rawDamage / 2);
        }
        return rawDamage;
    }

    /// <summary>Modifica el daño saliente según el comportamiento activo.</summary>
    public int ApplyOffense(int baseDamage)
    {
        // Boss carga 2 turnos (daño reducido), desata al 3ro
        if (Behavior == EnemyBehavior.Boss)
        {
            if (BossChargeCounter is 2 or 1)
                return Math.Max(1, baseDamage / 2); // Cargando
            if (BossChargeCounter == 0)
                return baseDamage * 3; // Aliento de dragón
        }

        // Berserker enfurecido
        if (Behavior is EnemyBehavior.Berserker or EnemyBehavior.Boss && IsEnraged)
            return (int)(baseDamage * 1.5);

        return baseDamage;
    }

    /// <summary>Avanza el contador de carga del Boss. Se llama después de aplicar daño.</summary>
    public void AdvanceBossCharge()
    {
        if (Behavior != EnemyBehavior.Boss) return;
        BossChargeCounter--;
        if (BossChargeCounter < 0)
            BossChargeCounter = 2; // Reinicia ciclo: 2→1→0→2→...
    }

    /// <summary>Verifica si el enemigo debe enfurecerse según su HP actual.</summary>
    public void CheckEnrage()
    {
        if (IsEnraged) return;
        if (Behavior == EnemyBehavior.Berserker && CurrentHp <= MaxHp * 0.5)
            IsEnraged = true;
        if (Behavior == EnemyBehavior.Boss && CurrentHp <= MaxHp * 0.3)
            IsEnraged = true;
    }

    /// <summary>Resetea el estado de comportamiento entre combates.</summary>
    public void ResetBehavior()
    {
        ShieldTurns = Behavior == EnemyBehavior.Shielded ? 2 : 0;
        BossChargeCounter = Behavior == EnemyBehavior.Boss ? 2 : 0;
        IsEnraged = false;
        AppliedPoisonThisTurn = false;
    }

    // ══════════════════════════════════════════════════
    //   TEMPLATES DE ENEMIGOS (exploración libre)
    // ══════════════════════════════════════════════════

    public static readonly Enemy[] Templates =
    [
        new() { Name = "Rata Gigante",   IconKey = IconPalette.Bullet,  MaxHp = 20,  CurrentHp = 20,  Attack = 5,  Defense = 1, ExpReward = 15, GoldReward = 3  },
        new() { Name = "Goblin",         IconKey = IconPalette.Dodge,   MaxHp = 35,  CurrentHp = 35,  Attack = 10, Defense = 3, ExpReward = 30, GoldReward = 7  },
        new() { Name = "Esqueleto",      IconKey = IconPalette.Cross,   MaxHp = 50,  CurrentHp = 50,  Attack = 14, Defense = 5, ExpReward = 50, GoldReward = 12 },
        new() { Name = "Orco Berserker", IconKey = IconPalette.Attack,  MaxHp = 80,  CurrentHp = 80,  Attack = 20, Defense = 8, ExpReward = 80, GoldReward = 20 },
        new() { Name = "Dragon Menor",   IconKey = IconPalette.Critical,MaxHp = 150, CurrentHp = 150, Attack = 30, Defense = 15, ExpReward = 200, GoldReward = 50 },
    ];

    /// <summary>Crea un enemigo aleatorio escalado al nivel del jugador (exploración libre).</summary>
    public static Enemy Random(int playerLevel)
    {
        var pool = Templates.Take(Math.Min(playerLevel + 1, Templates.Length)).ToArray();
        var template = pool[System.Random.Shared.Next(pool.Length)];

        double multiplier = 1.0 + (playerLevel - 1) * LevelScaleFactor;

        var enemy = new Enemy
        {
            Name = template.Name,
            IconKey = template.IconKey,
            MaxHp = (int)Math.Round(template.MaxHp * multiplier),
            CurrentHp = (int)Math.Round(template.MaxHp * multiplier),
            Attack = (int)Math.Round(template.Attack * multiplier),
            Defense = (int)Math.Round(template.Defense * multiplier),
            ExpReward = (int)Math.Round(template.ExpReward * multiplier),
            GoldReward = (int)Math.Round(template.GoldReward * multiplier),
        };
        return enemy;
    }

    // ══════════════════════════════════════════════════
    //   ENEMIGOS DE MAZMORRA (comportamiento por piso)
    // ══════════════════════════════════════════════════

    private static readonly Enemy[] DungeonTemplates =
    [
        // Piso 1 — Warmup
        new() { Name = "Rata de las Sombras", IconKey = IconPalette.Bullet,
                MaxHp = 25, CurrentHp = 25, Attack = 6, Defense = 1, ExpReward = 20, GoldReward = 5,
                Behavior = EnemyBehavior.Normal },
        // Piso 2 — Shielded
        new() { Name = "Escudado Maldito", IconKey = IconPalette.Dodge,
                MaxHp = 40, CurrentHp = 40, Attack = 10, Defense = 4, ExpReward = 40, GoldReward = 10,
                Behavior = EnemyBehavior.Shielded },
        // Piso 3 — Venomous
        new() { Name = "Araña Venenosa", IconKey = IconPalette.Cross,
                MaxHp = 50, CurrentHp = 50, Attack = 14, Defense = 3, ExpReward = 60, GoldReward = 15,
                Behavior = EnemyBehavior.Venomous },
        // Piso 4 — Berserker
        new() { Name = "Berserker del Abismo", IconKey = IconPalette.Attack,
                MaxHp = 90, CurrentHp = 90, Attack = 22, Defense = 7, ExpReward = 100, GoldReward = 25,
                Behavior = EnemyBehavior.Berserker },
        // Piso 5 — Boss
        new() { Name = "★ Dragón Ancestral ★", IconKey = IconPalette.Critical,
                MaxHp = 220, CurrentHp = 220, Attack = 35, Defense = 18, ExpReward = 500, GoldReward = 200,
                Behavior = EnemyBehavior.Boss },
    ];

    /// <summary>Crea un enemigo de mazmorra para el piso indicado, escalado al nivel del jugador.
    /// El escalado por piso es más agresivo que el de exploración libre.</summary>
    public static Enemy CreateDungeonEnemy(int floor, int playerLevel)
    {
        if (floor < 1 || floor > DungeonTemplates.Length)
            throw new ArgumentOutOfRangeException(nameof(floor), $"Piso inválido: {floor}");

        var template = DungeonTemplates[floor - 1];

        // Escalado: nivel del jugador + bonus por piso (cada piso suma 15% extra)
        double multiplier = 1.0 + (playerLevel - 1) * LevelScaleFactor + (floor - 1) * 0.15;

        var enemy = new Enemy
        {
            Name = template.Name,
            IconKey = template.IconKey,
            Behavior = template.Behavior,
            MaxHp = (int)Math.Round(template.MaxHp * multiplier),
            CurrentHp = (int)Math.Round(template.MaxHp * multiplier),
            Attack = (int)Math.Round(template.Attack * multiplier),
            Defense = (int)Math.Round(template.Defense * multiplier),
            ExpReward = (int)Math.Round(template.ExpReward * multiplier),
            GoldReward = (int)Math.Round(template.GoldReward * multiplier),
        };
        enemy.ResetBehavior();
        return enemy;
    }
}

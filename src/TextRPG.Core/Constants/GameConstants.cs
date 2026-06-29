using TextRPG.Core.Enums;

namespace TextRPG.Core.Constants;

/// <summary>Constantes del juego, centralizadas para evitar magic numbers duplicados.</summary>
public static class GameConstants
{
    // -- Económicas --
    /// <summary>Coste de la curación en el curandero.</summary>
    public const int HealCost = 30;
    /// <summary>Gold inicial del jugador.</summary>
    public const int StartingGold = 10;

    // -- Progresión --
    /// <summary>Multiplicador de experiencia por nivel: Nivel * ExpPerLevel.</summary>
    public const int ExpPerLevel = 100;
    /// <summary>HP ganado por nivel.</summary>
    public const int HpPerLevel = 10;
    /// <summary>ATK ganado por nivel.</summary>
    public const int AtkPerLevel = 3;
    /// <summary>DEF ganado por nivel.</summary>
    public const int DefPerLevel = 2;

    // -- Stats iniciales por clase (HP, ATK, DEF) --
    public static readonly (int Hp, int Atk, int Def) WarriorStats = (120, 15, 10);
    public static readonly (int Hp, int Atk, int Def) MageStats    = (70,  25,  5);
    public static readonly (int Hp, int Atk, int Def) RogueStats   = (90,  20,  7);
    public static readonly (int Hp, int Atk, int Def) DefaultStats = (100, 15,  8);

    // -- Combate --
    /// <summary>Límite de rondas en combate antes de declarar empate.</summary>
    public const int MaxCombatRounds = 200;
    /// <summary>Probabilidad de golpe crítico (0-100).</summary>
    public const int CritChance = 15;
    /// <summary>Evasión del jugador (0-100).</summary>
    public const int PlayerEvasionChance = 10;
    /// <summary>Evasión del enemigo (0-100).</summary>
    public const int EnemyEvasionChance = 8;

    // -- Acciones de combate --
    /// <summary>Multiplicador de daño para Ataque Fuerte.</summary>
    public const double HeavyAttackMultiplier = 1.5;
    /// <summary>Multiplicador de daño recibido al Defender.</summary>
    public const double DefendDamageReduction = 0.5;
    /// <summary>Multiplicador de daño infligido al Defender.</summary>
    public const double DefendAttackReduction = 0.5;

    // -- Behaviors de enemigos --
    /// <summary>Turnos que dura el escudo (Shielded).</summary>
    public const int ShieldDuration = 2;
    /// <summary>Reducción de daño con escudo (fracción).</summary>
    public const double ShieldDamageReduction = 0.5;
    /// <summary>Daño por turno de veneno.</summary>
    public const int PoisonDamagePerTurn = 3;
    /// <summary>Turnos que dura el veneno.</summary>
    public const int PoisonDuration = 3;
    /// <summary>Umbral de HP% para enfurecer a Berserker.</summary>
    public const double BerserkerEnrageThreshold = 0.5;
    /// <summary>Umbral de HP% para enfurecer al Boss.</summary>
    public const double BossEnrageThreshold = 0.3;
    /// <summary>Multiplicador de ataque del Boss durante carga.</summary>
    public const double BossChargeAttackReduction = 0.5;
    /// <summary>Multiplicador de ataque del Boss al desatar.</summary>
    public const double BossUnleashMultiplier = 3.0;
    /// <summary>Multiplicador de ataque Berserker/Boss enfurecido.</summary>
    public const double EnrageAttackMultiplier = 1.5;

    // -- Mazmorra --
    /// <summary>Número de pisos de la Mazmorra Profunda.</summary>
    public const int DungeonFloorCount = 5;
    /// <summary>Bonus de escalado por piso adicional (fracción).</summary>
    public const double DungeonFloorScaleBonus = 0.15;

    // -- Pantalla --
    /// <summary>Ancho de ventana (Desktop).</summary>
    public const int ScreenWidth = 960;
    /// <summary>Alto de ventana (Desktop).</summary>
    public const int ScreenHeight = 600;
    /// <summary>Nombre máximo del personaje (caracteres).</summary>
    public const int MaxNameLength = 16;

    // -- Animación --
    /// <summary>Duración de la animación de ataque (segundos).</summary>
    public const float AttackAnimDuration = 0.5f;
    /// <summary>Duración de la pantalla de resultado de combate (segundos).</summary>
    public const float CombatResultDuration = 2.5f;
    /// <summary>Duración de la pantalla de game over (segundos).</summary>
    public const float GameOverDuration = 2f;
}

namespace TextRPG.Core.Engine;

/// <summary>Acciones que el jugador puede elegir en cada ronda de combate.</summary>
public enum PlayerAction
{
    /// <summary>Ataque normal. Sin modificadores.</summary>
    Attack,
    /// <summary>Ataque fuerte (1.5× daño). El enemigo ataca primero si sobrevive.</summary>
    HeavyAttack,
    /// <summary>Defensa: recibe 50% de daño, causa 50% de daño.</summary>
    Defend,
    /// <summary>Usar una poción del inventario (no gasta turno de ataque).</summary>
    UsePotion,
    /// <summary>Huir del combate.</summary>
    Flee
}

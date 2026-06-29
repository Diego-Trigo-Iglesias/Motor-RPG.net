namespace TextRPG.Core.Models;

/// <summary>Estado completo del juego. Persistible para guardado/carga.</summary>
public class GameState
{
    public Player Player { get; set; } = null!;
    public string CurrentLocationId { get; set; } = "village";
    public int TotalBattles { get; set; } = 0;
    public int TotalVictories { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSavedAt { get; set; } = DateTime.UtcNow;

    //  Mazmorra 
    /// <summary>Piso actual de la mazmorra. 0 = no está en la mazmorra.</summary>
    public int DungeonFloor { get; set; } = 0;

    /// <summary>True si el jugador ha completado el juego (venció al jefe final).</summary>
    public bool HasWon { get; set; } = false;

    public Location CurrentLocation => World.Get(CurrentLocationId);

    /// <summary>Indica si el jugador está actualmente dentro de la mazmorra.</summary>
    public bool IsInDungeon => DungeonFloor >= 1;

    /// <summary>Resetea el progreso de mazmorra (al salir o morir).</summary>
    public void ResetDungeon()
    {
        DungeonFloor = 0;
    }
}

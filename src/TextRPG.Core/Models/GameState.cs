namespace TextRPG.Core.Models;

public class GameState
{
    public Player Player { get; set; } = null!;
    public string CurrentLocationId { get; set; } = "village";
    public int TotalBattles { get; set; } = 0;
    public int TotalVictories { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSavedAt { get; set; } = DateTime.UtcNow;

    public Location CurrentLocation => World.Get(CurrentLocationId);
}

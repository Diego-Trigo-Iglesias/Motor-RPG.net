using TextRPG.Core.Enums;
using TextRPG.Core.Models;
using TextRPG.Core.Engine;

namespace TextRPG.Core.Rendering;

public interface IRenderer
{
    void Clear();
    void PrintTitle();
    void PrintStatus(Player player);
    void PrintLocation(Location loc);
    void PrintCombatHeader(Player player, Enemy enemy);
    void PrintCombatRound(Player player, Enemy enemy, CombatRound round, int roundNum);
    void PrintVictory(Enemy enemy, int exp, int gold);
    void PrintDefeat();
    void PrintLevelUp(Player player);
    T SelectMenu<T>(string title, Dictionary<T, string> options) where T : notnull;
    string Prompt(string question, string defaultValue);
    bool Confirm(string question);
    void Pause(string? message = null);
    void MarkupLine(string markup);
    string Ask(string question, string defaultValue);
    void Wait(int milliseconds);
    void WritePlayerStats(Player player, GameState state);

    // Animaciones pixel art
    void PlayTravelAnimation(string fromId, string toId);
    void PlayHealAnimation();
    void PlayExploreAnimation(string locationId);
    void PlayClassSelectionAnimation(CharacterClass selectedClass);
}


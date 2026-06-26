using TextRPG.Core.Models;

namespace TextRPG.Core.SaveSystem;

public interface ISaveManager
{
    void Save(GameState state, string slotName);
    GameState? Load(string slotName);
    bool SaveExists(string slotName);
    void Delete(string slotName);
    List<SaveInfo> ListSaves();
}

public record SaveInfo(string Slot, string PlayerInfo, DateTime LastSavedAt);

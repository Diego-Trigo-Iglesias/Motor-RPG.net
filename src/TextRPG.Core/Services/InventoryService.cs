using TextRPG.Core.Models;
using TextRPG.Core.Rendering;

namespace TextRPG.Core.Services;

public sealed class InventoryService
{
    private readonly IRenderer _renderer;

    public InventoryService(IRenderer renderer)
    {
        _renderer = renderer;
    }

    public void Show(Player player, GameState state)
    {
        _renderer.Clear();
        _renderer.MarkupLine("[bold yellow] Estado del personaje[/]\n");
        _renderer.WritePlayerStats(player, state);
        _renderer.Pause();
    }
}



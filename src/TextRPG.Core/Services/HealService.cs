using TextRPG.Core.Models;
using TextRPG.Core.Rendering;

namespace TextRPG.Core.Services;

public sealed class HealService
{
    private readonly IRenderer _renderer;

    public HealService(IRenderer renderer)
    {
        _renderer = renderer;
    }

    public bool HealAtInn(Player player, int cost = 30)
    {
        if (player.Gold < cost)
        {
            _renderer.MarkupLine($"[red]Necesitas {cost} oro para descansar en la posada.[/]");
            _renderer.Pause();
            return false;
        }

        player.Gold -= cost;
        player.Heal(player.MaxHp);
        _renderer.PlayHealAnimation();
        _renderer.MarkupLine("[green]Descansaste en la posada. HP completamente restaurado.[/]");
        _renderer.Pause();
        return true;
    }
}



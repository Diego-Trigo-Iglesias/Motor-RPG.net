using TextRPG.Core.Models;
using TextRPG.Core.Rendering;

namespace TextRPG.Core.Services;

public sealed class ShopService
{
    private readonly IRenderer _renderer;

    public ShopService(IRenderer renderer)
    {
        _renderer = renderer;
    }

    public void Enter(Player player)
    {
        _renderer.Clear();
        _renderer.MarkupLine($"[bold yellow]{IconPalette.Shop} Tienda del Mercader[/]\n");

        var items = new Dictionary<string, (int Price, string Effect)>
        {
            ["Pocion pequena"]  = (10, "Restaura 30 HP"),
            ["Pocion grande"]   = (25, "Restaura 70 HP"),
            ["Amuleto de fuerza"] = (50, "+5 ATK permanente"),
            ["Armadura ligera"]   = (40, "+3 DEF permanente"),
        };

        foreach (var (name, (price, effect)) in items)
            _renderer.MarkupLine($"  [cyan]{name}[/] {IconPalette.Gold} {price}  [grey]{effect}[/]");

        _renderer.MarkupLine($"\n[yellow]{IconPalette.Gold} Tu oro: {player.Gold}[/]\n");

        var opts = items.Keys.ToDictionary(k => k, k => $"{k} ({items[k].Price} {IconPalette.Gold})");
        opts["exit"] = $"{IconPalette.Back} Salir";

        var choice = _renderer.SelectMenu("Que compras?", opts);
        if (choice == "exit") return;

        var (cost, _) = items[choice];
        if (player.Gold < cost)
        {
            _renderer.MarkupLine("[red]No tienes suficiente oro.[/]");
            _renderer.Pause();
            return;
        }

        player.Gold -= cost;

        switch (choice)
        {
            case "Pocion pequena":
                player.Heal(30);
                _renderer.MarkupLine("[green]+30 HP restaurados.[/]");
                break;
            case "Pocion grande":
                player.Heal(70);
                _renderer.MarkupLine("[green]+70 HP restaurados.[/]");
                break;
            case "Amuleto de fuerza":
                player.Attack += 5;
                _renderer.MarkupLine("[green]+5 ATK permanente.[/]");
                break;
            case "Armadura ligera":
                player.Defense += 3;
                _renderer.MarkupLine("[green]+3 DEF permanente.[/]");
                break;
        }

        player.Inventory.Add(choice);
        _renderer.Pause();
    }
}



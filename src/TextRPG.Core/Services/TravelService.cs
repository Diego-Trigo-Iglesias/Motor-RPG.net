using TextRPG.Core.Models;
using TextRPG.Core.Rendering;

namespace TextRPG.Core.Services;

public sealed class TravelService
{
    private readonly IRenderer _renderer;

    public TravelService(IRenderer renderer)
    {
        _renderer = renderer;
    }

    public string? TravelFrom(string currentLocationId)
    {
        var destinations = World.Locations
            .Where(l => l.Id != currentLocationId)
            .ToDictionary(l => l.Id, l => $"{l.IconKey}  {l.Name}");

        destinations["cancel"] = $"{IconPalette.Back} Cancelar";

        var choice = _renderer.SelectMenu("A donde viajas?", destinations);
        if (choice == "cancel") return null;

        return choice;
    }
}



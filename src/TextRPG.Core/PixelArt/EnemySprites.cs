namespace TextRPG.Core.PixelArt;

using static SpriteHelpers;

public static class EnemySprites
{

    //  Rata 16×16 
    public static PixelSprite Rat => _rat.Value;
    private static readonly Lazy<PixelSprite> _rat = new(() =>
    {
        var s = PixelSprite.Create(16, 16);
        // Cuerpo
        Fill(s, 3, 5, 10, 6, C(125, 85, 50));
        Fill(s, 4, 6, 8, 4, C(160, 120, 80));
        // Cabeza
        Fill(s, 1, 4, 5, 4, C(125, 85, 50));
        Fill(s, 2, 5, 3, 2, C(160, 120, 80));
        // Ojo
        Set(s, 2, 5, C(220, 40, 40));
        // Nariz
        Set(s, 1, 6, C(200, 160, 120));
        // Orejas
        Fill(s, 2, 2, 2, 2, C(125, 85, 50));
        Fill(s, 5, 2, 2, 2, C(125, 85, 50));
        Fill(s, 3, 2, 1, 2, C(180, 130, 90));
        Fill(s, 5, 2, 1, 2, C(180, 130, 90));
        // Patas
        Fill(s, 4, 11, 2, 3, C(125, 85, 50));
        Fill(s, 9, 11, 2, 3, C(125, 85, 50));
        // Cola
        Fill(s, 12, 7, 4, 2, C(180, 130, 90));
        Set(s, 13, 6, C(180, 130, 90));
        Set(s, 15, 8, C(125, 85, 50));
        return s;
    });

    //  Goblin 16×16 
    public static PixelSprite Goblin => _gob.Value;
    private static readonly Lazy<PixelSprite> _gob = new(() =>
    {
        var s = PixelSprite.Create(16, 16);
        int cx = 8;
        // Cuerpo
        Fill(s, 4, 8, 8, 6, C(50, 135, 40));
        Fill(s, 5, 9, 6, 4, C(70, 160, 55));
        // Cabeza
        Fill(s, 4, 3, 8, 6, C(50, 135, 40));
        Fill(s, 5, 4, 6, 4, C(70, 160, 55));
        // Orejas puntiagudas
        Fill(s, 1, 4, 3, 2, C(50, 135, 40));
        Fill(s, 12, 4, 3, 2, C(50, 135, 40));
        Fill(s, 2, 5, 2, 1, C(70, 160, 55));
        Fill(s, 12, 5, 2, 1, C(70, 160, 55));
        // Ojos rojos
        Set(s, cx - 2, 5, C(210, 40, 40));
        Set(s, cx + 2, 5, C(210, 40, 40));
        // Boca (dientes)
        Fill(s, cx - 2, 7, 4, 1, C(220, 180, 120));
        Fill(s, cx - 1, 8, 2, 1, C(220, 180, 120));
        // Brazos
        Fill(s, 2, 9, 2, 4, C(50, 135, 40));
        Fill(s, 12, 9, 2, 4, C(50, 135, 40));
        // Piernas
        Fill(s, 5, 14, 2, 2, C(115, 75, 35));
        Fill(s, 9, 14, 2, 2, C(115, 75, 35));
        return s;
    });

    //  Esqueleto 16×16 
    public static PixelSprite Skeleton => _skel.Value;
    private static readonly Lazy<PixelSprite> _skel = new(() =>
    {
        var s = PixelSprite.Create(16, 16);
        int cx = 8;
        // Cuerpo (tórax)
        Fill(s, 5, 8, 6, 5, C(210, 210, 215));
        // Costillas
        Fill(s, 5, 9, 1, 3, C(125, 125, 135));
        Fill(s, 10, 9, 1, 3, C(125, 125, 135));
        // Cabeza (cráneo)
        Fill(s, 4, 3, 8, 6, C(210, 210, 215));
        Fill(s, 5, 4, 6, 4, C(210, 210, 215));
        // Cuencas oculares
        Set(s, cx - 2, 5, C(45, 45, 50));
        Set(s, cx + 2, 5, C(45, 45, 50));
        // Ojos rojos (brillo)
        Set(s, cx - 2, 5, C(250, 35, 35));
        Set(s, cx + 2, 5, C(250, 35, 35));
        // Boca
        Fill(s, cx - 2, 7, 4, 1, C(45, 45, 50));
        Fill(s, cx - 1, 8, 2, 1, C(45, 45, 50));
        // Brazos
        Fill(s, 2, 9, 2, 5, C(210, 210, 215));
        Fill(s, 12, 9, 2, 5, C(210, 210, 215));
        // Cadera
        Fill(s, 6, 13, 4, 1, C(125, 125, 135));
        // Piernas
        Fill(s, 6, 14, 1, 2, C(210, 210, 215));
        Fill(s, 9, 14, 1, 2, C(210, 210, 215));
        return s;
    });

    //  Orco 16×16 
    public static PixelSprite Orc => _orc.Value;
    private static readonly Lazy<PixelSprite> _orc = new(() =>
    {
        var s = PixelSprite.Create(16, 16);
        int cx = 8;
        // Cuerpo (armadura)
        Fill(s, 4, 8, 8, 6, C(95, 70, 40));
        Fill(s, 5, 9, 6, 4, C(120, 90, 55));
        // Cabeza
        Fill(s, 3, 2, 10, 7, C(65, 135, 50));
        Fill(s, 4, 3, 8, 5, C(85, 160, 65));
        // Mandíbula prominente
        Fill(s, 4, 7, 8, 2, C(65, 135, 50));
        // Ojos rojos
        Set(s, cx - 2, 4, C(220, 40, 40));
        Set(s, cx + 2, 4, C(220, 40, 40));
        // Cejas fruncidas
        Set(s, cx - 3, 3, C(40, 95, 30));
        Set(s, cx + 3, 3, C(40, 95, 30));
        // Colmillos
        Set(s, cx - 2, 8, C(230, 190, 100));
        Set(s, cx + 2, 8, C(230, 190, 100));
        // Brazos
        Fill(s, 2, 9, 2, 5, C(65, 135, 50));
        Fill(s, 12, 9, 2, 5, C(65, 135, 50));
        // Piernas
        Fill(s, 5, 14, 2, 2, C(95, 70, 40));
        Fill(s, 9, 14, 2, 2, C(95, 70, 40));
        return s;
    });

    //  Dragón 16×16 
    public static PixelSprite Dragon => _dragon.Value;
    private static readonly Lazy<PixelSprite> _dragon = new(() =>
    {
        var s = PixelSprite.Create(16, 16);
        // Cuerpo
        Fill(s, 4, 5, 10, 8, C(195, 30, 30));
        Fill(s, 5, 6, 8, 6, C(220, 50, 50));
        // Cabeza
        Fill(s, 3, 2, 10, 5, C(195, 30, 30));
        Fill(s, 4, 3, 8, 3, C(220, 50, 50));
        // Ojos amarillos
        Set(s, 5, 3, C(255, 220, 50));
        Set(s, 10, 3, C(255, 220, 50));
        Set(s, 5, 3, C(255, 220, 50));
        Set(s, 10, 3, C(255, 220, 50));
        // Pupilas
        Set(s, 6, 3, C(0, 0, 0));
        Set(s, 9, 3, C(0, 0, 0));
        // Cuernos
        Fill(s, 3, 0, 2, 2, C(85, 50, 18));
        Fill(s, 11, 0, 2, 2, C(85, 50, 18));
        // Alas
        Fill(s, 0, 6, 3, 3, C(60, 60, 70));
        Fill(s, 13, 6, 3, 3, C(60, 60, 70));
        // Vientre claro
        Fill(s, 6, 7, 4, 4, C(240, 200, 100));
        // Garras
        Set(s, 5, 14, C(200, 170, 50));
        Set(s, 7, 14, C(200, 170, 50));
        Set(s, 9, 14, C(200, 170, 50));
        Set(s, 11, 14, C(200, 170, 50));
        return s;
    });

    public static PixelSprite ForName(string name) => name switch
    {
        string n when n.Contains("Rata") => Rat,
        string n when n.Contains("Goblin") => Goblin,
        string n when n.Contains("Esqueleto") => Skeleton,
        string n when n.Contains("Orco") => Orc,
        string n when n.Contains("Dragon") => Dragon,
        _ => Goblin
    };
}

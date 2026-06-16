namespace TextRPG.Core.PixelArt;

/// Sprite pixel art como matriz de colores RGBA.
/// No depende de ninguna librería de renderizado.
/// Cada renderer (Console, Web, Desktop) lo dibuja a su manera.
public sealed class PixelSprite
{
    public int Width { get; }
    public int Height { get; }
    public int TotalPixels => Width * Height;

    private readonly PixelColor[] _pixels;

    private PixelSprite(int width, int height, PixelColor[] pixels)
    {
        Width = width;
        Height = height;
        _pixels = pixels;
    }

    /// Crea un sprite vacío (todos los píxeles transparentes).
    public static PixelSprite Create(int width, int height)
    {
        var pixels = new PixelColor[width * height];
        Array.Fill(pixels, PixelColor.Transparent);
        return new PixelSprite(width, height, pixels);
    }

    /// 
    /// Crea un sprite a partir de arte ASCII.
    /// Cada carácter se mapea a un color según la paleta.
    /// El espacio (' ') es siempre transparente.
    /// 
    public static PixelSprite FromAscii(string[] ascii, Dictionary<char, PixelColor> palette)
    {
        int height = ascii.Length;
        int width = ascii[0].Length;
        var sprite = Create(width, height);

        for (int y = 0; y < height; y++)
        {
            string line = ascii[y];
            for (int x = 0; x < width && x < line.Length; x++)
            {
                char ch = line[x];
                if (ch == ' ') continue;
                if (palette.TryGetValue(ch, out var color))
                    sprite._pixels[y * width + x] = color;
            }
        }
        return sprite;
    }

    /// Establece un píxel.
    public void SetPixel(int x, int y, PixelColor color)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return;
        _pixels[y * Width + x] = color;
    }

    /// Obtiene un píxel.
    public PixelColor GetPixel(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return PixelColor.Transparent;
        return _pixels[y * Width + x];
    }

    /// Obtiene el array plano de píxeles (útil para enviar al renderer).
    public PixelColor[] GetPixels() => _pixels;

    /// Indica si un píxel es transparente.
    public bool IsTransparent(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return true;
        return _pixels[y * Width + x].IsTransparent;
    }

    /// Dibuja este sprite sobre otro (composición).
    public void DrawOn(PixelSprite target, int offsetX = 0, int offsetY = 0)
    {
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                var px = _pixels[y * Width + x];
                if (px.IsTransparent) continue;
                int tx = offsetX + x;
                int ty = offsetY + y;
                if (tx >= 0 && tx < target.Width && ty >= 0 && ty < target.Height)
                    target._pixels[ty * target.Width + tx] = px;
            }
    }

    /// Crea un sprite escalado (cada píxel → bloque scale×scale).
    public PixelSprite Scaled(int scale)
    {
        if (scale <= 1) return this;
        var result = Create(Width * scale, Height * scale);
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                var px = _pixels[y * Width + x];
                if (px.IsTransparent) continue;
                for (int dy = 0; dy < scale; dy++)
                    for (int dx = 0; dx < scale; dx++)
                        result._pixels[(y * scale + dy) * result.Width + (x * scale + dx)] = px;
            }
        return result;
    }

    /// Combina dos sprites superponiendo top sobre bottom.
    public static PixelSprite Merge(PixelSprite bottom, PixelSprite top, int x, int y)
    {
        int w = Math.Max(bottom.Width, top.Width + x);
        int h = Math.Max(bottom.Height, top.Height + y);
        var result = Create(w, h);
        // Copiar bottom
        for (int i = 0; i < bottom.TotalPixels; i++)
        {
            if (!bottom._pixels[i].IsTransparent)
                result._pixels[i] = bottom._pixels[i];
        }
        // Superponer top
        top.DrawOn(result, x, y);
        return result;
    }

    /// Representación ASCII para depuración.
    public string ToAsciiString()
    {
        var lines = new string[Height];
        for (int y = 0; y < Height; y++)
        {
            var chars = new char[Width];
            for (int x = 0; x < Width; x++)
                chars[x] = _pixels[y * Width + x].IsTransparent ? ' ' : '#';
            lines[y] = new string(chars);
        }
        return string.Join('\n', lines);
    }
}

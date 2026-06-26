namespace TextRPG.Core.PixelArt;

/// 
/// Color RGBA para pixel art. Independiente de librerías de renderizado.
/// 
public readonly struct PixelColor : IEquatable<PixelColor>
{
    public byte R { get; }
    public byte G { get; }
    public byte B { get; }
    public byte A { get; }

    public PixelColor(byte r, byte g, byte b, byte a = 255)
    {
        R = r; G = g; B = b; A = a;
    }

    public bool IsTransparent => A == 0;
    public static PixelColor Transparent => new(0, 0, 0, 0);
    public static PixelColor Black => new(0, 0, 0);
    public static PixelColor White => new(255, 255, 255);

    public override bool Equals(object? obj) =>
        obj is PixelColor other && Equals(other);

    public bool Equals(PixelColor other) =>
        R == other.R && G == other.G && B == other.B && A == other.A;

    public override int GetHashCode() =>
        HashCode.Combine(R, G, B, A);

    public static bool operator ==(PixelColor a, PixelColor b) => a.Equals(b);
    public static bool operator !=(PixelColor a, PixelColor b) => !a.Equals(b);
}

namespace TextRPG.Core.PixelArt;

/// Frame de animación: sprite + duración.
public sealed class AnimationFrame
{
    public PixelSprite Sprite { get; }
    public int DelayMs { get; }
    public AnimationFrame(PixelSprite sprite, int delayMs)
    {
        Sprite = sprite ?? throw new ArgumentNullException(nameof(sprite));
        DelayMs = Math.Max(1, delayMs);
    }
}

/// Animación con frames, loop opcional y el contenedor, tal como se usa en el juego.
public sealed class Animation
{
    public string Name { get; }
    public IReadOnlyList<AnimationFrame> Frames { get; }
    public bool Loop { get; }
    public int TotalDurationMs { get; }
    public bool IsFinished { get; private set; }

    public Animation(string name, bool loop, params AnimationFrame[] frames)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name required", nameof(name));
        if (frames is null || frames.Length == 0)
            throw new ArgumentException("Frames required", nameof(frames));
        Name = name;
        Frames = frames;
        Loop = loop;
        TotalDurationMs = frames.Sum(f => f.DelayMs);
        IsFinished = false;
    }

    public AnimationFrame GetFrameAtTime(int elapsedMs)
    {
        if (elapsedMs < 0) return Frames[0];
        int total = TotalDurationMs;
        if (total <= 0) return Frames[0];
        int time = Loop ? elapsedMs % total : Math.Min(elapsedMs, total - 1);
        int accumulated = 0;
        for (int i = 0; i < Frames.Count; i++)
        {
            accumulated += Frames[i].DelayMs;
            if (time < accumulated)
            {
                IsFinished = !Loop && i == Frames.Count - 1;
                return Frames[i];
            }
        }
        IsFinished = !Loop;
        return Frames[^1];
    }

    public void Reset() => IsFinished = false;
}

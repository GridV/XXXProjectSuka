using System;

public sealed class SystemRng : IRng
{
    private readonly Random _random;

    public SystemRng(int seed)
    {
        _random = new Random(seed);
    }

    public float Next01()
    {
        return (float)_random.NextDouble();
    }
}

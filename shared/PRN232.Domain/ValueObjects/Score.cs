using System;

namespace PRN232.Domain.ValueObjects;

public sealed class Score : IEquatable<Score>
{
    public decimal Value { get; }
    public decimal MaxValue { get; }

    private Score(decimal value, decimal maxValue)
    {
        Value = value;
        MaxValue = maxValue;
    }

    public static Score Create(decimal value, decimal maxValue)
    {
        if (value < 0)
            throw new ArgumentException("Score không thể âm.");
        if (value > maxValue)
            throw new ArgumentException("Score vượt quá điểm tối đa.");

        return new Score(value, maxValue);
    }

    public static Score Zero(decimal maxValue) => new(0, maxValue);

    public Score Add(Score other)
    {
        var newMax = MaxValue + other.MaxValue;
        return Create(Value + other.Value, newMax);
    }

    public bool Equals(Score? other) =>
        other is not null && Value == other.Value && MaxValue == other.MaxValue;

    public override bool Equals(object? obj) => Equals(obj as Score);
    public override int GetHashCode() => HashCode.Combine(Value, MaxValue);
}

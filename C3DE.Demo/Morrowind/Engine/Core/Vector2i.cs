using System;

/// <summary>
/// A 2-dimensional vector with integer components.
/// </summary>
public struct Vector2i : IEquatable<Vector2i>
{
    public static Vector2i Zero => new Vector2i(0, 0);
    public static Vector2i Right => new Vector2i(1, 0);
    public static Vector2i Up => new Vector2i(0, 1);
    public static Vector2i One => new Vector2i(1, 1);

    public int X;
    public int Y;

    public static bool operator ==(Vector2i a, Vector2i b)
    {
        return (a.X == b.X) && (a.Y == b.Y);
    }

    public static bool operator !=(Vector2i a, Vector2i b)
    {
        return (a.X != b.X) || (a.Y != b.Y);
    }

    public Vector2i(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static float Distance(Vector2i value1, Vector2i value2)
    {
        float v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
        return (float)Math.Sqrt((v1 * v1) + (v2 * v2));
    }

    public override bool Equals(object obj) => base.Equals(obj);
    public bool Equals(Vector2i other) => (X == other.X) && (Y == other.Y);
    public override int GetHashCode() => X ^ Y;
    public override string ToString() => "(" + X.ToString() + ", " + Y.ToString() + ")";
}
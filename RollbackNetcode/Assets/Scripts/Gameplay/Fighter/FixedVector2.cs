using FixMath.NET;

public struct FixedVector2
{
    public Fix64 x;
    public Fix64 y;

    public FixedVector2(Fix64 x, Fix64 y)
    {
        this.x = x;
        this.y = y;
    }

    // Vector addition
    public static FixedVector2 operator +(FixedVector2 a, FixedVector2 b)
    {
        return new FixedVector2(a.x + b.x, a.y + b.y);
    }

    // Vector subtraction
    public static FixedVector2 operator -(FixedVector2 a, FixedVector2 b)
    {
        return new FixedVector2(a.x - b.x, a.y - b.y);
    }

    // Scalar multiplication
    public static FixedVector2 operator *(FixedVector2 a, Fix64 scalar)
    {
        return new FixedVector2(a.x * scalar, a.y * scalar);
    }

    // Scalar division
    public static FixedVector2 operator /(FixedVector2 a, Fix64 scalar)
    {
        return new FixedVector2(a.x / scalar, a.y / scalar);
    }

    // Magnitude (length) of the vector
    public Fix64 Magnitude => Fix64.Sqrt(x * x + y * y);

    // Normalize the vector
    public FixedVector2 Normalized => this / Magnitude;

    // Dot product
    public static Fix64 Dot(FixedVector2 a, FixedVector2 b)
    {
        return a.x * b.x + a.y * b.y;
    }

    // Convert to Unity Vector2 for display or other purposes
    public UnityEngine.Vector2 ToVector2()
    {
        return new UnityEngine.Vector2((float)x, (float)y);
    }

    // Utility to create a FixedVector2 from a Unity Vector2
    public static FixedVector2 FromVector2(UnityEngine.Vector2 vector)
    {
        return new FixedVector2((Fix64)vector.x, (Fix64)vector.y);
    }
    public override string ToString()
    {
        return $"x: {x.ToString()} y: {y.ToString()}";
    }
}

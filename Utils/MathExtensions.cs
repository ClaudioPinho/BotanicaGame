namespace BotanicaGame.Utils;

public static class MathExtensions
{
    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
    
    public static double Lerp(double a, double b, double t)
    {
        return a + (b - a) * t;
    }
    
    public static float InverseLerp(float a, float b, float value)
    {
        if (a != b)
        {
            return (value - a) / (b - a);
        }
        else
        {
            return 0f; // Or throw an exception if a == b is not expected
        }
    }

    public static double InverseLerp(double a, double b, double value)
    {
        if (a != b)
        {
            return (value - a) / (b - a);
        }
        else
        {
            return 0.0; // Or throw an exception if a == b is not expected
        }
    }
}
public static class MathematicsExtensions
{
    public static double Normalize(this double value, double min, double max)
    {
        return (value - min) / (max - min);
    }
}
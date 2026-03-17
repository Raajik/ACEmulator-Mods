namespace EasyEnlightenment;

public class LevelCost
{
    public double C { get; set; }
    public double Rate { get; set; }
    public double Coefficient { get; set; }
    public int Offset { get; set; }
    public GrowthType GrowthType { get; set; }

    public LevelCost(double c = 100, double rate = 2, double coefficient = 2, int offset = 0, GrowthType growthType = GrowthType.Exponential)
    {
        C = c;
        Rate = rate;
        Coefficient = coefficient;
        Offset = offset;
        GrowthType = growthType;
    }

    public long GetTotalCost(long x, long n)
    {
        n += Offset;
        return GrowthType switch
        {
            GrowthType.Polynomial => Enumerable.Range((int)x, (int)n).Sum(step => GetCost(step)),
            _ => GetTotalCost(x + n) - GetTotalCost(x),
        };
    }

    public long GetTotalCost(long n)
    {
        n += Offset;
        return GrowthType switch
        {
            GrowthType.Linear => (long)((n + 1) * C + (Rate / 2) * n * (n + 1)),
            GrowthType.Exponential => (long)(Rate == 1 ? C * (n + 1) : C * ((1 - Math.Pow(Rate, n + 1)) / (1 - Rate))),
            GrowthType.Polynomial => Enumerable.Range(0, (int)n + 1).Select(step => GetCost(step)).Sum(),
            _ => 0,
        };
    }

    public long GetCost(long n)
    {
        n += Offset;
        return GrowthType switch
        {
            GrowthType.Linear => (long)(C + n * Rate),
            GrowthType.Exponential => (long)(C * Math.Pow(Rate, n)),
            GrowthType.Polynomial => (long)(C + Coefficient * Math.Pow(n, Rate)),
            _ => 0,
        };
    }
}

public enum GrowthType
{
    Linear,
    Exponential,
    Polynomial,
}


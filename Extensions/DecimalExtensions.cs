namespace OpenMicroFiscal.Extensions;

public static class DecimalExtensions
{
    public static decimal RoundTo(this decimal value, int decimals)
    {
        return Math.Round(value, decimals, MidpointRounding.AwayFromZero);
    }

    public static decimal IncreaseBy(this decimal value, decimal percentage)
    {
        return value + value * 0.01M * percentage;
    }
}
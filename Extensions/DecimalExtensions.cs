using System.Globalization;

namespace OpenMicroFiscal.Extensions;

internal static class DecimalExtensions
{
    public static decimal RoundTo(this decimal value, int decimals)
    {
        var roundedValue = Math.Round(value, decimals, MidpointRounding.AwayFromZero);
        var formattedRoundedValue = roundedValue.ToFormattedString(decimals);
        return decimal.Parse(formattedRoundedValue, CultureInfo.InvariantCulture);
    }

    public static decimal IncreaseBy(this decimal value, decimal percentage)
    {
        return value + value * 0.01M * percentage;
    }

    public static string ToFormattedString(this decimal value, int decimals)
    {
        const char zeroChar = '0';
        return value.ToString($"{zeroChar}.{new string(zeroChar, decimals)}", CultureInfo.InvariantCulture);
    }
}
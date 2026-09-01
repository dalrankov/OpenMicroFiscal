namespace OpenMicroFiscal.Extensions;

internal static class DateTimeExtensions
{
    public static DateTime WithoutMilliseconds(this DateTime dateTime)
    {
        return new DateTime(
            dateTime.Ticks - dateTime.Ticks % TimeSpan.TicksPerSecond,
            dateTime.Kind);
    }
}
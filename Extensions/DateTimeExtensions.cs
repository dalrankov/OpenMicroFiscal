namespace OpenMicroFiscal.Extensions;

internal static class DateTimeExtensions
{
    public static DateTime WithoutMilliseconds(this DateTime dateTime)
    {
        return new DateTime(
            dateTime.Year,
            dateTime.Month,
            dateTime.Day,
            dateTime.Hour,
            dateTime.Minute,
            dateTime.Second,
            DateTimeKind.Utc);
    }
}
using System.Globalization;

namespace RetroBin.Infrastructure.Data;

internal static class DbFormat
{
    public static string ToDb(DateTimeOffset value) =>
        value.UtcDateTime.ToString("O", CultureInfo.InvariantCulture);

    public static DateTimeOffset FromDb(string value) =>
        DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

    public static string ToDb(Guid value) => value.ToString("D");
}
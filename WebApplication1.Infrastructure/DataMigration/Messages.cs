using System.Globalization;
using System.Resources;

namespace WebApplication1.DataMigration;

internal static class Messages
{
    private static readonly ResourceManager Resources = new(
        "WebApplication1.DataMigration.Resources.resource_" +
        (Environment.GetEnvironmentVariable("MIGRATION_LANGUAGE") == "en" ? "en" : "ptbr"),
        typeof(Messages).Assembly);

    public static string Get(string key, params object[] args) =>
        string.Format(CultureInfo.CurrentCulture, Resources.GetString(key) ?? key, args);
}

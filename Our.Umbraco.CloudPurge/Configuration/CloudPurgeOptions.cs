namespace Our.Umbraco.CloudPurge;

public sealed class CloudPurgeOptions
{
    public const string SectionName = "CloudPurge";

    public CloudflareOptions Cloudflare { get; set; } = new();
    public bool AutomaticPurgeEnabled { get; set; } = true;
    public string IncludedContentTypes { get; set; } = "";
    public string ExcludedContentTypes { get; set; } = "";

    public bool Allows(string alias)
    {
        static HashSet<string> Parse(string value) => value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var excluded = Parse(ExcludedContentTypes);
        var included = Parse(IncludedContentTypes);
        return !excluded.Contains(alias) && (included.Count == 0 || included.Contains(alias));
    }
}

public sealed class CloudflareOptions
{
    public string ApiToken { get; set; } = "";
    public string ZoneId { get; set; } = "";
}

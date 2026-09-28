# CloudPurge

CloudPurge clears Cloudflare cache when Umbraco content is published, unpublished, or moved. Editors can also purge one page or a page and its descendants from the Content menu.

## Requirements

- Umbraco 17 or 18 on .NET 10
- A Cloudflare API token with **Cache Purge** permission scoped to the required zone

## Configuration

Install the `CloudPurge` NuGet package and supply these settings through environment variables or your deployment's secret manager:

| Environment variable | Purpose |
| --- | --- |
| `CloudPurge__Cloudflare__ApiToken` | Cloudflare API token (secret) |
| `CloudPurge__Cloudflare__ZoneId` | Cloudflare zone ID |

The application validates both values at startup. Keep the token out of source control, browser code, and checked-in `appsettings.json`. For local development, use .NET user secrets:

```powershell
dotnet user-secrets set "CloudPurge:Cloudflare:ApiToken" "<token>" --project <your-website-project>
dotnet user-secrets set "CloudPurge:Cloudflare:ZoneId" "<zone-id>" --project <your-website-project>
```

Optional settings can also be supplied as environment variables:

| Environment variable | Default | Purpose |
| --- | --- | --- |
| `CloudPurge__AutomaticPurgeEnabled` | `true` | Automatic purging for content changes |
| `CloudPurge__IncludedContentTypes` | empty | Comma-separated document type aliases to include |
| `CloudPurge__ExcludedContentTypes` | empty | Comma-separated aliases to exclude; exclusions take priority |

CloudPurge submits absolute published URLs. Configure Umbraco hostnames for every public site and culture so those URLs match the URLs cached by Cloudflare. Only published pages are purged; content without an absolute URL is skipped. A successful Cloudflare response means it accepted the purge request, not that a URL was cached.

## Upgrading from the Umbraco 8 package

Version 1 is a replacement package for Umbraco 17+. The XML `Config/CloudPurge.config` file is no longer read. Transfer the zone ID to `CloudPurge__Cloudflare__ZoneId`, create a zone-scoped API token for `CloudPurge__Cloudflare__ApiToken`, and remove the old XML file from deployments. The settings dashboard and zone-wide purge control have been removed. Manual per-page purging remains available in the Content menu. If an old global API key was exposed through the previous settings UI, rotate it in Cloudflare.

## Development

Use the .NET 10 SDK. Build and test with `dotnet test Our.Umbraco.CloudPurge.Tests/Our.Umbraco.CloudPurge.Tests.csproj`. To check a newer Umbraco version, pass `-p:UmbracoVersion=18.1.1` to restore, build, and test. The minimum referenced version is 17.0.2; CI checks current Umbraco 17 and 18 releases.

## Publishing

Add a NuGet.org API key with permission to push `CloudPurge` as the GitHub Actions repository secret `NUGET_API_KEY`. To publish, update `<Version>` in `Our.Umbraco.CloudPurge/Our.Umbraco.CloudPurge.csproj`, then push a matching `v<version>` tag (for example, `v1.0.0-preview1`). The workflow tests against Umbraco 17 and 18, packs the release, and pushes it to NuGet.org. Branch and pull request builds only test and pack locally.

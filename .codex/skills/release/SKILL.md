---
name: release
description: Release CloudPurge from this repository by updating its NuGet version, committing it, and pushing a matching Git tag. Use when asked to make a preview or full release.
---

# Release CloudPurge

Ask whether the user wants a **preview** or **full** release unless they have already specified one. Wait for the choice before changing files or pushing anything. The requested release includes pushing the version commit and tag; the tag triggers the publishing workflow in `.github/workflows/main.yml`.

Use `Our.Umbraco.CloudPurge/Our.Umbraco.CloudPurge.csproj` as the version source. Its `<Version>` must have the form `MAJOR.MINOR.PATCH` or `MAJOR.MINOR.PATCH-previewN`, where `N` is a positive integer. Incrementing the version means incrementing `PATCH` by one and leaving `MAJOR` and `MINOR` unchanged.

Before calculating the next version, fetch tags from `origin` and inspect the package version and remote tags. A current preview series consists of tags `vMAJOR.MINOR.PATCH-previewN` for the package version's current numeric part, provided the corresponding full tag `vMAJOR.MINOR.PATCH` does not exist. Use the highest `N` in that series, not the order returned by Git. Do not count preview tags from older numeric versions. If the package has a preview suffix that conflicts with the current tag series, stop and explain the mismatch rather than guessing.

| Choice | Current preview series exists | Next `<Version>` |
| --- | --- | --- |
| Preview | Yes | Keep the numeric version and use `-previewN` with `N` one greater than the highest existing preview tag. |
| Preview | No | Increment `PATCH` and append `-preview1`. |
| Full | Yes | Keep the numeric version and remove the `-previewN` suffix. |
| Full | No | Increment `PATCH` and use no preview suffix. |

Removing preview tags for a full release means removing the prerelease suffix from `<Version>`. Keep historical Git preview tags; they identify releases already made.

Before editing, require a clean working tree and a named branch. Check that the calculated `v{version}` tag does not already exist locally or on `origin`. Change only the package `<Version>`, then verify that `dotnet msbuild Our.Umbraco.CloudPurge/Our.Umbraco.CloudPurge.csproj -getProperty:Version` returns the exact calculated version. Review the diff, commit the version change, and create an annotated `v{version}` tag on that commit. Push the branch commit first, then push only that exact tag to `origin`. If any check or push fails, stop and report what succeeded; do not push the tag before the commit is on the remote branch.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Cms.Core.Actions;
using Microsoft.Extensions.Logging;

namespace Our.Umbraco.CloudPurge;

[ApiController]
[Route("umbraco/management/api/v1/cloudpurge")]
[Authorize(Policy = global::Umbraco.Cms.Web.Common.Authorization.AuthorizationPolicies.BackOfficeAccess)]
public sealed class CloudPurgeController(
    IContentService contentService,
    IContentPermissionService permissions,
    IBackOfficeSecurityAccessor securityAccessor,
    CloudPurgeService purger,
    ILogger<CloudPurgeController> logger) : ManagementApiControllerBase
{
    [HttpPost("content/{key:guid}")]
    public async Task<IActionResult> Purge(Guid key, [FromQuery] bool descendants, CancellationToken cancellationToken)
    {
        var user = securityAccessor.BackOfficeSecurity?.CurrentUser;
        if (user is null)
            return Unauthorized();

        var content = contentService.GetById(key);
        if (content is null)
            return NotFound();

        var permission = await permissions.AuthorizeAccessAsync(user, key, ActionPublish.ActionLetter);
        if (permission != ContentAuthorizationStatus.Success)
            return Forbid();

        if (descendants &&
            await permissions.AuthorizeDescendantsAccessAsync(user, key, ActionPublish.ActionLetter) != ContentAuthorizationStatus.Success)
            return Forbid();

        try
        {
            var count = await purger.PurgeAsync(new[] { content.Id }, descendants, cancellationToken);
            return Ok(new { purgedUrls = count });
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(exception, "CloudPurge manual purge failed");
            return StatusCode(502, new { error = "Cloudflare could not complete the purge." });
        }
    }
}

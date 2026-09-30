using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Our.Umbraco.CloudPurge.Tests;

public sealed class CloudPurgeControllerTests
{
    [Test]
    public async Task ManualPurgeLooksUpContentByNumericId()
    {
        var key = Guid.NewGuid();
        var user = CreateProxy<IUser>((_, _) => throw new AssertionException("User members should not be accessed."));
        var security = CreateProxy<IBackOfficeSecurity>((method, _) =>
            method.Name == "get_CurrentUser" ? user : throw new AssertionException(method.Name));
        var securityAccessor = CreateProxy<IBackOfficeSecurityAccessor>((method, _) =>
            method.Name == "get_BackOfficeSecurity" ? security : throw new AssertionException(method.Name));
        var idKeyMap = CreateProxy<IIdKeyMap>((method, args) =>
            method.Name == "GetIdForKey" && Equals(args?[0], key)
                && Equals(args?[1], UmbracoObjectTypes.Document)
                ? Attempt<int>.Succeed(42)
                : throw new AssertionException(method.Name));
        var lookedUpId = 0;
        var contentService = CreateProxy<IContentService>((method, args) =>
        {
            if (method.Name != "GetById" || method.GetParameters()[0].ParameterType != typeof(int))
                throw new AssertionException(method.Name);
            lookedUpId = (int)args![0]!;
            return null;
        });
        var permissions = CreateProxy<IContentPermissionService>((method, _) => throw new AssertionException(method.Name));
        var controller = new CloudPurgeController(contentService, idKeyMap, permissions, securityAccessor,
            null!, NullLogger<CloudPurgeController>.Instance);

        var result = await controller.Purge(key, false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
            Assert.That(lookedUpId, Is.EqualTo(42));
        });
    }

    private static T CreateProxy<T>(Func<MethodInfo, object?[]?, object?> handler) where T : class
    {
        var proxy = DispatchProxy.Create<T, MethodProxy>();
        ((MethodProxy)(object)proxy).Handler = handler;
        return proxy;
    }

    public class MethodProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
            Handler(targetMethod ?? throw new AssertionException("Missing method."), args);
    }
}

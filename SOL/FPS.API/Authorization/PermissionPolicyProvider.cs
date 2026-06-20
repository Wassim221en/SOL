using Template.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Template.API.Authorization;

public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // policyName مثال:
        // And:Users.View,Users.Edit
        // Or:Users.View,Users.Create
        if (!policyName.Contains(":"))
            return _fallbackPolicyProvider.GetPolicyAsync(policyName);

        var split = policyName.Split(":");
        var op = Enum.Parse<PermissionOperator>(split[0]);
        var permissions = split[1].Split(",");

        var policy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(op, permissions))
            .Build();

        return Task.FromResult(policy);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => _fallbackPolicyProvider.GetFallbackPolicyAsync();
}
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public class DynamicAuthorizationHandler : AuthorizationHandler<DynamicReuqirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        DynamicReuqirement requirement
        )
    {
        if(context.Resource is  not HttpContext ctx)
        {
            context.Fail();
            return Task.CompletedTask;
        }
        
        var endpoint = ctx.GetEndpoint();
        var authTags = endpoint.Metadata.OfType<TagsAttribute>()
            .SelectMany(x => x.Tags)
            .Where(x => x.StartsWith("auth"))
            .Select(x => x.Split(':').Last());

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        using var scope = ctx.RequestServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PermissionDatabase>();
        foreach(var authTag in authTags)
        {
            if(db.HasPermission(userId, authTag)) 
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        context.Fail();
        return Task.CompletedTask;
    }
}
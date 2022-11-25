using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Server
{
    public static class Login
    {
        public static async Task<IResult> Handler(HttpContext ctx, string returnUrl)
        {
            await ctx.SignInAsync(
                "cookie",
                new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new Claim[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, 
                                Guid.NewGuid().ToString()),
                        },
                    "cookie"
                    )
                )
           );

            return Results.Redirect(returnUrl);
        }
    }
}

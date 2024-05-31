using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace SpotMate.Infrastructure.Options.OptionsSetup;

public sealed class CustomAuthorizationOptionsSetup : IConfigureOptions<AuthorizationOptions>
{
    public void Configure(AuthorizationOptions options)
    {
        options.AddPolicy(CustomJwtBearerDefaults.CheckOnlySignature, p => p.RequireAuthenticatedUser());
    }
}
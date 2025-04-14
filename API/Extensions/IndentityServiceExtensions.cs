using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API.Extensions;

public static class IndentityServiceExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services,
    IConfiguration config)
    {
        services.AddIdentityCore<AppUser>(opt =>
        {
            opt.Password.RequireNonAlphanumeric = false;
        })
    .AddRoles<AppRole>()
    .AddRoleManager<RoleManager<AppRole>>()
    .AddEntityFrameworkStores<DataContext>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var tokenKey = config["TokenKey"] ?? throw new Exception("TokenKey not found");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    });
        services.AddAuthorizationBuilder()
            .AddPolicy("RequiredAdminRole", policy => policy.RequireRole("Admin"))
            .AddPolicy("ModeratorPhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
        return services;
    }
}

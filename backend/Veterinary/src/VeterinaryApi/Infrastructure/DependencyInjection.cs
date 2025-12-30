using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Infrastructure.Auth;
using VeterinaryApi.Infrastructure.Persistence;
using VeterinaryApi.Infrastructure.Services.Hashers;

namespace VeterinaryApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {

        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();


        // jwt options config 
        services.Configure<JwtOptions>(options =>
           {
               options.SingingKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? throw new InvalidOperationException("JWT_SECRET_KEY environment variable is not set");
               options.Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? throw new InvalidOperationException("JWT_ISSUER environment variable is not set");
               options.Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? throw new InvalidOperationException("JWT_AUDIENCE environment variable is not set");
               options.LifeTime = byte.Parse(Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_LIFETIME_MINUTES") ?? "15");
           });


        // auth config 
        services.AddAuthentication(options =>
           {
               options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
               options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
           }).AddJwtBearer(options =>
           {
               options.SaveToken = true;

               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = true,
                   ValidateAudience = true,
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true,
                   ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                   ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET_KEY")!)),
                   ClockSkew = TimeSpan.Zero
               };
           });

        // ef core config  
        var connectionString = Environment
            .GetEnvironmentVariable("DefaultConnection");
        services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        return services;
    }
}

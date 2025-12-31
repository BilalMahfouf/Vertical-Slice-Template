using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.Abstracions.Emails;
using VeterinaryApi.Infrastructure.Auth;
using VeterinaryApi.Infrastructure.Interceptors;
using VeterinaryApi.Infrastructure.Persistence;
using VeterinaryApi.Infrastructure.Services.Hashers;
using VeterinaryApi.Infrastructure.Services.Notifications;
using VeterinaryApi.Infrastructure.Services.Users;

namespace VeterinaryApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
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
        services.AddScoped<IJwtProvider, JwtProvider>();

        var ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
        var ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");


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

        // interceptors config

        services.AddScoped<AuditInterceptor>();

        // ef core config  
        var connectionString = Environment
            .GetEnvironmentVariable("DefaultConnection");
        services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(
            (sp, options) =>
        {
            options.UseNpgsql(connectionString);
        }, ServiceLifetime.Scoped);

        // Email Options config 
        services.Configure<EmailOptions>(options =>
        {
            options.Port = configuration.GetValue<int>("EMAIL_CONFIGURATIONS:PORT");
            options.Host = configuration.GetValue<string>("EMAIL_CONFIGURATIONS:HOST") ?? throw new InvalidOperationException("EMAIL_CONFIGURATIONS_HOST is not set");
            options.Password = Environment.GetEnvironmentVariable("EMAIL_CONFIGURATIONS_PASSWORD") ?? throw new InvalidOperationException("EMAIL_CONFIGURATIONS_PASSWORD environment variable is not set");
            options.Email = Environment.GetEnvironmentVariable("EMAIL_CONFIGURATIONS_EMAIL") ?? throw new InvalidOperationException("EMAIL_CONFIGURATIONS_EMAIL environment variable is not set");
        });
        services.AddSingleton<IEmailService, EmailService>();

        services.AddScoped<ICurrentUser, CurrentUserService>();

        services.AddHttpContextAccessor();

        return services;
    }
}

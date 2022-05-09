using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyBook.DataAccess;
using MyBook.Entity;
using MyBook.Entity.Identity;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults;

namespace MyBook.WebApi;

public static class StartupExtensions
{
    public static IServiceCollection AddAuthenticationAndJwt(this IServiceCollection sc)
    {
        sc.AddAuthentication(configureOptions =>
            {
                configureOptions.DefaultAuthenticateScheme = AuthenticationScheme;
                configureOptions.DefaultChallengeScheme = AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.ClaimsIssuer = AuthenticationScheme;
                ////
                options.TokenValidationParameters = new 
                    TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("iNivDmHLpUA223sqsfhqGbMRdRj1PVkH")),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true
                    };
            });
        return sc;
    }

    public static IServiceCollection AddIdentity(this IServiceCollection services)
    {
        services
            .AddIdentity<User, Role>()
            .AddEntityFrameworkStores<ApplicationContext>()
            .AddDefaultTokenProviders();
        
        // Configure Identity to use the same JWT claims as OpenIddict instead
        // of the legacy WS-Federation claims it uses by default (ClaimTypes),
        // which saves you from doing the mapping in your authorization controller.
        services.Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserNameClaimType = Claims.Name;
            options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
            options.ClaimsIdentity.RoleClaimType = Claims.Role;
            options.ClaimsIdentity.EmailClaimType = Claims.Email;
        });
        
        return services;
    }

    public static OpenIddictBuilder AddOpenIddictServer(this IServiceCollection services, 
        IWebHostEnvironment environment)
    {
        return services
            .AddOpenIddict()
            .AddCore(options =>
            {
                options
                    .UseEntityFrameworkCore()
                    .UseDbContext<ApplicationContext>();
            })
            .AddServer(options =>
            {
                options
                    .AcceptAnonymousClients()
                    .AllowPasswordFlow()
                    .AllowRefreshTokenFlow();

                options
                    .SetTokenEndpointUris("/Auth/SignUp", "/Auth/Login");
                
                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                var cfg = options.UseAspNetCore();
                if (environment.IsDevelopment() || environment.IsStaging())
                {
                    cfg.DisableTransportSecurityRequirement();   
                }
                
                cfg.EnableTokenEndpointPassthrough();
                
                options
                    .AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();
            }).AddValidation(options =>
            {
                options.UseAspNetCore();
                options.UseLocalServer();
            });
    }
}
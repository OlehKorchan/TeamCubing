using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AspNetCore.Identity.Stores;
using AspNetCore.Identity.Stores.AzureCosmosDB.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Serilog;
using TeamCubing.API.Hubs;
using TeamCubing.BLL.Interfaces;
using TeamCubing.BLL.Services;
using TeamCubing.DAL.Interfaces;
using TeamCubing.DAL.Repositories;
using TeamCubing.Domain.Models;
using TeamCubing.Domain.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (_, lc) => lc
        .WriteTo.Console()
        .WriteTo.Seq("https://teamcubing-logs.azurewebsites.net"));

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            "TeamCubingApp",
            new CorsPolicyBuilder()
                .WithOrigins(
                    "https://localhost:44420",
                    "http://teamcubing.somee.com",
                    "https://teamcubing.azurewebsites.net/")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .Build());
    });
// if (builder.Environment.IsProduction())
// {
//     builder.Configuration.AddAzureKeyVault(
//         new Uri("https://team-cubing.vault.azure.net/"),
//         new DefaultAzureCredential()
//     );
// }

builder.Services
    .AddControllers()
    .AddJsonOptions(
        options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        }
    );
builder.Services.AddSignalR();

AddSwagger();

ConfigureServices();

var settings = builder.Configuration.GetSection(nameof(Settings));
builder.Services.Configure<Settings>(settings);
var configuration = settings.Get<Settings>();

SetupIdentity();

AddAuthAndUserAccessor();

ConfigureMiddleware();

void AddSwagger()
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(
        options =>
        {
            options.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Name = HeaderNames.Authorization,
                    Description =
                        "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                });

            options.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                        },
                        Array.Empty<string>()
                    },
                });
        }
    );
}

void SetupIdentity()
{
    builder.Services.Configure<IdentityStoresOptions>(
        options => options
            .UseAzureCosmosDB(
                configuration.CosmosSettings.Host,
                configuration.CosmosSettings.Secret,
                databaseId: configuration.CosmosSettings.Database));

    builder.Services.AddDefaultIdentity<ApplicationUser>(
            options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 5;
                options.Password.RequiredUniqueChars = 1;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            }
        )
        .AddRoles<IdentityRole>()
        .AddAzureCosmosDbStores()
        .AddDefaultTokenProviders();
}

void AddAuthAndUserAccessor()
{
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(configuration.JwtSettings.TokenKey)
    );
    builder.Services
        .AddAuthentication(
            options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }
        )
        .AddJwtBearer(
            options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = key,
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/api/hubs"))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },
                };
            }
        );

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddTransient(
        services =>
        {
            var httpContextAccessor = services.GetService<IHttpContextAccessor>();

            var userClaims = httpContextAccessor?.HttpContext?.User;
            var userName = userClaims.FindFirstValue(ClaimTypes.NameIdentifier);

            return new ApplicationUser
            {
                UserName = userName,
            };
        });
}

void ConfigureServices()
{
    builder.Services.AddSingleton<IRoomRepository, RoomRepository>();
    builder.Services.AddTransient<IJwtGenerator, JwtGenerator>();
    builder.Services.AddTransient<IRoomService, RoomService>();
    builder.Services.AddTransient<IScramblerService, ScramblerService>();
}

void ConfigureMiddleware()
{
    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }

    app.UseCors("TeamCubingApp");

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseAuthentication();

    app.UseRouting();

    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<RoomHub>("/api/hubs/room");

    app.MapFallbackToFile("index.html");

    app.Run();
}

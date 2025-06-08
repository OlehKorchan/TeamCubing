using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Serilog;
using TeamCubing.API.Hubs;
using TeamCubing.API.Middleware;
using TeamCubing.BLL.Interfaces;
using TeamCubing.BLL.Services;
using TeamCubing.DAL.Interfaces;
using TeamCubing.DAL.Repositories;
using TeamCubing.Domain.DTO;
using TeamCubing.Domain.MappingProfiles;
using TeamCubing.Domain.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

builder.Host.ConfigureLogging(cfg => cfg.ClearProviders())
    .UseSerilog(
        (_, lc) => lc
            .WriteTo.Console());

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            "TeamCubingApp",
            new CorsPolicyBuilder()
                .WithOrigins(
                    "https://localhost:44420",
                    "https://team-cubing.azurewebsites.net/")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .Build());
    });

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
builder.Services.AddAutoMapper(typeof(GeneralProfile));

var settings = builder.Configuration.GetSection(nameof(Settings));
builder.Services.Configure<Settings>(settings);
var configuration = settings.Get<Settings>();

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

    builder.Services.AddScoped<UserContext>();
}

void ConfigureServices()
{
    builder.Services.AddSingleton<IRoomRepository, RoomRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IAccountService, AccountService>();
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
    app.UseMiddleware<CustomAuthMiddleware>();

    app.MapControllers();
    app.MapHub<RoomHub>("/api/hubs/room");

    app.MapFallbackToFile("index.html");

    app.Run();
}
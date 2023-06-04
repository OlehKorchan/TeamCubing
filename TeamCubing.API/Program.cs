using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Serilog;
using TeamCubing.API.Hubs;
using TeamCubing.API.Mappings;
using TeamCubing.BLL.Interfaces;
using TeamCubing.BLL.Services;
using TeamCubing.BLL.Settings;
using TeamCubing.DAL.Data;
using TeamCubing.DAL.Interfaces;
using TeamCubing.DAL.Models;
using TeamCubing.DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (_, lc) => lc
        .WriteTo.Console()
        .WriteTo.Seq("http://localhost:5341"));

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            "TeamCubingApp",
            new CorsPolicyBuilder()
                .WithOrigins("https://localhost:44420", "http://teamcubing.somee.com")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .Build());
    });
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddAzureKeyVault(
        new Uri("https://team-cubing.vault.azure.net/"),
        new DefaultAzureCredential()
    );
}

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

var mapperConfig = new MapperConfiguration(mc => { mc.AddProfile(new GeneralProfile()); });

var mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.AddTransient<IJwtGenerator, JwtGenerator>();
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddTransient<ISessionService, SessionService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IRoomService, RoomService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(connectionString),
    ServiceLifetime.Transient
);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(nameof(JwtSettings)));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options => options.User.RequireUniqueEmail = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var key = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:TokenKey"])
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

builder.Services.Configure<IdentityOptions>(
    options =>
    {
        // Password settings.
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;

        // Lockout settings.
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings.
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = false;
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}

app.UseCors("TeamCubingApp");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    dbContext.Database.Migrate();
}

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

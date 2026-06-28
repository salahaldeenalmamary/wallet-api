using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using WalletApi.Data;
using WalletApi.Endpoints;
using WalletApi.Middleware;
using WalletApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Minimal API ───────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();

// ── Swagger with JWT Bearer UI ────────────────────────────────────────────────
builder.Services.AddSwaggerGen(opts =>
{
    opts.SwaggerDoc("v1", new()
    {
        Title = "Wallet API",
        Version = "v1",
        Description = "ASP.NET Core + PostgreSQL multi-wallet system with deposits, withdrawals, and transfers.",
    });

    // Bearer token input in Swagger UI
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token (without 'Bearer ' prefix).",

    };
    opts.AddSecurityDefinition("Bearer", securityScheme);

    opts.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        opts.IncludeXmlComments(xmlPath);
});

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<WalletDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default"),
        npgsql => npgsql.MigrationsAssembly(typeof(WalletDbContext).Assembly.FullName)
    ));

// ── JWT Bearer Authentication ─────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = Encoding.UTF8.GetBytes(jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key is missing"));

builder.Services
    .AddAuthentication(opts =>
    {
        opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
            ClockSkew = TimeSpan.Zero, // no grace period on expiry
        };
    });

builder.Services.AddAuthorization();

// ── Application Services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();

// ── CORS (Optional but recommended) ──────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// ── App Pipeline ──────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Wallet API v1");
        c.RoutePrefix = string.Empty; // Swagger at root "/"
    });

    // Auto-apply migrations in dev
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<WalletDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseHttpsRedirection();

// ── Order matters! ──────────────────────────────────────────────────────────
app.UseCors("AllowAll");  // CORS should come before Authentication
app.UseAuthentication();  // 1. Authentication
app.UseAuthorization();   // 2. Authorization
// 3. Minimal API Endpoints
app.MapAuthEndpoints();
app.MapExchangeRatesEndpoints();
app.MapTransactionsEndpoints();
app.MapTransfersEndpoints();
app.MapWalletsEndpoints();
app.MapCurrenciesEndpoints();

app.Run();
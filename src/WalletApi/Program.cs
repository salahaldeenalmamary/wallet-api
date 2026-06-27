using Microsoft.EntityFrameworkCore;
using WalletApi.Data;
using WalletApi.Middleware;
using WalletApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Services ─────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
{
    opts.SwaggerDoc("v1", new()
    {
        Title = "Wallet API",
        Version = "v1",
        Description = "ASP.NET Core + PostgreSQL port of bavix/laravel-wallet — " +
                      "multi-wallet system with deposits, withdrawals, and transfers."
    });
    // Include XML comments for Swagger
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

// ── Application Services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITransferService, TransferService>();

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
app.MapControllers();

app.Run();

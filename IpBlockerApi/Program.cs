using IpBlockerApi.BackgroundServices;
using IpBlockerApi.interfaces;
using IpBlockerApi.Repositories;
using IpBlockerApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Controllers + Swagger ─────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "IP Blocker API",
        Version = "v1",
        Description = "Manage blocked countries and validate IP addresses using geolocation."
    });

    // Include XML comments in Swagger (optional but professional)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// ── 2. HttpClient for Geolocation service ────────────────────────────────────
// Named client with a timeout — IHttpClientFactory manages the connection pool
builder.Services.AddHttpClient("GeoLocation", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.Add("User-Agent", "IpBlockerApi/1.0");
});

// ── 3. Repositories (Singleton — in-memory data must survive the request) ────
builder.Services.AddSingleton<IBlockedCountryRepository, InMemoryBlockedCountryRepository>();
builder.Services.AddSingleton<ITemporalBlockRepository, InMemoryTemporalBlockRepository>();
builder.Services.AddSingleton<ILogRepository, InMemoryLogRepository>();

// ── 4. Services (Scoped — new instance per request) ──────────────────────────
builder.Services.AddScoped<IGeolocationService, GeolocationService>();
builder.Services.AddScoped<ICountryBlockService, CountryBlockService>();
builder.Services.AddScoped<ILogService, LogService>();

// ── 5. Background service ─────────────────────────────────────────────────────
builder.Services.AddHostedService<TemporalBlockCleanupService>();

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "IP Blocker API v1");
    c.RoutePrefix = string.Empty; // Swagger at root URL
});

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(c =>
//    {
//        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IP Blocker API v1");
//        c.RoutePrefix = string.Empty;  // Swagger available at root URL
//    });
//}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
using System;
using System.IO;
using AwesomeFiles.Api.Background;
using AwesomeFiles.Api.Middleware;
using AwesomeFiles.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

string? archivesDir, filesRoot, logsRoot;

if (builder.Environment.IsDevelopment())
{
    // тут настройки для дефолтного запуска без докера
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5010);
    });
    filesRoot = builder.Configuration["Api-settings:filesRoot"];
    archivesDir = builder.Configuration["Api-settings:ArchivesDir"];
    logsRoot = builder.Configuration["Api-settings:logsRoot"];
}
else
{
    filesRoot = Environment.GetEnvironmentVariable("FILES_ROOT") ??  builder.Configuration["Api-settings:filesRoot"];
    archivesDir = Environment.GetEnvironmentVariable("ARCHIVES_DIR") ?? builder.Configuration["Api-settings:ArchivesDir"];
    logsRoot = Environment.GetEnvironmentVariable("LOGS_ROOT") ?? builder.Configuration["Api-settings:logsRoot"];
}




// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Awesome Files API", 
        Version = "v1",
        Description = "API for creating ZIP archives from files"
    });
});






// Add logging
Directory.CreateDirectory(logsRoot ?? throw new ArgumentNullException(nameof(logsRoot)));
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File($"{logsRoot}/log-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Заменяем стандартный логгер на Serilog
builder.Host.UseSerilog();

// Register services
builder.Services.AddSingleton<IFileListService, FileListService>();
builder.Services.AddSingleton<ArchiveService>();
builder.Services.AddSingleton<IArchiveService>(provider => provider.GetRequiredService<ArchiveService>());

// Register background service
builder.Services.AddHostedService<ArchiveWorker>();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
   
    app.UseCors("AllowAll");
}
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Awesome Files API v1");
    c.RoutePrefix = string.Empty; // Serve Swagger UI at root
});

app.UseHttpsRedirection();

// Add custom middleware
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Log startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting Awesome Files API");
logger.LogInformation("Files root: {FilesRoot}", filesRoot);
logger.LogInformation("Archives directory: {ArchivesDir}", archivesDir);

// Ensure directories exist
Directory.CreateDirectory(filesRoot ?? throw new ArgumentNullException(nameof(filesRoot)));
Directory.CreateDirectory(archivesDir ?? throw new ArgumentNullException(nameof(archivesDir)));


app.Run();
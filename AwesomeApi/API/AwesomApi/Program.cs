using AwesomeFiles.Api.Background;
using AwesomeFiles.Api.Middleware;
using AwesomeFiles.Api.Services;

var builder = WebApplication.CreateBuilder(args);


if (builder.Environment.IsDevelopment())
{
    // тут настройки для дефолтного запуска без докера
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5010);
    });
    var filesRoot =builder.Configuration["MySettings:ApiKey"];
    var ArchivesDir = builder.Configuration["MySettings:ApiKey"];
}
else
{
    var filesRoot = Environment.GetEnvironmentVariable("FILES_ROOT") ?? "";
    var ArchivesDir = Environment.GetEnvironmentVariable("ARCHIVES_DIR") ?? "";
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
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

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
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Awesome Files API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
    app.UseCors("AllowAll");
}

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
Directory.CreateDirectory(filesRoot);
Directory.CreateDirectory(archivesDir);

app.Run();
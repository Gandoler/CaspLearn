# Точки расширения Awesome Files API

Этот документ описывает места в коде, где можно легко добавить новую функциональность.

## 1. Кэширование архивов

### Место: `ArchiveWorker.cs`

```csharp
// TODO: Implement disk cache for archives
// Cache key: SHA256(sorted filenames + sizes + mtimes)
// If cache hit: return existing archive immediately
// If cache miss: create new archive and store in cache

private async Task<string> GetCacheKeyAsync(List<string> files)
{
    // TODO: Implement cache key generation
    // 1. Sort filenames
    // 2. Get file sizes and modification times
    // 3. Generate SHA256 hash
    throw new NotImplementedException();
}

private async Task<string?> GetCachedArchiveAsync(string cacheKey)
{
    // TODO: Check if cached archive exists
    // Return path to cached archive or null
    throw new NotImplementedException();
}

private async Task StoreInCacheAsync(string cacheKey, string archivePath)
{
    // TODO: Store archive in cache with cache key
    throw new NotImplementedException();
}
```

## 2. Персистентное хранение задач

### Место: `ArchiveService.cs`

```csharp
// TODO: Replace in-memory storage with persistent storage
// Options: SQLite, PostgreSQL, Redis, etc.

public interface IArchiveTaskRepository
{
    Task<ArchiveTask?> GetByIdAsync(Guid id);
    Task<ArchiveTask> CreateAsync(ArchiveTask task);
    Task<ArchiveTask> UpdateAsync(ArchiveTask task);
    Task<bool> DeleteAsync(Guid id);
    Task<List<ArchiveTask>> GetByStatusAsync(ArchiveStatus status);
}

// TODO: Implement repository
public class SqliteArchiveTaskRepository : IArchiveTaskRepository
{
    // TODO: Implement with EF Core
}
```

## 3. Логирование в базу данных

### Место: `Program.cs`

```csharp
// TODO: Add database logging
// 1. Create LogEntry entity
// 2. Configure EF Core DbContext
// 3. Add database logger provider

public class LogEntry
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? RequestId { get; set; }
    public string? UserId { get; set; }
}

// TODO: Add to Program.cs
// builder.Services.AddDbContext<LoggingDbContext>(options =>
//     options.UseSqlite("Data Source=logs.db"));
// builder.Services.AddScoped<ILoggerProvider, DatabaseLoggerProvider>();
```

## 4. Аутентификация и авторизация

### Место: `Program.cs`

```csharp
// TODO: Add authentication
// Options: JWT, OAuth2, API Keys, etc.

// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuer = true,
//             ValidateAudience = true,
//             ValidateLifetime = true,
//             ValidateIssuerSigningKey = true,
//             ValidIssuer = "AwesomeFiles",
//             ValidAudience = "AwesomeFiles.Users",
//             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-secret-key"))
//         };
//     });

// TODO: Add authorization policies
// builder.Services.AddAuthorization(options =>
// {
//     options.AddPolicy("RequireAuthenticatedUser", policy =>
//         policy.RequireAuthenticatedUser());
//     options.AddPolicy("AdminOnly", policy =>
//         policy.RequireRole("Admin"));
// });
```

## 5. Метрики и мониторинг

### Место: `Program.cs`

```csharp
// TODO: Add application metrics
// Options: Prometheus, Application Insights, etc.

// builder.Services.AddSingleton<IMetricsCollector, PrometheusMetricsCollector>();
// builder.Services.AddHealthChecks()
//     .AddCheck<DatabaseHealthCheck>("database")
//     .AddCheck<FileSystemHealthCheck>("filesystem");

// TODO: Add custom metrics
public class ArchiveMetrics
{
    public int TotalArchivesCreated { get; set; }
    public int TotalArchivesFailed { get; set; }
    public TimeSpan AverageProcessingTime { get; set; }
    public long TotalBytesProcessed { get; set; }
}
```

## 6. Уведомления

### Место: `ArchiveWorker.cs`

```csharp
// TODO: Add notification system
// Options: Email, WebSocket, SignalR, etc.

public interface INotificationService
{
    Task NotifyArchiveReadyAsync(Guid taskId, string downloadUrl);
    Task NotifyArchiveFailedAsync(Guid taskId, string errorMessage);
}

// TODO: Implement notification service
public class EmailNotificationService : INotificationService
{
    // TODO: Implement email notifications
}

public class WebSocketNotificationService : INotificationService
{
    // TODO: Implement WebSocket notifications
}
```

## 7. Очередь задач с Redis

### Место: `ArchiveService.cs`

```csharp
// TODO: Replace Channel with Redis queue
// Benefits: Persistence, scaling, monitoring

public interface ITaskQueue
{
    Task EnqueueAsync(ArchiveTask task);
    Task<ArchiveTask?> DequeueAsync(CancellationToken cancellationToken);
    Task<long> GetQueueLengthAsync();
}

// TODO: Implement Redis queue
public class RedisTaskQueue : ITaskQueue
{
    // TODO: Implement with StackExchange.Redis
}
```

## 8. Сжатие и оптимизация

### Место: `ArchiveWorker.cs`

```csharp
// TODO: Add compression options
public class CompressionOptions
{
    public CompressionLevel Level { get; set; } = CompressionLevel.Optimal;
    public bool IncludeEmptyDirectories { get; set; } = false;
    public string? Password { get; set; }
    public List<string> ExcludePatterns { get; set; } = new();
}

// TODO: Add file filtering
private bool ShouldIncludeFile(string filePath, CompressionOptions options)
{
    // TODO: Implement file filtering logic
    // 1. Check exclude patterns
    // 2. Check file size limits
    // 3. Check file type restrictions
    return true;
}
```

## 9. API версионирование

### Место: `Program.cs`

```csharp
// TODO: Add API versioning
// builder.Services.AddApiVersioning(options =>
// {
//     options.DefaultApiVersion = new ApiVersion(1, 0);
//     options.AssumeDefaultVersionWhenUnspecified = true;
//     options.ReportApiVersions = true;
// });

// TODO: Create v2 controllers
// [ApiVersion("2.0")]
// [Route("api/v{version:apiVersion}/[controller]")]
// public class ArchivesV2Controller : ControllerBase
// {
//     // TODO: Add new features for v2
// }
```

## 10. Конфигурация через UI

### Место: Новый проект `AwesomeFiles.Admin`

```csharp
// TODO: Create admin web interface
// Features:
// - View archive tasks
// - Configure settings
// - Monitor system health
// - Manage users (if auth is added)
// - View logs and metrics

public class AdminController : ControllerBase
{
    // TODO: Implement admin endpoints
    [HttpGet("tasks")]
    public async Task<IActionResult> GetTasks()
    {
        // TODO: Return paginated list of tasks
    }
    
    [HttpPost("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] SettingsDto settings)
    {
        // TODO: Update application settings
    }
}
```

## 11. Тестирование

### Место: `tests/` папка

```csharp
// TODO: Add integration tests
public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    // TODO: Test full workflow
    // 1. Create archive
    // 2. Poll status
    // 3. Download archive
    // 4. Verify contents
}

// TODO: Add performance tests
public class PerformanceTests
{
    // TODO: Test with large files
    // TODO: Test concurrent requests
    // TODO: Test memory usage
}

// TODO: Add load tests
public class LoadTests
{
    // TODO: Test with many concurrent users
    // TODO: Test queue behavior under load
}
```

## 12. CI/CD

### Место: `.github/workflows/` или `.gitlab-ci.yml`

```yaml
# TODO: Add CI/CD pipeline
# Features:
# - Run tests
# - Build Docker images
# - Deploy to staging/production
# - Run security scans
# - Generate documentation

name: CI/CD Pipeline
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal
      - name: Publish
        run: dotnet publish -c Release -o ./publish
```

## Рекомендации по реализации

1. **Начните с кэширования** - это даст наибольший эффект для производительности
2. **Добавьте персистентность** - для production использования
3. **Настройте мониторинг** - для отслеживания состояния системы
4. **Добавьте аутентификацию** - для безопасности
5. **Создайте админ-панель** - для удобства управления

Каждое расширение должно включать:
- Unit тесты
- Документацию
- Примеры использования
- Обратную совместимость

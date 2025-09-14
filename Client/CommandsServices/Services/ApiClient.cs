using System.Text;
using AwesomeFiles.Client.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AwesomeFiles.Client.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;
    private readonly string _baseUrl;

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5010";
    }

    public async Task<List<FileMetadata>> GetFilesAsync()
    {
        try
        {
            _logger.LogDebug("Requesting files list from {BaseUrl}/api/files", _baseUrl);
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/files");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var files = JsonConvert.DeserializeObject<List<FileMetadata>>(content) ?? new List<FileMetadata>();
                _logger.LogDebug("Retrieved {Count} files", files.Count);
                return files;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to get files: {StatusCode} - {Content}", response.StatusCode, errorContent);
            throw new ApiException($"Failed to get files: {response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while getting files");
            throw new ApiException($"Network error: {ex.Message}");
        }
    }

    public async Task<Guid> CreateArchiveAsync(List<string> files)
    {
        try
        {
            var request = new CreateArchiveRequest { Files = files };
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _logger.LogDebug("Creating archive for {Count} files", files.Count);
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/archives", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<CreateArchiveResponse>(responseContent);
                if (result?.Id != null)
                {
                    _logger.LogDebug("Archive created with ID: {Id}", result.Id);
                    return result.Id;
                }
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
            var errorMessage = error?.Error ?? $"Failed to create archive: {response.StatusCode}";
            
            _logger.LogError("Failed to create archive: {StatusCode} - {Error}", response.StatusCode, errorMessage);
            throw new ApiException(errorMessage);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while creating archive");
            throw new ApiException($"Network error: {ex.Message}");
        }
    }

    public async Task<ArchiveStatusResponse> GetArchiveStatusAsync(Guid id)
    {
        try
        {
            _logger.LogDebug("Getting status for archive {Id}", id);
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/archives/{id}/status");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var status = JsonConvert.DeserializeObject<ArchiveStatusResponse>(content);
                if (status != null)
                {
                    _logger.LogDebug($"Archive {id} status: {status.Status} ({status.Progress}%)");
                    return status;
                }
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
            var errorMessage = error?.Error ?? $"Failed to get archive status: {response.StatusCode}";
            
            _logger.LogError("Failed to get archive status: {StatusCode} - {Error}", response.StatusCode, errorMessage);
            throw new ApiException(errorMessage);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while getting archive status");
            throw new ApiException($"Network error: {ex.Message}");
        }
    }

    public async Task<Stream> DownloadArchiveAsync(Guid id)
    {
        try
        {
            _logger.LogDebug("Downloading archive {Id}", id);
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/archives/{id}/download");
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Archive {Id} downloaded successfully", id);
                return await response.Content.ReadAsStreamAsync();
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
            var errorMessage = error?.Error ?? $"Failed to download archive: {response.StatusCode}";
            
            _logger.LogError("Failed to download archive: {StatusCode} - {Error}", response.StatusCode, errorMessage);
            throw new ApiException(errorMessage);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while downloading archive");
            throw new ApiException($"Network error: {ex.Message}");
        }
    }
}

public class ApiException : Exception
{
    public ApiException(string message) : base(message) { }
    public ApiException(string message, Exception innerException) : base(message, innerException) { }
}

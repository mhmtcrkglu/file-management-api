using file_management.api.Clients;
using file_management.api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace file_management.api.Services;

public class DocumentService : IDocumentService
{
    private readonly IMemoryCache _cache;
    private readonly IGoogleDriveClient _googleDriveClient;
    
    public DocumentService(IMemoryCache cache, IGoogleDriveClient googleDriveClient)
    {
        _cache = cache;
        _googleDriveClient = googleDriveClient;
    }
    
    public async Task<List<string>> Upload(IFormFile[] files)
    {
        var filePaths = new List<string>();

        var allowedExtensions = GetAllowedFileExtensions();

        foreach (var file in files)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new InvalidOperationException($"Invalid file type: {fileExtension}. Allowed types are: {string.Join(", ", allowedExtensions)}.");
            }

            await using var stream = file.OpenReadStream();
            await _googleDriveClient.UploadFileAsync(stream, file.FileName);
        
            filePaths.Add(file.FileName); 
        }

        return filePaths;
    }

    public async Task<List<DocumentResponse>> GetFiles()
    {
        var documents = await _googleDriveClient.ListFilesAsync();
        return documents.Select(d =>
        {
            var downloadCountKey = $"DownloadCount_{d.Id}";
            var downloadCount = _cache.Get<int>(downloadCountKey);

            return new DocumentResponse
            {
                Id = d.Id,
                Name = d.Name,
                Type = d.Type,
                UploadDate = d.UploadDate,
                ThumbnailLink = d.ThumbnailLink,
                DownloadCount = downloadCount
            };
        }).ToList();
    }
    
    public async Task<DocumentDownloadResponse> Download(string fileId, string? token = null)
    {
        CheckTokenExpireIfExists(token);
        var downloadResponse = await _googleDriveClient.DownloadFileAsync(fileId);
        IncreaseDownloadCount(fileId);
        return new DocumentDownloadResponse
        {
            FileBytes = downloadResponse.FileBytes,
            MimeType = downloadResponse.MimeType
        };
    }
    
    public Task<SharedDocumentResponse> ShareDocumentAsync(string fileId, IUrlHelper urlHelper)
    {
        var token = Guid.NewGuid().ToString();
        var expiry = DateTime.UtcNow.AddHours(1);
        
        _cache.Set(token, expiry, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = expiry
        });
        
        var link = urlHelper.Action("GetPreviewLink", "Document", new { fileId }, "http");
        
        return Task.FromResult(new SharedDocumentResponse
        {
            Url = link + $"?token={token}",
            Token = token,
            ExpiresAt = expiry
        });
    }
    
    public Task<string> GetPreviewLinkAsync(string fileId, string token)
    {
        CheckTokenExpireIfExists(token);
        return Task.FromResult($"https://drive.google.com/file/d/{fileId}/preview");
    }

    public void DeleteAll()
    {
        _googleDriveClient.DeleteAll();
    }
    
    private void CheckTokenExpireIfExists(string? token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            if (_cache.TryGetValue(token, out DateTime expiry))
            {
                if (expiry < DateTime.UtcNow) 
                {
                    throw new UnauthorizedAccessException("The link has expired.");
                }
            }
            else
            {
                throw new UnauthorizedAccessException("Invalid or expired token.");
            }
        }
    }
    
    private static HashSet<string> GetAllowedFileExtensions()
    {
        return new HashSet<string> 
        { 
            ".pdf", ".xls", ".xlsx", ".doc", ".docx", ".txt", 
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" 
        };
    }
    
    private void IncreaseDownloadCount(string fileId)
    {
        var downloadCountKey = $"DownloadCount_{fileId}";

        var downloadCount = _cache.GetOrCreate(downloadCountKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1); // Cache ömrü
            return 0; 
        });

        _cache.Set(downloadCountKey, downloadCount + 1);
    }
}
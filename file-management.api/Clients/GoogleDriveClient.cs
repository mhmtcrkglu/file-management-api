using file_management.api.Clients;
using file_management.api.Clients.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Hosting;

public class GoogleDriveClient : IGoogleDriveClient
{
    private readonly DriveService _driveService;

    public GoogleDriveClient(IWebHostEnvironment env)
    {
        const string applicationName = "File Management App";
        var filePath = Path.Combine(env.WebRootPath, "credentials.json");
        var credential = GoogleCredential.FromFile(filePath)
            .CreateScoped(DriveService.ScopeConstants.Drive);

        _driveService = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = applicationName
        });
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = fileName
        };

        var request = _driveService.Files.Create(fileMetadata, fileStream, "application/octet-stream");
        request.Fields = "id";

        await request.UploadAsync();
        var fileId = request.ResponseBody.Id;
        await MakeFilePublicAsync(fileId);
        return fileId;
    }

    public async Task<GoogleDriveDownloadModel> DownloadFileAsync(string fileId)
    {
        var request = _driveService.Files.Get(fileId);
        var file = await request.ExecuteAsync();
        using var memoryStream = new MemoryStream();
        await request.DownloadAsync(memoryStream);
        return new GoogleDriveDownloadModel()
        {
            FileBytes = memoryStream.ToArray(),
            MimeType = file.MimeType,
        };
    }
    
    public async Task<List<GoogleDriveDocumentModel>> ListFilesAsync()
    {
        var request = _driveService.Files.List();
        request.Fields = "files(id, name, mimeType, createdTime, thumbnailLink)";

        var result = await request.ExecuteAsync();

        return result.Files.Select(a => new GoogleDriveDocumentModel
            {
                Id = a.Id,
                Name = a.Name,
                Type = a.MimeType,
                UploadDate = a.CreatedTimeRaw,
                ThumbnailLink = a.ThumbnailLink
            })
            .ToList();
    }
    
    public async void DeleteAll()
    {
        var items = await ListFilesAsync();
        foreach (var item in items)
        {
            await _driveService.Files.Delete(item.Id).ExecuteAsync();
        }
    }
    
    private async Task MakeFilePublicAsync(string fileId)
    {
        var permission = new Permission
        {
            Role = "reader",
            Type = "anyone" 
        };

        await _driveService.Permissions.Create(permission, fileId).ExecuteAsync();
    }
}
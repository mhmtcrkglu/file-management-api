using file_management.api.Clients.Models;

namespace file_management.api.Clients;

public interface IGoogleDriveClient
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName);
    Task<GoogleDriveDownloadModel> DownloadFileAsync(string fileId);
    Task<List<GoogleDriveDocumentModel>> ListFilesAsync();
    void DeleteAll();
}
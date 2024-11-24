namespace file_management.api.Clients.Models;

public class GoogleDriveDownloadModel
{
    public byte[] FileBytes { get; set; }
    public string MimeType { get; set; }
}
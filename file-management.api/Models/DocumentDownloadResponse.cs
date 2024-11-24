namespace file_management.api.Models;

public class DocumentDownloadResponse
{
    public byte[] FileBytes { get; set; }
    public string MimeType { get; set; }
}

namespace file_management.api.Models;

public class DocumentResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string UploadDate { get; set; }
    public string ThumbnailLink { get; set; }
    public int DownloadCount { get; set; }
}
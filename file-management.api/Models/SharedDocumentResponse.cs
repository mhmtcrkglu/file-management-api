namespace file_management.api.Models;

public class SharedDocumentResponse
{
    public string Url { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}
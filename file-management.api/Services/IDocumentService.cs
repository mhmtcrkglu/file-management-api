using file_management.api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace file_management.api.Services;

public interface IDocumentService
{
    Task<List<string>> Upload(IFormFile[] files);
    Task<List<DocumentResponse>> GetFiles();
    Task<DocumentDownloadResponse> Download(string fileName, string? token);
    Task<string> GetPreviewLinkAsync(string fileId,string token);
    Task<SharedDocumentResponse> ShareDocumentAsync(string fileId, IUrlHelper url);
    void DeleteAll();
}
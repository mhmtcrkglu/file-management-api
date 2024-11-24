using file_management.api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace file_management.api.Controller;

[ApiController]
[Route("[controller]")]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument(IFormFile[] files)
    {
        var filePaths = await _documentService.Upload(files);
        return Ok(new { FilePath = filePaths });
    }
    
    [HttpGet("list")]
    public async Task<IActionResult> GetDocuments()
    {
        var files = await _documentService.GetFiles();
        return Ok(new { Files = files });
    }
    
    [HttpGet("download/{fileId}")]
    public async Task<IActionResult> DownloadDocument(string fileId, [FromQuery] string? token)
    {
        var result = await _documentService.Download(fileId,token);
        return File(result.FileBytes, result.MimeType, fileId);
    }
    
    [HttpGet("share/{fileId}")]
    public async Task<IActionResult> ShareDocument(string fileId)
    {
        var sharedResult = await _documentService.ShareDocumentAsync(fileId, Url);
        return Ok(sharedResult);
    }
    
    [HttpGet("preview-link/{fileId}")]
    public async Task<IActionResult> GetPreviewLink(string fileId, [FromQuery] string token)
    {
        var previewLink = await _documentService.GetPreviewLinkAsync(fileId, token);
        if (string.IsNullOrEmpty(previewLink))
            return NotFound("File not found or access denied.");

        return Redirect(previewLink);
    }
    
    [HttpDelete("delete-all")]
    public Task<IActionResult> DeleteAll()
    {
        _documentService.DeleteAll();
        return Task.FromResult<IActionResult>(NoContent());
    }
    
}
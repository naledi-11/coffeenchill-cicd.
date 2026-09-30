using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CoffeeNChill.Services;

namespace CoffeeNChill.Functions;

public class UploadDocument
{
    private readonly ILogger<UploadDocument> _logger;
    private readonly BlobStorageService _blobStorageService;

    public UploadDocument(
        ILogger<UploadDocument> logger,
        BlobStorageService blobStorageService)
    {
        _logger = logger;
        _blobStorageService = blobStorageService;
    }

    // This function handles POST requests that include a multipart/form-data body featuring a form field called "file".
    // Example (Postman): POST /api/documents/upload, Body -> form-data -> key "file" -> type File -> select a document.
    [Function("UploadDocument")]
    public async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "documents/upload")] HttpRequest req)
    {
        // Due to the inclusion of the AspNetCore Http extension package,
        // the detached worker provides us with a standard HttpRequest in this case, so
        // The parsing of multipart/form-data functions precisely like it does in an MVC controller.

        if (!req.HasFormContentType || req.Form.Files.Count == 0)
        {
            return new BadRequestObjectResult(
                "No file was received. Send the file as multipart/form-data " +
                "with a form field named 'file'.");
        }

        var file = req.Form.Files[0];

        if (file.Length == 0)
        {
            return new BadRequestObjectResult("Uploaded file is empty.");
        }

        if (string.IsNullOrWhiteSpace(file.FileName))
        {
            return new BadRequestObjectResult("Uploaded file has no file name.");
        }

        try
        {
            await using var stream = file.OpenReadStream();

            await _blobStorageService.UploadDocumentAsync(
                file.FileName,
                stream,
                file.ContentType);

            _logger.LogInformation(
                "Uploaded staff document {FileName} ({Size} bytes)",
                file.FileName,
                file.Length);

            return new OkObjectResult(new
            {
                fileName = file.FileName,
                sizeBytes = file.Length,
                uploadedUtc = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            // Any storage-level failure (e.g. Azurite not running) is
            // caught here so the caller gets a clean error response
            // instead of an unhandled exception / HTTP 500 with a stack
            // trace.
            _logger.LogError(ex, "Failed to upload document {FileName}", file.FileName);
            return new ObjectResult(
                $"Failed to upload document '{file.FileName}'. {ex.Message}")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}

using Azure.Core;
using CoffeeNChill.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Reflection.Metadata;

namespace CoffeeNChill.Functions;

public class DownloadDocument
{
    private readonly ILogger<DownloadDocument> _logger;
    private readonly BlobStorageService _blobStorageService;

    public DownloadDocument(
        ILogger<DownloadDocument> logger,
        BlobStorageService blobStorageService)
    {
        _logger = logger;
        _blobStorageService = blobStorageService;
    }
    //This function handles GET requests and streams an earlier.
    // returned the staff document to the caller.
    // {fileName} serves as a route parameter that specifies which document to retrieve.
    // Example: GET /api/documents/download/staff-manual.pdf

    [Function("DownloadDocument")]
    public async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "documents/download/{fileName}")] HttpRequest req,
        string fileName)
    {
        try
        {
            bool exists = await _blobStorageService.DocumentExistsAsync(fileName);

            if (!exists)
            {
                return new NotFoundObjectResult(
                    $"Document '{fileName}' was not found.");
            }

            var (content, contentType) =
                await _blobStorageService.DownloadDocumentAsync(fileName);

           

            //FileStreamResult sends the response directly to the client without first loading the entire file into memory.

            return new FileStreamResult(content, contentType)
            {
                FileDownloadName = fileName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download document {FileName}", fileName);
            return new ObjectResult(
                $"Failed to download document '{fileName}'. {ex.Message}")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}

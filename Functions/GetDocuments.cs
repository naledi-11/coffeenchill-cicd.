using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CoffeeNChill.Services;

namespace CoffeeNChill.Functions;

public class GetDocuments
{
    private readonly ILogger<GetDocuments> _logger;
    private readonly BlobStorageService _blobStorageService;

    public GetDocuments(
        ILogger<GetDocuments> logger,
        BlobStorageService blobStorageService)
    {
        _logger = logger;
        _blobStorageService = blobStorageService;
    }

    // This function handles GET requests and enumerates all staff members.
    // file currently stored, including its name, byte size, and last modified date.
    // Example: GET /api/files

    [Function("GetDocuments")]
    public async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "documents")] HttpRequest req)
    {
        try
        {
            var documents = await _blobStorageService.ListDocumentsAsync();
            return new OkObjectResult(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list documents");
            return new ObjectResult($"Failed to list documents. {ex.Message}")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}

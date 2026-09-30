using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CoffeeNChill.Services;

namespace CoffeeNChill.Functions;

public class DeleteMenuItem
{
    // Logger allows us to record information about the function.
    private readonly ILogger<DeleteMenuItem> _logger;

    // Service responsible for communicating with Azure Table Storage.
    private readonly TableStorageService _tableStorageService;

    // Constructor receives the required services through dependency injection.
    public DeleteMenuItem(
        ILogger<DeleteMenuItem> logger,
        TableStorageService tableStorageService)
    {
        _logger = logger;
        _tableStorageService = tableStorageService;
    }

    // This function responds to DELETE requests.
    //
    // The URL contains:
    // {category} = PartitionKey
    // {id}       = RowKey
    //
    // Example:
    // DELETE /api/menu/Hot%20Drinks/COF-002
    [Function("DeleteMenuItem")]
    public async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "delete",
            Route = "menu/{category}/{id}")] HttpRequest req,
        string category,
        string id)
    {
        // Send the category and ID to the storage service.
        // Azure Tables uses these two values to locate the entity
        // that needs to be deleted.
        await _tableStorageService.DeleteMenuItemAsync(
            category,
            id);

        // Return a successful response to let the client know
        // that the delete operation was completed.
        return new OkObjectResult(
            $"Menu item {id} deleted successfully.");
    }
}
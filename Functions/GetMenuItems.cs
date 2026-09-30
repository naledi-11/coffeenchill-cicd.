using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CoffeeNChill.Services;

namespace CoffeeNChill.Functions;

public class GetMenuItems
{
    // Logger allows us to record information about what the function is doing.
    private readonly ILogger<GetMenuItems> _logger;

    // This service handles communication with Azure Table Storage.
    private readonly TableStorageService _tableStorageService;

    // The constructor receives the logger and Table Storage service
    // through dependency injection.
    public GetMenuItems(
        ILogger<GetMenuItems> logger,
        TableStorageService tableStorageService)
    {
        _logger = logger;
        _tableStorageService = tableStorageService;
    }

    // This tells Azure Functions that this method is an HTTP-triggered function.
    // The "get" means this function responds to GET requests.
    // Route = "menu" creates the endpoint:
    // GET /api/menu
    [Function("GetMenuItems")]
    public async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "menu")] HttpRequest req)
    {
        // Ask the Table Storage service to retrieve every MenuItem
        // stored in the MenuItems Azure Table.
        var menuItems =
            await _tableStorageService.GetAllMenuItemsAsync();

        // Return the retrieved menu items to the client as an HTTP response.
        // OkObjectResult represents a successful HTTP 200 response.
        return new OkObjectResult(menuItems);
    }
}
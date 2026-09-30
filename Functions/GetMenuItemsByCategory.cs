using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CoffeeNChill.Services;

namespace CoffeeNChill.Functions;

public class GetMenuItemsByCategory
{
    // Logger is used to record information about the function.
    private readonly ILogger<GetMenuItemsByCategory> _logger;

    // Service responsible for communicating with Azure Table Storage.
    private readonly TableStorageService _tableStorageService;

    // Constructor receives the required services through dependency injection.
    public GetMenuItemsByCategory(
        ILogger<GetMenuItemsByCategory> logger,
        TableStorageService tableStorageService)
    {
        _logger = logger;
        _tableStorageService = tableStorageService;
    }

    // This function responds to GET requests.
    //
    // {category} is a route parameter.
    //
    // Example:
    // GET /api/menu/category/Hot%20Drinks
    //
    // The value "Hot Drinks" will be passed into the category variable.
    [Function("GetMenuItemsByCategory")]
    public async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "menu/category/{category}")] HttpRequest req,
        string category)
    {
        // Send the category to the Table Storage service.
        // The service searches the PartitionKey for matching categories.
        var menuItems =
            await _tableStorageService.GetMenuItemsByCategoryAsync(category);

        // Return the matching menu items to the client.
        // This produces a successful HTTP 200 response.
        return new OkObjectResult(menuItems);
    }
}
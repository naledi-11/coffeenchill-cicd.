using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CoffeeNChill.Models;
using CoffeeNChill.Services;

namespace CoffeeNChill.Functions;

public class UpdateMenuItem
{
    // Logger is used for recording information about the function.
    private readonly ILogger<UpdateMenuItem> _logger;

    // Service used to communicate with Azure Table Storage.
    private readonly TableStorageService _tableStorageService;

    // Constructor receives the logger and storage service through
    // dependency injection.
    public UpdateMenuItem(
        ILogger<UpdateMenuItem> logger,
        TableStorageService tableStorageService)
    {
        _logger = logger;
        _tableStorageService = tableStorageService;
    }

    // This function responds to PUT requests.
    //
    // The URL contains two route parameters:
    //
    // {category} = PartitionKey
    // {id} = RowKey
    //
    // Example:
    // PUT /api/menu/Hot%20Drinks/COF-002
    [Function("UpdateMenuItem")]
    public async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "put",
            Route = "menu/{category}/{id}")] HttpRequest req,
        string category,
        string id)
    {
        // Read the JSON sent in the request body and convert it
        // into a MenuItem C# object.
        var menuItem =
            await JsonSerializer.DeserializeAsync<MenuItem>(
                req.Body,
                new JsonSerializerOptions
                {
                    // Allows JSON property names such as "name"
                    // to match C# properties such as "Name".
                    PropertyNameCaseInsensitive = true
                });

        // Check whether the request contained valid menu item data.
        // If no MenuItem could be created, return HTTP 400 Bad Request.
        if (menuItem == null)
        {
            return new BadRequestObjectResult("Invalid menu item.");
        }

        // The category and id from the URL determine which Azure Table
        // entity we are updating.
        //
        // PartitionKey identifies the category.
        // RowKey identifies the individual menu item.
        menuItem.PartitionKey = category;
        menuItem.RowKey = id;

        // Send the updated MenuItem to Azure Table Storage.
        await _tableStorageService.UpdateMenuItemAsync(menuItem);

        // Return the updated item to the client.
        return new OkObjectResult(menuItem);
    }
}
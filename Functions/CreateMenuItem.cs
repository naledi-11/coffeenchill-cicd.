using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CoffeeNChill.Models;
using CoffeeNChill.Services;

namespace CoffeeNChill.Functions;

public class CreateMenuItem
{
    private readonly ILogger<CreateMenuItem> _logger;
    private readonly TableStorageService _tableStorageService;

    public CreateMenuItem(
        ILogger<CreateMenuItem> logger,
        TableStorageService tableStorageService)
    {
        _logger = logger;
        _tableStorageService = tableStorageService;
    }
    // The HttpTrigger attribute allows the function to respond to HTTP requests.
    // Microsoft (2026) documents HTTP-triggered Azure Functions and their
    // supported HTTP methods.
    // Reference: Microsoft (2026).
    [Function("CreateMenuItem")]
    public async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous, //anonymus indicates that anyone can create a menu item
            "post",
            Route = "menu")] HttpRequest req)
    {
        var menuItem =
            await JsonSerializer.DeserializeAsync<MenuItem>(req.Body); //Take the JSON from the request and turn it into a C# MenuItem object.

        if (menuItem == null)
        {
            return new BadRequestObjectResult("Invalid menu item."); // the user will recieve an error mesaage if they inserted null values
        }
        _logger.LogInformation(
    "PartitionKey: {PartitionKey}, RowKey: {RowKey}",
    menuItem?.PartitionKey,
    menuItem?.RowKey);

        await _tableStorageService.AddMenuItemAsync(menuItem);// this will send the menu item to the azure table storage

        return new OkObjectResult(menuItem);
    }
}
using Azure.Data.Tables;
using CoffeeNChill.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeNChill.Services
{
    public class TableStorageService
    {
        // TableClient is used to communicate with Azure Table Storage.
        // Microsoft (2026) explains that TableClient provides methods for
        // creating, querying, updating and deleting table entities.
        // Reference: Microsoft (2026).
       
        private readonly TableClient _tableClient;

        public TableStorageService(IConfiguration configuration)
        {
            string connectionString =
                configuration["AzureWebJobsStorage"]
                ?? throw new InvalidOperationException(
                    "AzureWebJobsStorage is not configured.");

            _tableClient = new TableClient(
                connectionString,
                "MenuItems");

            _tableClient.CreateIfNotExists();
        }

        public async Task AddMenuItemAsync(MenuItem item)

        {


            await _tableClient.AddEntityAsync(item);
        }

        public async Task<List<MenuItem>> GetAllMenuItemsAsync()
        {
            var items = new List<MenuItem>();

            await foreach (MenuItem item in _tableClient.QueryAsync<MenuItem>())
            {
                items.Add(item);
            }

            return items;
        }

        public async Task<List<MenuItem>> GetMenuItemsByCategoryAsync(
            string category)
        {
            var items = new List<MenuItem>();

            await foreach (MenuItem item in
                _tableClient.QueryAsync<MenuItem>(
                    x => x.PartitionKey == category))
            {
                items.Add(item);
            }

            return items;
        }

       
           public async Task UpdateMenuItemAsync(MenuItem item)
        {
            // First, retrieve the existing menu item from Azure Table Storage.
            // PartitionKey and RowKey uniquely identify the item.
            MenuItem existingItem =
                await _tableClient.GetEntityAsync<MenuItem>(
                    item.PartitionKey,
                    item.RowKey);

            // Use the ETag of the existing entity.
            // The ETag represents the current version of the entity
            // and prevents us from accidentally overwriting a newer version.
            item.ETag = existingItem.ETag;

            // Replace the existing entity with the updated item.
            await _tableClient.UpdateEntityAsync(
                item,
                item.ETag,
                TableUpdateMode.Replace);
        }
        

        public async Task DeleteMenuItemAsync(
            string category,
            string id)
        {
            await _tableClient.DeleteEntityAsync(
                category,
                id);
        }
    }
}


using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using Azure;
using Azure.Data.Tables;

namespace CoffeeNChill.Models
{
    // MenuItem implements ITableEntity so that it can be stored as an
    // entity in Azure Table Storage.
    // Microsoft (2026) documents ITableEntity as the interface used
    // for Azure Table entities.
    
    public class MenuItem : ITableEntity
    {
        // Azure Table Storage keys
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        // Menu item information
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public bool IsAvailable { get; set; }

        // Required by ITableEntity
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }

}

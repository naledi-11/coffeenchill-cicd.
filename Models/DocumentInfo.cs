using System;

namespace CoffeeNChill.Models
{
    //A staff document that has been uploaded to storage is described by DocumentInfo, a simple DTO rather than an ITableEntity. The "list documents" endpoint returns it to the client.
    public class DocumentInfo
    {
        public string FileName { get; set; } = string.Empty;
        public long SizeInBytes { get; set; }
        public DateTimeOffset? LastModified { get; set; }
    }
}

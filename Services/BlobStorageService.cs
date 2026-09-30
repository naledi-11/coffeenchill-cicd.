using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CoffeeNChill.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CoffeeNChill.Services
{
    // NOTE on Azure File Share vs. Blob Storage:  
    //Although the assignment brief requests an Azure File Share for staff-docs, Azurite (the local storage emulator used for this project) only emulates Blob, Queue, and Table storage; it does not implement the Azure Files service (Microsoft, 2026).
    // BlobStorageService manages the "staff-docs" container used to store staff documents (uploads, listings, and downloads). Because the emulator just does not run that service, pointing a ShareServiceClient at "UseDevelopmentStorage=true" fails to connect locally and there is no workaround. For this use case, a Blob container provides the same streaming upload/list/download experience to a File Share and functions the same against Azurite and a real Azure Storage account. //Rather than being an error, this is a purposeful, documented substitution; see the README.md for the same explanation.

    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(IConfiguration configuration)
        {
            string connectionString =
                configuration["AzureWebJobsStorage"]
                ?? throw new InvalidOperationException(
                    "AzureWebJobsStorage is not configured.");

            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient("staff-docs");
            _containerClient.CreateIfNotExists();
        }

        public async Task<BlobContentInfo> UploadDocumentAsync(
            string fileName,
            Stream content,
            string? contentType)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = string.IsNullOrWhiteSpace(contentType)
                        ? "application/octet-stream"
                        : contentType
                }
            };

            //Instead of first buffering the entire file into memory, UploadAsync(stream,...) streams the file directly to storage.
            var response = await blobClient.UploadAsync(
                content,
                uploadOptions);

            return response.Value;
        }

        public async Task<List<DocumentInfo>> ListDocumentsAsync()
        {
            var documents = new List<DocumentInfo>();

            await foreach (BlobItem blobItem in _containerClient.GetBlobsAsync())
            {
                documents.Add(new DocumentInfo
                {
                    FileName = blobItem.Name,
                    SizeInBytes = blobItem.Properties.ContentLength ?? 0,
                    LastModified = blobItem.Properties.LastModified
                });
            }

            return documents;
        }

        public async Task<bool> DocumentExistsAsync(string fileName)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            return await blobClient.ExistsAsync();
        }

        public async Task<(Stream Content, string ContentType)> DownloadDocumentAsync(
            string fileName)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);

            BlobDownloadStreamingResult download =
                (await blobClient.DownloadStreamingAsync()).Value;

            string contentType =
                download.Details.ContentType ?? "application/octet-stream";

            return (download.Content, contentType);
        }
    }
}

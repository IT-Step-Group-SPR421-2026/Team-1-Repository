using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Shop_mvc_pv421.Interfaces;

namespace Shop_mvc_pv421.Services
{
    public class AzureBlobService : IFileService
    {
        // TODO: read value from appsettings
        private const string containerName = "images";
        private readonly string connectionString;

        public AzureBlobService(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("AzureBlobs")!;
        }

        public async Task<string> SaveImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty");
            }
            var client = new BlobContainerClient(connectionString, containerName);
            await client.CreateIfNotExistsAsync();
            await client.SetAccessPolicyAsync(PublicAccessType.Blob);

            // generate new file name
            string name = Guid.NewGuid().ToString();             // random name
            string extension = Path.GetExtension(file.FileName); // get original extension
            string fullName = name + extension;                  // full name: name.ext

            BlobHttpHeaders httpHeaders = new BlobHttpHeaders()
            {
                ContentType = file.ContentType
            };

            var blob = client.GetBlobClient(fullName);
            await blob.UploadAsync(file.OpenReadStream(), httpHeaders);

            return blob.Uri.ToString();
        }

        public async Task DeleteProductImageExcept(string?[] exceptFiles)
        {
            var client = new BlobContainerClient(connectionString, containerName);
            var blobs = client.GetBlobs();

            var exceptUrls = exceptFiles?.Where(x => !string.IsNullOrEmpty(x))
                .Select(x => Path.GetFileName(x))
                .ToArray()
                ?? Array.Empty<string>();


            foreach (var item in blobs)
            {
                if (exceptUrls.Contains(item.Name)) continue;

                var blob = client.GetBlobClient(item.Name);
                await blob.DeleteIfExistsAsync();
            }
        }

        public async Task DeleteImage(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            var client = new BlobContainerClient(connectionString, containerName);
            var fileName = Path.GetFileName(path);
            var blob = client.GetBlobClient(fileName);

            await blob.DeleteIfExistsAsync();
        }

    }
}

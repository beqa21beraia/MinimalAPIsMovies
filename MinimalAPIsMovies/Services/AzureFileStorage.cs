
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MinimalAPIsMovies.Interfaces;

namespace MinimalAPIsMovies.Services
{
    public class AzureFileStorage : IFileStorage
    {
        private readonly string _conectionString;

        public AzureFileStorage(IConfiguration configuration)
        {
            _conectionString = configuration.GetConnectionString("AzureStorage")!;
        }

        public async Task DeleteAsync(string? route, string container)
        {
            if (string.IsNullOrEmpty(route))
            {
                return;
            }

            var client = new BlobContainerClient(_conectionString, container);
            await client.CreateIfNotExistsAsync();
            var fileName = Path.GetFileName(route);
            var blob = client.GetBlobClient(fileName);
            await blob.DeleteIfExistsAsync();
        }

        public async Task<string> StoreAsync(string container, IFormFile file)
        {
            var client = new BlobContainerClient(_conectionString, container);
            await client.CreateIfNotExistsAsync();
            await client.SetAccessPolicyAsync(PublicAccessType.Blob);
            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var blob = client.GetBlobClient(fileName);
            BlobHttpHeaders blobHttpHeaders = new();
            blobHttpHeaders.ContentType = file.ContentType;
            await blob.UploadAsync(file.OpenReadStream(), blobHttpHeaders);
            return blob.Uri.ToString();
        }
    }
}

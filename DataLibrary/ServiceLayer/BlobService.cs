/* 
*    IBlobService.cs
*    4/19/2026
*    ======================================
*    - Initial creation
*    ======================================
*    Interface for calls to the Azure Blob Service
*   
*/

using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace DataLibrary.ServiceLayer.BlobService
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blob;
       
        public BlobService(IConfiguration config)
        {
            _blob = new BlobServiceClient
            (
                new Uri($"https://nextstepsstorage.blob.core.windows.net"),
                new DefaultAzureCredential()
            );
        }
       
        // Retreive all of the documents in the blob container
        public async Task<List<BlobItem>> GetDocuments()
        {
            var docs = new List<BlobItem>();

            // Get the container by its name, then get each blob in the container and add it to the list
            await foreach (var blob in _blob.GetBlobContainerClient("next-steps-blob").GetBlobsAsync())
            {
                docs.Add(blob);
            }
            
            return docs;
        }

        // Get the bytes of a document in the blob container by its name
        public async Task<byte[]> GetDocumentBytes(string docName)
        {
            using (var ms = new MemoryStream())
            {
                await _blob.GetBlobContainerClient("next-steps-blob") // Get the blob container
                           .GetBlobClient(docName)                    // Get the document by name 
                           .DownloadToAsync(ms);                      // Download it to a memory stream

                return ms.ToArray(); // Return the bytes
            }
        }
    }
}

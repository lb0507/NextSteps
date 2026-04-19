/* 
*    IBlobService.cs
*    4/19/2026
*    ======================================
*    - Initial creation
*    ======================================
*    Interface for calls to the Azure Blob Service
*   
*/

using Azure.Storage.Blobs.Models;

namespace DataLibrary.ServiceLayer.BlobService
{
    public interface IBlobService
    {
        // Retrieve all the documents in the storage blob container as BlobItems
        Task<List<BlobItem>> GetDocuments();

        // Get the bytes of a document by its passed in name
        Task<byte[]> GetDocumentBytes(string docName);
    }
}
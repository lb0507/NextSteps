/* 
*    IContactService.cs
*    3/28/2026
*    ======================================
*    - Initial creation
*    ======================================
*    Interface for Contact related Database calls
*   
*/

using DataLibrary.Models;
using Microsoft.Data.SqlClient;

namespace DataLibrary.ServiceLayer.ContactService
{
    public interface IContactService
    {
        // Get a Contact by its Guid
        Task<Contact?> GetContact(Guid contactId);

        // Get all Contacts associated with the current user and funeral is applicable
        Task<List<Contact>> GetContacts(Guid userId, Guid funeralId);

        // Get Contacts that are Vendors
        Task<List<Contact>> GetVendorContacts(Guid userId, Guid funeralId);

        // Create a new Contact object
        Task<Contact?> CreateContact(Contact contact);

        // Update a Contact
        Task<Contact?> UpdateContact(Contact contact);

        // Delete a Contact
        Task<bool> DeleteContact(Guid contactId);

        // Helper method for reading nullable string columns
        string? HanldeGetString(SqlDataReader reader, string columnName);

        // Helper method for reading nullable DateTime columns
        DateTime? HanldeGetDateTime(SqlDataReader reader, string columnName);
    }
}
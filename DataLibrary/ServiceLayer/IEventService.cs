/* 
*    IEventService.cs
*    4/18/2026
*    ======================================
*    - Initial creation
*    ======================================
*    Interface for Event related Database calls
*   
*/

using DataLibrary.Models;
using Microsoft.Data.SqlClient;

namespace DataLibrary.ServiceLayer.EventService
{
    public interface IEventService
    {
        // Get all Events associated with the current user and funeral is applicable
        Task<List<Event>> GetEvents(Guid userId, Guid funeralId);

        // Check for existing events that overlap with the time range of a new event
        Task<List<Event>> CheckForConflict(DateTime start, DateTime end, Guid userId, Guid eventId);

        // Create a new Event
        Task<Event?> CreateEvent(Event evnt);

        // Update an existing Event
        Task<Event?> UpdateEvent(Event evnt);
        
        // Delete an existing Event
        Task<bool> DeleteEvent(Guid eventId);

        // Helper method for reading nullable string columns
        string? HandleGetString(SqlDataReader reader, string columnName);

        // Helper method for reading nullable DateTime columns
        DateTime? HandleGetDateTime(SqlDataReader reader, string columnName);

        // Helper method for reading nullable Guid columns
        Guid? HandleGetGuid(SqlDataReader reader, string columnName);

        // Helper method for reading nullable integer columns
        int? HandleGetInt(SqlDataReader reader, string columnName);
    }
}
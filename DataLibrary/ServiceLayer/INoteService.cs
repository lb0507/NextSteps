/* 
*    INoteService.cs
*    3/21/2026
*    ======================================
*    - Initial creation
*    - Added Note Update and Deletion
*    ======================================
*    Interface for Note related Database calls
*   
*/

using DataLibrary.Models;
using Microsoft.Data.SqlClient;

namespace DataLibrary.ServiceLayer.NoteService
{
    public interface INoteService
    {
        // Get all non-deleted Notes attached to an Object
        Task<List<Note>> GetNotes(Guid objectId);

        // Create and attach a new note to an Object
        Task<Note?> CreateNote(Note note);

        // Delete a note
        Task<bool> DeleteNote(Guid noteId);
    }
}
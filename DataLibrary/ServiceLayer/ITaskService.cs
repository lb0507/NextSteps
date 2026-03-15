/* 
*    ITaskService.cs
*    3/14/2026
*    ======================================
*    - Initial creation
*    ======================================
*    Interface for Task related Database calls
*   
*/

using DataLibrary.Models;
using Microsoft.Data.SqlClient;

namespace DataLibrary.ServiceLayer.TaskService
{
    public interface ITaskService
    {
        // Get a Task by its TaskId
        Task<NsTask?> GetTask(Guid id);

        // Get all Tasks for a specific category and Funeral
        Task<List<NsTask>> GetTasksByCategory(Guid funeral, string category);

        // Create a new Task or Update and existing Task
        Task<NsTask?> SaveTask(NsTask task);

        // Helper method for reading nullable string columns
        string? HanldeGetString(SqlDataReader reader, string columnName);

        // Helper method for reading nullable DateTime columns
        DateTime? HanldeGetDateTime(SqlDataReader reader, string columnName);
    }
}
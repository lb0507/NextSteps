/* 
*    ITaskService.cs
*    3/21/2026
*    ======================================
*    - Initial creation
*    - Added Task Update and Deletion
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

        // Get the number of completed tasks and the number of total tasks for a category
        Task<(int, int)> GetCountOfTasksByFuneral(Guid funeral);

        // Create a new Task 
        Task<NsTask?> CreateTask(NsTask task);

        // Save changes to an existing Task in the database
        Task<NsTask?> UpdateTask(NsTask task);

        // Delete a Task
        Task<bool> DeleteTask(Guid taskId);

        // Helper method for reading nullable string columns
        string? HandleGetString(SqlDataReader reader, string? columnName = null, int colIdx = 0);

        // Helper method for reading nullable DateTime columns
        DateTime? HandleGetDateTime(SqlDataReader reader, string? columnName = null, int colIdx = 0);
    }
}
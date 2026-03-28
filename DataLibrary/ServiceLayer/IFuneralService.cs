/* 
*    IFuneralService.cs
*    3/28/2026
*    ======================================
*    - Initial creation
*    - Added CreateFuneral
*    - Added Update and Delete
*    ======================================
*    Interface for Funeral related Database calls
*   
*/

using DataLibrary.Models;
using Microsoft.Data.SqlClient;

namespace DataLibrary.ServiceLayer.FuneralService
{
    public interface IFuneralService
    {
        // Get all Funerals associated with the current user
        Task<List<Funeral>> GetFunerals(Guid userId);

        // Create a new Funeral object
        Task<Funeral?> CreateFuneral(Funeral funeral);

        // Update Number of Tasks for a Funeral
        Task<Funeral?> UpdateTaskNumber(Funeral funeral);

        // Update a Funeral
        Task<Funeral?> UpdateFuneral(Funeral funeral);

        // Delete a Funeral
        Task<bool> DeleteFuneral(Guid funeralId);

        // Helper method for reading nullable string columns
        string? HanldeGetString(SqlDataReader reader, string columnName);

        // Helper method for reading nullable DateTime columns
        DateTime? HanldeGetDateTime(SqlDataReader reader, string columnName);
    }
}

/* 
*    FuneralService.cs
*    3/15/2026
*    ======================================
*    - Initial creation
*    - Added CreateFuneral
*    ======================================
*    Service Layer for Funeral related Database calls
*   
*/

using DataLibrary.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DataLibrary.ServiceLayer.FuneralService
{
    public class FuneralService : IFuneralService
    {
        // Store the current selected Funeral data
        public Funeral? current_funeral { get; private set; }

        private readonly IConfiguration _config;

        public FuneralService(IConfiguration config)
        {
            _config = config;
        }

        // Get all Funerals associated with the current user
        public async Task<List<Funeral>> GetFunerals(Guid userId)
        {
            var funerals = new List<Funeral>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("AzureSql")))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Funerals WHERE UserId = @UserId", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var funeral = new Funeral
                                {
                                    FuneralId = reader.GetGuid(reader.GetOrdinal("FuneralId")),
                                    DeceasedName = reader.GetString(reader.GetOrdinal("DeceasedName")),
                                    Location = HanldeGetString(reader, "Location"),
                                    DateOfService = HanldeGetDateTime(reader, "DateOfService"),
                                    NumberOfTasks = reader.GetInt32(reader.GetOrdinal("NumberOfTasks")),
                                    UserId = reader.GetGuid(reader.GetOrdinal("UserId"))
                                };
                                funerals.Add(funeral);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return funerals;
        }


        // Create a new Funeral object
        public async Task<Funeral?> CreateFuneral(Funeral funeral)
        {
            funeral.FuneralId = Guid.NewGuid(); // Generate a new unique ID for the funeral
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("AzureSql")))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Funerals (FuneralId, DeceasedName, UserId, NumberOfTasks) VALUES (@FuneralId, @DeceasedName, @UserId, 0)", conn);

                    cmd.Parameters.AddWithValue("@FuneralId", funeral.FuneralId);
                    cmd.Parameters.AddWithValue("@DeceasedName", funeral.DeceasedName);
                    cmd.Parameters.AddWithValue("@UserId", funeral.UserId);

                    int rowsInserted = cmd.ExecuteNonQuery();

                    if (rowsInserted > 0)
                    {
                        return funeral;
                    }
                    else
                        return null;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return null;
        }


        // Update Number of Tasks for a Funeral
        public async Task<Funeral?> UpdateTaskNumber(Funeral funeral)
        {
            funeral.NumberOfTasks++; // Increment the number of tasks by 1
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("AzureSql")))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE Funerals SET NumberOfTasks = @numberOfTasks WHERE FuneralId = @funeralId", conn);

                    cmd.Parameters.AddWithValue("@funeralId", funeral.FuneralId);
                    cmd.Parameters.AddWithValue("@numberOfTasks", funeral.NumberOfTasks);

                    int rowsInserted = cmd.ExecuteNonQuery();

                    if (rowsInserted > 0)
                        return funeral;
                    else
                        return null;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return null;
        }


        // Update a Funeral -- WIP
        //public async Task<Funeral?> UpdateFuneral(Funeral funeral)
        //{
        //    try
        //    {
        //        using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("AzureSql")))
        //        {
        //            conn.Open();
        //            SqlCommand cmd = new SqlCommand(
        //                "UPDATE Funerals SET NumberOfTasks = @numberOfTasks WHERE FuneralId = @funeralId", conn);

        //            cmd.Parameters.AddWithValue("@funeralId", funeral.FuneralId);
        //            cmd.Parameters.AddWithValue("@numberOfTasks", funeral.NumberOfTasks);

        //            int rowsInserted = cmd.ExecuteNonQuery();

        //            if (rowsInserted > 0)
        //                return funeral;
        //            else
        //                return null;
        //        }
        //    }
        //    catch (SqlException ex)
        //    {
        //        Console.WriteLine("SQL Error: " + ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("An error occurred: " + ex.Message);
        //    }

        //    return null;
        //}


        // Return the current selected Funeral
        public Funeral? GetFuneralInfo() => current_funeral;

        // Generic Helper methods for null value handline
        public string? HanldeGetString(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
                return reader.GetString(reader.GetOrdinal(columnName));
            return null;
        }

        public DateTime? HanldeGetDateTime(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
                return reader.GetDateTime(reader.GetOrdinal(columnName));
            return null;
        }
    }
}
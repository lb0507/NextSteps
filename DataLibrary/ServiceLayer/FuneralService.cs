/* 
*    FuneralService.cs
*    3/28/2026
*    ======================================
*    - Initial creation
*    - Added CreateFuneral
*    - Added Update and Delete
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
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Funerals WHERE UserId = @UserId AND IsDeleted = 0", conn))
                    {
                        // Add the parameters values to the query
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var funeral = new Funeral
                                {
                                    FuneralId = reader.GetGuid(reader.GetOrdinal("FuneralId")),
                                    DeceasedName = reader.GetString(reader.GetOrdinal("DeceasedName")),
                                    Location = HandleGetString(reader, "Location"),
                                    DateOfService = HandleGetDateTime(reader, "DateOfService"),
                                    NumberOfTasks = reader.GetInt32(reader.GetOrdinal("NumberOfTasks")),
                                    UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                                    IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted")),
                                    Obituary = HandleGetString(reader, "Obituary"),
                                    IsArchived = reader.GetBoolean(reader.GetOrdinal("IsArchived"))
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
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Funerals (FuneralId, DeceasedName, UserId, NumberOfTasks, IsDeleted) VALUES (@FuneralId, @DeceasedName, @UserId, 0, 0)", conn);

                    // Add the parameters values to the query
                    cmd.Parameters.AddWithValue("@FuneralId", funeral.FuneralId);
                    cmd.Parameters.AddWithValue("@DeceasedName", funeral.DeceasedName);
                    cmd.Parameters.AddWithValue("@UserId", funeral.UserId);

                    // Execute the query and check if a row was affected
                    if (await cmd.ExecuteNonQueryAsync() > 0)
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


        // Update Number of Tasks for a Funeral
        public async Task<Funeral?> UpdateTaskNumber(Funeral funeral)
        {
            funeral.NumberOfTasks++; // Increment the number of tasks by 1
            try
            {
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE Funerals SET NumberOfTasks = @numberOfTasks WHERE FuneralId = @funeralId", conn);

                    // Add the parameters values to the query
                    cmd.Parameters.AddWithValue("@funeralId", funeral.FuneralId);
                    cmd.Parameters.AddWithValue("@numberOfTasks", funeral.NumberOfTasks);

                    // Execute the query and check if a row was affected
                    if (await cmd.ExecuteNonQueryAsync() > 0)
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


        // Update a Funeral
        public async Task<Funeral?> UpdateFuneral(Funeral funeral)
        {
            try
            {
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand
                    (
                        "UPDATE Funerals SET DeceasedName = @DeceasedName, DateOfService = @DateOfService, Location = @Location, Obituary = @Obituary, IsArchived = @IsArchived WHERE FuneralId = @FuneralId",
                        conn
                    );

                    // Add parameters to the query
                    cmd.Parameters.AddWithValue("@DeceasedName", funeral.DeceasedName);
                    cmd.Parameters.AddWithValue("@DateOfService", (funeral.DateOfService is null) ? DBNull.Value : funeral.DateOfService);
                    cmd.Parameters.AddWithValue("@Location", string.IsNullOrEmpty(funeral.Location) ? DBNull.Value : funeral.Location);
                    cmd.Parameters.AddWithValue("@Obituary", string.IsNullOrEmpty(funeral.Obituary) ? DBNull.Value : funeral.Obituary);
                    cmd.Parameters.AddWithValue("@IsArchived", funeral.IsArchived);
                    cmd.Parameters.AddWithValue("@FuneralId", funeral.FuneralId);

                    // Execute the query and check if a row was affected
                    if (await cmd.ExecuteNonQueryAsync() > 0)
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


        // Delete a Funeral
        public async Task<bool> DeleteFuneral(Guid funeralId)
        {
            try
            {
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE Funerals SET IsDeleted = 1 WHERE FuneralId = @FuneralId", conn);

                    // Add the parameters values to the query
                    cmd.Parameters.AddWithValue("@FuneralId", funeralId);

                    // Execute the query and check if a row was affected
                    if (await cmd.ExecuteNonQueryAsync() > 0)
                        return true;
                    else
                        return false;
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
            return false;
        }


        // Generic Helper methods for null value handlig
        public string? HandleGetString(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
                return reader.GetString(reader.GetOrdinal(columnName));
            return null;
        }

        public DateTime? HandleGetDateTime(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
                return reader.GetDateTime(reader.GetOrdinal(columnName));
            return null;
        }
    }
}
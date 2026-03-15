/* 
*    TaskService.cs
*    3/14/2026
*    ======================================
*    - Initial creation
*    ======================================
*    Service Layer for Task related Database calls
*   
*/

using DataLibrary.Models;
using DataLibrary.ServiceLayer.TaskService;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DataLibrary.ServiceLayer.FuneralService
{
    public class TaskService : ITaskService
    {
        private readonly IConfiguration _config;

        public TaskService(IConfiguration config)
        {
            _config = config;
        }

        // Get a Task by its TaskId
        public async Task<NsTask?> GetTask(Guid id)
        {
            var tasks = new List<NsTask>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("AzureSql")))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Tasks WHERE TaskId = @TaskId", conn))
                    {
                        cmd.Parameters.AddWithValue("@TaskId", id);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var task = new NsTask
                                {
                                    TaskId = reader.GetGuid(reader.GetOrdinal("TaskId")),
                                    TaskNumber = reader.GetString(reader.GetOrdinal("TaskNumber")),
                                    Description = HanldeGetString(reader, "Description"),
                                    Category = reader.GetString(reader.GetOrdinal("Category")),
                                    IsComplete = reader.GetBoolean(reader.GetOrdinal("IsComplete")),
                                    CreationDate = reader.GetDateTime(reader.GetOrdinal("CreationDate")),
                                    CompletionDate = HanldeGetDateTime(reader, "CompletionDate"),
                                    FuneralId = reader.GetGuid(reader.GetOrdinal("FuneralId"))
                                };
                                tasks.Add(task);
                            }
                            // Check that the Task was found
                            if (tasks.Any())
                                return tasks.FirstOrDefault();
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
            return null;
        }


        // Get all Tasks for a specific category and Funeral
        public async Task<List<NsTask>> GetTasksByCategory(Guid funeral, string category)
        {
            var tasks = new List<NsTask>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("AzureSql")))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Tasks WHERE FuneralId = @Funeral AND Category = @Category", conn))
                    {
                        cmd.Parameters.AddWithValue("@Funeral", funeral);
                        cmd.Parameters.AddWithValue("@Category", category);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var task = new NsTask
                                {
                                    TaskId = reader.GetGuid(reader.GetOrdinal("TaskId")),
                                    TaskNumber = reader.GetString(reader.GetOrdinal("TaskNumber")),
                                    Description = HanldeGetString(reader, "Description"),
                                    Category = reader.GetString(reader.GetOrdinal("Category")),
                                    IsComplete = reader.GetBoolean(reader.GetOrdinal("IsComplete")),
                                    CreationDate = reader.GetDateTime(reader.GetOrdinal("CreationDate")),
                                    CompletionDate = HanldeGetDateTime(reader, "CompletionDate"),
                                    FuneralId = reader.GetGuid(reader.GetOrdinal("FuneralId"))
                                };
                                tasks.Add(task);
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

            return tasks;
        }


        public async Task<NsTask?> SaveTask(NsTask task)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("AzureSql")))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand();

                    if (task.CompletionDate is null)
                        cmd = new SqlCommand("INSERT INTO Tasks (TaskId, TaskNumber, Description, Category, IsComplete, CreationDate, FuneralId) VALUES (@TaskId, @TaskNumber, @Description, @Category, @IsComplete, @CreationDate, @FuneralId)", conn);
                    else
                        cmd = new SqlCommand("INSERT INTO Tasks (TaskId, TaskNumber, Description, Category, IsComplete, CreationDate, CompletionDate, FuneralId) VALUES (@TaskId, @TaskNumber, @Description, @Category, @IsComplete, @CreationDate, @CompletionDate, @FuneralId)", conn);

                    cmd.Parameters.AddWithValue("@TaskId", task.TaskId);
                    cmd.Parameters.AddWithValue("@TaskNumber", task.TaskNumber);
                    cmd.Parameters.AddWithValue("@Description", task.Description);
                    cmd.Parameters.AddWithValue("@Category", task.Category);
                    cmd.Parameters.AddWithValue("@IsComplete", task.IsComplete);
                    cmd.Parameters.AddWithValue("@CreationDate", task.CreationDate);
                    if (task.CompletionDate is not null)
                        cmd.Parameters.AddWithValue("@CompletionDate", task.CompletionDate);
                    cmd.Parameters.AddWithValue("@FuneralId", task.FuneralId);

                    int rowsInserted = cmd.ExecuteNonQuery();

                    if (rowsInserted > 0)
                        return task;
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
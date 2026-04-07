/* 
*    TaskService.cs
*    3/21/2026
*    ======================================
*    - Initial creation
*    - Added Task Update and Deletion
*    ======================================
*    Service Layer for Task related Database calls
*   
*/

using DataLibrary.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DataLibrary.ServiceLayer.TaskService
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
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Tasks WHERE TaskId = @TaskId", conn))
                    {
                        // Add the parameters values to the query
                        cmd.Parameters.AddWithValue("@TaskId", id);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var task = new NsTask
                                {
                                    TaskId = reader.GetGuid(0),
                                    TaskNumber = reader.GetString(1),
                                    Description = HandleGetString(reader, colIdx:2),
                                    Category = reader.GetString(3),
                                    IsComplete = reader.GetBoolean(4),
                                    CreationDate = reader.GetDateTime(5),
                                    CompletionDate = HandleGetDateTime(reader, colIdx: 6),
                                    FuneralId = reader.GetGuid(7),
                                    IsDeleted = reader.GetBoolean(8)
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
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Tasks WHERE FuneralId = @Funeral AND Category = @Category AND IsDeleted = 0", conn))
                    {
                        // Add the parameters values to the query
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
                                    Description = HandleGetString(reader, "Description"),
                                    Category = reader.GetString(reader.GetOrdinal("Category")),
                                    IsComplete = reader.GetBoolean(reader.GetOrdinal("IsComplete")),
                                    CreationDate = reader.GetDateTime(reader.GetOrdinal("CreationDate")),
                                    CompletionDate = HandleGetDateTime(reader, "CompletionDate"),
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


        // Get the number of completed tasks and the number of total tasks for a category
        public async Task<(int, int)> GetCountOfTasksByFuneral(Guid funeral)
        {
            var taskCounts = (0, 0);
            try
            {
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand
                            (
                            "SELECT COUNT( CASE WHEN FuneralId = @FuneralId AND IsDeleted = 0 AND IsComplete = 1 THEN 1 END ) AS complete_tasks, COUNT( CASE WHEN FuneralId = @FuneralId AND IsDeleted = 0 THEN 1 END ) AS total_tasks FROM Tasks;", 
                            conn))
                    {
                        // Add the parameters values to the query
                        cmd.Parameters.AddWithValue("@FuneralId", funeral);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                // Get the count of completed tasks
                                taskCounts.Item1 = reader.GetInt32(0);
                                // Get the count of total tasks
                                taskCounts.Item2 = reader.GetInt32(1);
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

            return taskCounts;
        }


        // Add a new Task to the database
        public async Task<NsTask?> CreateTask(NsTask task)
        {
            try
            {
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();

                    SqlCommand cmd = new SqlCommand();

                    if (task.CompletionDate is null)
                        cmd = new SqlCommand("INSERT INTO Tasks (TaskId, TaskNumber, Description, Category, IsComplete, CreationDate, FuneralId, IsDeleted) VALUES (@TaskId, @TaskNumber, @Description, @Category, @IsComplete, @CreationDate, @FuneralId, 0)", conn);
                    else
                        cmd = new SqlCommand("INSERT INTO Tasks (TaskId, TaskNumber, Description, Category, IsComplete, CreationDate, CompletionDate, FuneralId, IsDeleted) VALUES (@TaskId, @TaskNumber, @Description, @Category, @IsComplete, @CreationDate, @CompletionDate, @FuneralId, 0)", conn);

                    // Add the parameters values to the query
                    cmd.Parameters.AddWithValue("@TaskId", task.TaskId);
                    cmd.Parameters.AddWithValue("@TaskNumber", task.TaskNumber);
                    cmd.Parameters.AddWithValue("@Description", task.Description);
                    cmd.Parameters.AddWithValue("@Category", task.Category);
                    cmd.Parameters.AddWithValue("@IsComplete", task.IsComplete);
                    cmd.Parameters.AddWithValue("@CreationDate", task.CreationDate);
                    if (task.CompletionDate is not null)
                        cmd.Parameters.AddWithValue("@CompletionDate", task.CompletionDate);
                    cmd.Parameters.AddWithValue("@FuneralId", task.FuneralId);

                    // Execute the query and check if a row was affected
                    if (await cmd.ExecuteNonQueryAsync() > 0)
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


        // Save changes to an existing Task in the database
        public async Task<NsTask?> UpdateTask(NsTask task)
        {
            try
            {
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();

                    string cmdString = "UPDATE Tasks SET Description = @Description, IsComplete = @IsComplete";

                    if (task.CompletionDate is not null)
                        cmdString = cmdString + ", CompletionDate = @CompletionDate";

                    cmdString = cmdString + " WHERE TaskId = @TaskId";

                    SqlCommand cmd = new SqlCommand(cmdString, conn);

                    // Add the parameters values to the query
                    cmd.Parameters.AddWithValue("@Description", task.Description);
                    cmd.Parameters.AddWithValue("@IsComplete", task.IsComplete);
                    if (task.CompletionDate is not null)
                        cmd.Parameters.AddWithValue("@CompletionDate", task.CompletionDate);
                    cmd.Parameters.AddWithValue("@TaskId", task.TaskId);

                    // Execute the query and check if a row was affected
                    if (await cmd.ExecuteNonQueryAsync() > 0)
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


        // Delete a Task
        public async Task<bool> DeleteTask(Guid taskId)
        {
            try
            {
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE Tasks SET IsDeleted = 1 WHERE TaskId = @TaskId", conn);

                    // Add the parameters values to the query
                    cmd.Parameters.AddWithValue("@TaskId", taskId);

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


        #region [NULL HANDLING]
        // Generic Helper methods for null value handling

        // Helper method for reading nullable string columns
        public string? HandleGetString(SqlDataReader reader, string? columnName = null, int colIdx = 0)
        {
            int column = 0;

            // if the column name was passed, use it to get the column index
            if (columnName != null)
                column = reader.GetOrdinal(columnName);
            else
                column = colIdx;

            // Check if the column value is null
            if (!reader.IsDBNull(column))
                return reader.GetString(column);
            return null;
        }

        // Helper method for reading nullable DateTime columns
        public DateTime? HandleGetDateTime(SqlDataReader reader, string? columnName = null, int colIdx = 0)
        {
            int column = 0;

            // if the column name was passed, use it to get the column index
            if (columnName != null)
                column = reader.GetOrdinal(columnName);
            else
                column = colIdx;

            // Check if the column value is null
            if (!reader.IsDBNull(column))
                return reader.GetDateTime(column);
            return null;
        }
        #endregion
    }
}
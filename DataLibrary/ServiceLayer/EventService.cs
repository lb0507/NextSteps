/* 
*    EventService.cs
*    4/18/2026
*    ======================================
*    - Initial creation
*    ======================================
*    Service Layer for Event related Database calls
*   
*/

using DataLibrary.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DataLibrary.ServiceLayer.EventService
{
    public class EventService : IEventService
    {
        private readonly IConfiguration _config;

        public EventService(IConfiguration config)
        {
            _config = config;
        }


        // Get all Events associated with the current user and funeral if applicable
        public async Task<List<Event>> GetEvents(Guid userId, Guid funeralId)
        {
            var events = new List<Event>();
            try
            {
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Events WHERE UserId = @UserId AND IsDeleted = 0 AND (IsGlobal = 1 OR FuneralId = @FuneralId)", conn))
                    {
                        // Add the parameters values to the query
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@FuneralId", funeralId);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var evnt = new Event
                                {
                                    EventId = reader.GetGuid(reader.GetOrdinal("EventId")),
                                    Subject = reader.GetString(reader.GetOrdinal("Subject")),
                                    Location = HandleGetString(reader, "Location"),
                                    StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                                    EndTime = reader.GetDateTime(reader.GetOrdinal("EndTime")),
                                    Description = HandleGetString(reader, "Description"),
                                    IsAllDay = reader.GetBoolean(reader.GetOrdinal("IsAllDay")),
                                    RecurrenceRule = HandleGetString(reader, "RecurrenceRule"),
                                    RecurrenceException = HandleGetString(reader, "RecurrenceException"),
                                    RecurrenceID = HandleGetInt(reader, "RecurrenceID"),
                                    UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                                    IsGlobal = reader.GetBoolean(reader.GetOrdinal("IsGlobal")),
                                    FuneralId = HandleGetGuid(reader, "FuneralId"),
                                    IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted"))
                                };
                                events.Add(evnt);
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

            return events;
        }


        // Check for existing events that overlap with the time range of a new event
        public async Task<List<Event>> CheckForConflict(DateTime start, DateTime end, Guid userId, Guid eventId)
        {
            try
            {
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("SELECT Subject, StartTime, EndTime FROM Events WHERE StartTime <= @End AND EndTime >= @Start AND IsDeleted = 0 AND UserId = @userId AND EventId <> @eventId", conn);

                    // Add the parameters values to the query
                    cmd.Parameters.AddWithValue("@End", end);
                    cmd.Parameters.AddWithValue("@Start", start);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@EventId", eventId);

                    var events = new List<Event>();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var evnt = new Event
                            {
                                Subject = reader.GetString(reader.GetOrdinal("Subject")),
                                StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                                EndTime = reader.GetDateTime(reader.GetOrdinal("EndTime")),
                            };
                            events.Add(evnt);
                        }
                    }

                    return events;
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
            return [];
        }


        // Create a new Event
        public async Task<Event?> CreateEvent(Event evnt)
        {
            // Assign a unique Id to the new event object
            evnt.EventId = Guid.NewGuid();
            try
            {
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Events (EventId, Subject, Location, StartTime, EndTime, Description, IsAllDay, RecurrenceRule, RecurrenceException, RecurrenceID, UserId, IsGlobal, FuneralId, IsDeleted) " +
                        "VALUES (@EventId, @Subject, @Location, @StartTime, @EndTime, @Description, @IsAllDay, @RecurrenceRule, @RecurrenceException, @RecurrenceID, @UserId, @IsGlobal, @FuneralId, @IsDeleted)", conn);

                    // Add the parameters values to the query
                    cmd.Parameters.AddWithValue("@EventId", evnt.EventId);
                    cmd.Parameters.AddWithValue("@Subject", evnt.Subject);
                    cmd.Parameters.AddWithValue("@Location", string.IsNullOrEmpty(evnt.Location) ? DBNull.Value : evnt.Location);
                    cmd.Parameters.AddWithValue("@StartTime", evnt.StartTime);
                    cmd.Parameters.AddWithValue("@EndTime", evnt.EndTime);
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(evnt.Description) ? DBNull.Value : evnt.Description);
                    cmd.Parameters.AddWithValue("@IsAllDay", evnt.IsAllDay);
                    cmd.Parameters.AddWithValue("@RecurrenceRule", string.IsNullOrEmpty(evnt.RecurrenceRule) ? DBNull.Value : evnt.RecurrenceRule);
                    cmd.Parameters.AddWithValue("@RecurrenceException", string.IsNullOrEmpty(evnt.RecurrenceException) ? DBNull.Value : evnt.RecurrenceException);
                    cmd.Parameters.AddWithValue("@RecurrenceID", (evnt.RecurrenceID is null) ? DBNull.Value : evnt.RecurrenceID);
                    cmd.Parameters.AddWithValue("@UserId", evnt.UserId);
                    cmd.Parameters.AddWithValue("@IsGlobal", evnt.IsGlobal);
                    cmd.Parameters.AddWithValue("@FuneralId", (evnt.FuneralId is null) ? DBNull.Value : evnt.FuneralId);
                    cmd.Parameters.AddWithValue("@IsDeleted", evnt.IsDeleted);

                    // Execute the query and check if a row was affected
                    if (await cmd.ExecuteNonQueryAsync() > 0)
                        return evnt;
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


        // Update an existing Event
        public async Task<Event?> UpdateEvent(Event evnt)
        {
            try
            {
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE Events SET Subject = @Subject, Location = @Location, StartTime = @StartTime, EndTime = @EndTime, Description = @Description, IsAllDay = @IsAllDay, RecurrenceRule = @RecurrenceRule, RecurrenceException = @RecurrenceException, RecurrenceID = @RecurrenceID, IsGlobal = @IsGlobal, FuneralId = @FuneralId WHERE EventId = @EventId", conn);

                    // Add the parameters values to the query
                    cmd.Parameters.AddWithValue("@EventId", evnt.EventId);
                    cmd.Parameters.AddWithValue("@Subject", evnt.Subject);
                    cmd.Parameters.AddWithValue("@Location", string.IsNullOrEmpty(evnt.Location) ? DBNull.Value : evnt.Location);
                    cmd.Parameters.AddWithValue("@StartTime", evnt.StartTime);
                    cmd.Parameters.AddWithValue("@EndTime", evnt.EndTime);
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(evnt.Description) ? DBNull.Value : evnt.Description);
                    cmd.Parameters.AddWithValue("@IsAllDay", evnt.IsAllDay);
                    cmd.Parameters.AddWithValue("@RecurrenceRule", string.IsNullOrEmpty(evnt.RecurrenceRule) ? DBNull.Value : evnt.RecurrenceRule);
                    cmd.Parameters.AddWithValue("@RecurrenceException", string.IsNullOrEmpty(evnt.RecurrenceException) ? DBNull.Value : evnt.RecurrenceException);
                    cmd.Parameters.AddWithValue("@RecurrenceID", (evnt.RecurrenceID is null) ? DBNull.Value : evnt.RecurrenceID);
                    cmd.Parameters.AddWithValue("@IsGlobal", evnt.IsGlobal);
                    cmd.Parameters.AddWithValue("@FuneralId", (evnt.FuneralId is null) ? DBNull.Value : evnt.FuneralId);

                    // Execute the query and check if a row was affected
                    if (await cmd.ExecuteNonQueryAsync() > 0)
                        return evnt;
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


        // Delete an Event
        public async Task<bool> DeleteEvent(Guid eventId)
        {
            try
            {
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE Events SET IsDeleted = 1 WHERE EventId = @EventId", conn);

                    // Add the parameters values to the query
                    cmd.Parameters.AddWithValue("@EventId", eventId);

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


        // Generic Helper methods for null value handling

        // Handle reading null strings from database
        public string? HandleGetString(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
                return reader.GetString(reader.GetOrdinal(columnName));
            return null;
        }

        // Handle reading null datetimes from database
        public DateTime? HandleGetDateTime(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
                return reader.GetDateTime(reader.GetOrdinal(columnName));
            return null;
        }

        // Handle reading null guids from database
        public Guid? HandleGetGuid(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
                return reader.GetGuid(reader.GetOrdinal(columnName));
            return null;
        }

        // Handle reading null integers from database
        public int? HandleGetInt(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
                return reader.GetInt32(reader.GetOrdinal(columnName));
            return null;
        }
    }
}
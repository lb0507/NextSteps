/* 
*    NoteService.cs
*    3/21/2026
*    ======================================
*    - Initial creation
*    - Added Note Update and Deletion
*    ======================================
*    Service Layer for Note related Database calls
*   
*/

using DataLibrary.Models;
using DataLibrary.ServiceLayer.NoteService;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DataLibrary.ServiceLayer.NoteService
{
    public class NoteService : INoteService
    {
        private readonly IConfiguration _config;

        public NoteService(IConfiguration config)
        {
            _config = config;
        }


        // Get all non-deleted Notes attached to an Object
        public async Task<List<Note>> GetNotes(Guid objectId)
        {
            var notes = new List<Note>();
            try
            {
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Notes WHERE ObjectId = @ObjectId AND IsDeleted = 0", conn))
                    {
                        // Add the parameters values to the query
                        cmd.Parameters.AddWithValue("@ObjectId", objectId);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var note = new Note
                                {
                                    NoteId = reader.GetGuid(reader.GetOrdinal("NoteId")),
                                    NoteText = reader.GetString(reader.GetOrdinal("NoteText")),
                                    CreationDate = reader.GetDateTime(reader.GetOrdinal("CreationDate")),
                                    ObjectId = reader.GetGuid(reader.GetOrdinal("ObjectId")),
                                    IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted"))
                                };
                                notes.Add(note);
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

            return notes;
        }


        // Create and attach a new note to an Object
        public async Task<Note?> CreateNote(Note note)
        {
            try
            {
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Notes (NoteId, NoteText, CreationDate, ObjectId, IsDeleted) VALUES (@NoteId, @NoteText, @CreationDate, @ObjectId, @IsDeleted)", conn);

                    // Add the parameters values to the query
                    cmd.Parameters.AddWithValue("@NoteId", note.NoteId);
                    cmd.Parameters.AddWithValue("@NoteText", note.NoteText);
                    cmd.Parameters.AddWithValue("@CreationDate", note.CreationDate);
                    cmd.Parameters.AddWithValue("@ObjectId", note.ObjectId);
                    cmd.Parameters.AddWithValue("@IsDeleted", note.IsDeleted);

                    // Execute the query and check if a row was affected
                    if (await cmd.ExecuteNonQueryAsync() > 0)
                        return note;
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


        // Delete a note
        public async Task<bool> DeleteNote(Guid noteId)
        {
            try
            {
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE Notes SET IsDeleted = 1 WHERE NoteId = @NoteId", conn);

                    // Add the parameters values to the query
                    cmd.Parameters.AddWithValue("@NoteId", noteId);

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
    }
}

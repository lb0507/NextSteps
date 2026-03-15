using DataLibrary.Models;
using DataLibrary.ServiceLayer.NoteService;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

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
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("AzureSql")))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Notes WHERE ObjectId = @ObjectId AND IsDeleted = 0", conn))
                    {
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
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("AzureSql")))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Notes (NoteId, NoteText, CreationDate, ObjectId, IsDeleted) VALUES (@NoteId, @NoteText, @CreationDate, @ObjectId, @IsDeleted)", conn);

                    cmd.Parameters.AddWithValue("@NoteId", note.NoteId);
                    cmd.Parameters.AddWithValue("@NoteText", note.NoteText);
                    cmd.Parameters.AddWithValue("@CreationDate", note.CreationDate);
                    cmd.Parameters.AddWithValue("@ObjectId", note.ObjectId);
                    cmd.Parameters.AddWithValue("@IsDeleted", note.IsDeleted);
                    int rowsInserted = cmd.ExecuteNonQuery();

                    if (rowsInserted > 0)
                    {
                        return note;
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


        // Delete a note

    }
}

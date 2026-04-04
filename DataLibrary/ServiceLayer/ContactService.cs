/* 
*    ContactService.cs
*    3/28/2026
*    ======================================
*    - Initial creation
*    ======================================
*    Service Layer for Contact related Database calls
*   
*/

using DataLibrary.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DataLibrary.ServiceLayer.ContactService
{
    public class ContactService : IContactService
    {
        private readonly IConfiguration _config;

        public ContactService(IConfiguration config)
        {
            _config = config;
        }


        // Get a contact by its Guid
        public async Task<Contact?> GetContact(Guid contactId)
        {
            var contacts = new List<Contact>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("AzureSql")))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Contacts WHERE ContactId = @ContactId", conn))
                    {
                        cmd.Parameters.AddWithValue("@ContactId", contactId);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var contact = new Contact
                                {
                                    ContactId = reader.GetGuid(reader.GetOrdinal("ContactId")),
                                    ContactName = reader.GetString(reader.GetOrdinal("ContactName")),
                                    ContactType = reader.GetString(reader.GetOrdinal("ContactType")),
                                    ContactEmail = HanldeGetString(reader, "ContactEmail"),
                                    ContactPhone = HanldeGetString(reader, "ContactPhone"),
                                    Location = HanldeGetString(reader, "Location"),
                                    VendorType = HanldeGetString(reader, "VendorType"),
                                    UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                                    IsGlobal = reader.GetBoolean(reader.GetOrdinal("IsGlobal")),
                                    FuneralId = HanldeGetGuid(reader, "FuneralId"),
                                    IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted"))
                                };
                                contacts.Add(contact);
                            }
                            // Check that the Contact was found
                            if (contacts.Any())
                                return contacts.FirstOrDefault();
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


        // Get all Contacts associated with the current user and funeral is applicable
        public async Task<List<Contact>> GetContacts(Guid userId, Guid funeralId)
        {
            var contacts = new List<Contact>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("AzureSql")))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Contacts WHERE UserId = @UserId AND IsDeleted = 0 AND (IsGlobal = 1 OR FuneralId = @FuneralId)", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@FuneralId", funeralId);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var contact = new Contact
                                {
                                    ContactId = reader.GetGuid(reader.GetOrdinal("ContactId")),
                                    ContactName = reader.GetString(reader.GetOrdinal("ContactName")),
                                    ContactType = reader.GetString(reader.GetOrdinal("ContactType")),
                                    ContactEmail = HanldeGetString(reader, "ContactEmail"),
                                    ContactPhone = HanldeGetString(reader, "ContactPhone"),
                                    Location = HanldeGetString(reader, "Location"),
                                    VendorType = HanldeGetString(reader, "VendorType"),
                                    UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                                    IsGlobal = reader.GetBoolean(reader.GetOrdinal("IsGlobal")),
                                    FuneralId = HanldeGetGuid(reader, "FuneralId"),
                                    IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted"))
                                };
                                contacts.Add(contact);
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

            return contacts;
        }


        // Get Contacts that are Vendors
        public async Task<List<Contact>> GetVendorContacts(Guid userId, Guid funeralId)
        {
            var contacts = new List<Contact>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("AzureSql")))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Contacts WHERE UserId = @UserId AND IsDeleted = 0 AND ContactType = 'Vendor' AND (IsGlobal = 1 OR FuneralId = @FuneralId)", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@FuneralId", funeralId);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var contact = new Contact
                                {
                                    ContactId = reader.GetGuid(reader.GetOrdinal("ContactId")),
                                    ContactName = reader.GetString(reader.GetOrdinal("ContactName")),
                                    ContactType = reader.GetString(reader.GetOrdinal("ContactType")),
                                    ContactEmail = HanldeGetString(reader, "ContactEmail"),
                                    ContactPhone = HanldeGetString(reader, "ContactPhone"),
                                    Location = HanldeGetString(reader, "Location"),
                                    VendorType = HanldeGetString(reader, "VendorType"),
                                    UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                                    IsGlobal = reader.GetBoolean(reader.GetOrdinal("IsGlobal")),
                                    FuneralId = HanldeGetGuid(reader, "FuneralId"),
                                    IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted"))
                                };
                                contacts.Add(contact);
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

            return contacts;
        }


        // Create a new Contact object
        public async Task<Contact?> CreateContact(Contact contact)
        {
            contact.ContactId = Guid.NewGuid(); // Generate a new unique ID for the funeral
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("AzureSql")))
                {
                    
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Contacts (ContactId, ContactName, ContactType, ContactEmail, ContactPhone, Location, VendorType, UserId, IsGlobal, FuneralId, IsDeleted) " +
                        "VALUES (@ContactId, @ContactName, @ContactType, @ContactEmail, @ContactPhone, @Location, @VendorType, @UserId, @IsGlobal, @FuneralId, @IsDeleted)", conn);

                    cmd.Parameters.AddWithValue("@ContactId", contact.ContactId);
                    cmd.Parameters.AddWithValue("@ContactName", contact.ContactName);
                    cmd.Parameters.AddWithValue("@ContactType", contact.ContactType);
                    cmd.Parameters.AddWithValue("@ContactEmail", string.IsNullOrEmpty(contact.ContactEmail) ? DBNull.Value : contact.ContactEmail);
                    cmd.Parameters.AddWithValue("@ContactPhone", string.IsNullOrEmpty(contact.ContactPhone) ? DBNull.Value : contact.ContactPhone);
                    cmd.Parameters.AddWithValue("@Location", string.IsNullOrEmpty(contact.Location) ? DBNull.Value : contact.Location);
                    cmd.Parameters.AddWithValue("@VendorType", string.IsNullOrEmpty(contact.VendorType) ? DBNull.Value : contact.VendorType);
                    cmd.Parameters.AddWithValue("@UserId", contact.UserId);
                    cmd.Parameters.AddWithValue("@IsGlobal", contact.IsGlobal);
                    cmd.Parameters.AddWithValue("@FuneralId", (contact.FuneralId is null) ? DBNull.Value : contact.FuneralId);
                    cmd.Parameters.AddWithValue("@IsDeleted", contact.IsDeleted);

                    int rowsInserted = cmd.ExecuteNonQuery();

                    if (rowsInserted > 0)
                    {
                        return contact;
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


        // Update a Contact
        public async Task<Contact?> UpdateContact(Contact contact)
        {
            try
            {
                //using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("AzureSql")))
                //{
                //    conn.Open();

                //    string cmdString = "UPDATE Funerals SET DeceasedName = @DeceasedName";

                //    if (funeral.DateOfService is not null)
                //        cmdString = cmdString + ", DateOfService = @DateOfService";

                //    if (funeral.Location is not null)
                //        cmdString = cmdString + ", Location = @Location";

                //    cmdString = cmdString + " WHERE FuneralId = @FuneralId";

                //    SqlCommand cmd = new SqlCommand(cmdString, conn);

                //    // Add parameters to the query
                //    cmd.Parameters.AddWithValue("@DeceasedName", funeral.DeceasedName);
                //    if (funeral.DateOfService is not null)
                //        cmd.Parameters.AddWithValue("@DateOfService", funeral.DateOfService);
                //    if (funeral.Location is not null)
                //        cmd.Parameters.AddWithValue("@Location", funeral.Location);
                //    cmd.Parameters.AddWithValue("@FuneralId", funeral.FuneralId);

                //    int rowsInserted = cmd.ExecuteNonQuery();

                //    if (rowsInserted > 0)
                //        return funeral;
                //    else
                //        return null;
                //}
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


        // Delete a Contact
        public async Task<bool> DeleteContact(Guid contactId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("AzureSql")))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE Contacts SET IsDeleted = 1 WHERE ContactId = @ContactId", conn);

                    cmd.Parameters.AddWithValue("@ContactId", contactId);

                    if (cmd.ExecuteNonQuery() > 0)
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


        // Generic Helper methods for null value handline

        // Handle reading null strings from database
        public string? HanldeGetString(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
                return reader.GetString(reader.GetOrdinal(columnName));
            return null;
        }

        // Handle reading null datetimes from database
        public DateTime? HanldeGetDateTime(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
                return reader.GetDateTime(reader.GetOrdinal(columnName));
            return null;
        }

        // Handle reading null guids from database
        public Guid? HanldeGetGuid(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
                return reader.GetGuid(reader.GetOrdinal(columnName));
            return null;
        }
    }
}
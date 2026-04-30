/* 
*    UserService.cs
*    4/29/2026
*    ======================================
*    - Initial creation
*    - Added methods for users to manage their accounts
*    ======================================
*    Service Layer for User related Database calls
*   
*/

using DataLibrary.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;


namespace DataLibrary.ServiceLayer.UserService
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _config;

        public UserService(IConfiguration config)
        {
            _config = config;
        }


        // Validates input credentials and sets the current User
        public async Task<User?> LoginUser(string email, string password)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();

                    SqlCommand cmd = new SqlCommand("SELECT * FROM Users WHERE Email = @Email", conn);
                    cmd.Parameters.AddWithValue("@Email", email);

                    var users = new List<User>();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        // Get Users with matching email, there should be only one
                        while (await reader.ReadAsync())
                        {
                            var user = new User
                            {
                                UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                                FirstName= reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName= reader.GetString(reader.GetOrdinal("LastName")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Password = reader.GetString(reader.GetOrdinal("HashedPassword"))
                            };
                            users.Add(user);
                        }

                        // Check that a user with the email was found
                        if (users.Any())
                        {
                            var user = users.FirstOrDefault();
                            if (user != null)
                            {
                                // Check that the input password matches 
                                var hasher = new PasswordHasher<User>();
                                var result = hasher.VerifyHashedPassword(user, user.Password, password);
                                if (result == PasswordVerificationResult.Failed)
                                    return null;
                                else
                                {
                                    return user;
                                }                                   
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
            return null;
        }

        // Create a new User in the database
        public async Task<User?> RegisterUser(User user)
        {
            // Hash the password and generate the user's guid
            var hasher = new PasswordHasher<User>();
            var hashedPassword = hasher.HashPassword(user, user.Password);
            user.UserId = Guid.NewGuid();

            try
            {
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Users (UserId, FirstName, LastName, Email, HashedPassword) VALUES (@UserId, @FirstName, @LastName, @Email, @PasswordHash)", conn);

                    // Add the parameters values to the query
                    cmd.Parameters.AddWithValue("@UserId", user.UserId);
                    cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", string.IsNullOrEmpty(user.LastName) ? DBNull.Value : user.LastName);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);

                    // Execute the query and check if a row was affected
                    if (await cmd.ExecuteNonQueryAsync() > 0)
                        return user;  
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

        // Check for existing email addresses
        public async Task<bool> ValidateEmail(string email)
        {
            try
            {
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email = @Email", conn);

                    // Add the parameters values to the query
                    cmd.Parameters.AddWithValue("@Email", email);

                    var emailExists = await cmd.ExecuteScalarAsync();
                    
                    // Convert the result to an integer and check if a value was found
                    if (emailExists != null && (int)emailExists > 0)
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

        // Update User in the database
        public async Task<User?> UpdateUser(User user)
        {
            try
            {
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand
                    (
                        "UPDATE Users SET FirstName = @FirstName, LastName = @LastName, Email = @Email WHERE UserId = @UserId", 
                        conn
                    );

                    // Add the parameters values to the query
                    cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", user.LastName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@UserId", user.UserId);

                    // Execute the query and check if a row was affected
                    if (await cmd.ExecuteNonQueryAsync() > 0)
                        return user;
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

        // Change User's Password
        public async Task<bool> ChangePassword(string oldPassword, string newPassword, User user)
        {
            var hasher = new PasswordHasher<User>();

            // Verify that the old password the user entered is correct
            var result = hasher.VerifyHashedPassword(user, user.Password, oldPassword);
            if (result == PasswordVerificationResult.Failed)
                return false;
           
            // Hash the new password
            var hashedPassword = hasher.HashPassword(user, newPassword);

            try
            {
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand
                    (
                        "UPDATE Users SET HashedPassword = @PasswordHash WHERE UserId = @UserId",
                        conn
                    );

                    // Add the parameters values to the query
                    cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                    cmd.Parameters.AddWithValue("@UserId", user.UserId);

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

        // Delete a User
        public async Task<bool> DeleteUser(Guid userId)
        {
            try
            {
                // Get the database connection string from Azure Key Vault and establish connection
                using (SqlConnection conn = new SqlConnection(_config["DbConnectionString"]))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand
                    (
                        "DELETE FROM Users WHERE UserId = @UserId",
                        conn
                    );

                    // Add the parameters values to the query
                    cmd.Parameters.AddWithValue("@UserId", userId);

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

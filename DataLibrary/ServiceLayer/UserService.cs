/* 
*    UserService.cs
*    3/13/2026
*    ======================================
*    - Initial creation
*    ======================================
*    Service Layer for User related Database calls
*   
*/

using DataLibrary.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;


namespace DataLibrary.ServiceLayer.UserService
{
    public class UserService : IUserService
    {
        // Store the current user's data
        public User? current_user { get; private set; }

        private readonly IConfiguration _config;

        public UserService(IConfiguration config)
        {
            _config = config;
        }

        public void SetUser(User user)
        {
            current_user = user;
        }

        // Validates input credentials and sets the current User
        public async Task<User?> LoginUser(string email, string password)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("AzureSql")))
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
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("AzureSql")))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Users (UserId, FirstName, LastName, Email, HashedPassword) VALUES (@UserId, @FirstName, @LastName, @Email, @PasswordHash)", conn);

                    cmd.Parameters.AddWithValue("@UserId", user.UserId);
                    cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", user.LastName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);

                    int rowsInserted = cmd.ExecuteNonQuery();

                    if (rowsInserted > 0)
                    {
                        current_user = user; // Set the newly registered user as the current user
                        return user;
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

        // Check for existing emails
        public async Task<bool> ValidateEmail(string email)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("AzureSql")))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email = @Email", conn);
                    cmd.Parameters.AddWithValue("@Email", email);

                    int emailExists = (int)cmd.ExecuteScalar();
                    if (emailExists > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
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
            return false;
        }

        // Return the current user
        public User? GetUserSignOnInfo() => current_user;

        // Sign out the user
        public void SignOutUser() => current_user = new(); 
    }
}

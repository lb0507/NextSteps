/* 
*    IUserService.cs
*    4/29/2026
*    ======================================
*    - Initial creation
*    - Added methods for users to manage their accounts
*    ======================================
*    Interface for User related Database calls
*   
*/

using DataLibrary.Models;

namespace DataLibrary.ServiceLayer.UserService
{
    public interface IUserService
    {
        //Returns true if the email and password match a user in the database, false otherwise
        Task<User?> LoginUser(string email, string password);

        //Creates a new user in the database. Returns the user if successful
        Task<User?> RegisterUser(User user);
        
        //Checks if a passed in email address already exists in the database
        Task<bool> ValidateEmail(string email);

        // Update User in the database
        Task<User?> UpdateUser(User user);

        // Change User's Password
        Task<bool> ChangePassword(string oldPassword, string newPassword, User user);

        // Delete a User
        Task<bool> DeleteUser(Guid user);
    }
}

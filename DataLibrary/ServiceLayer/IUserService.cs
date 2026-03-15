/* 
*    IUserService.cs
*    3/13/2026
*    ======================================
*    - Initial creation
*    ======================================
*    Interface for User related Database calls
*   
*/

using DataLibrary.Models;

namespace DataLibrary.ServiceLayer.UserService
{
    public interface IUserService
    {
        User? current_user { get; }

        void SetUser(User user);

        //Returns true if the email and password match a user in the database, false otherwise
        Task<User?> LoginUser(string email, string password);

        //Creates a new user in the database. Returns the user if successful
        Task<User?> RegisterUser(User user);
        
        //Checks if a passed in email address already exists in the database
        Task<bool> ValidateEmail(string email);
    }
}

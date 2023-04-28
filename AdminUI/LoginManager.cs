using System;
using DataLayer;
using DataLayer.Models;

namespace AdminUI;
public class LoginManager
{   
    private static LoginManager? instance;
        public static LoginManager Instance
        {
            get
            {
                if (instance is null)
                {
                    instance = new LoginManager();
                }
                return instance;
            }
        }

    public User? LoggedUser { get; set; } = null;
    public bool IsLoggedIn => LoggedUser is not null;

    public bool Login(string email, string password, out string ErrorMessage)
    {
        ErrorMessage = String.Empty;
        if (email == String.Empty)
        {
            ErrorMessage = "Email is required";
            return false;
        }

        if (password == String.Empty)
        {
            ErrorMessage = "Password is required";
            return false;
        }

        using (var connection = new DruidConnection(GlobalConfig.ConnectionString))
        { 
            var users = connection.GetByProperty<User>("Email", email);
            if (users.Count == 0)
            {
                ErrorMessage = "No user with that email";
                return false;
            }

            if (users.Count > 1)   // UNREACHABLE
                throw new Exception("More than one user with that email");

            User user = users[0];

            if (user.Password != DruidCRUD.ToMD5(password))
            {
                ErrorMessage = "Incorrect password";
                return false;
            }

            LoggedUser = user;
            return true;
        }
    }

    public void Logout()
    {
        LoggedUser = null;
    }
}

namespace DataLayer.Models;

[DruidCRUD.TableName("users")]
public class User : IModel
{
    [DruidCRUD.PrimaryKey]
    public int? Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Address { get; set; }
    public bool IsAdmin { get; set; }

    public User(int id, string firstName, string lastName, string email, string password, string? address, bool isAdmin)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Password = password;
        Address = address ?? String.Empty;
        IsAdmin = isAdmin;
    }

    public User(string firstName, string lastName, string email, string password, string? address, bool isAdmin)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Password = password;
        Address = address ?? String.Empty;
        IsAdmin = isAdmin;
    }
    public User() 
    { 
        FirstName = String.Empty;
        LastName = String.Empty;
        Email = String.Empty;
        Password = String.Empty;
        Address = String.Empty;
        IsAdmin = false;
    }

    public override string ToString()
    {
        return $"User {FirstName} {LastName} - {Email}";
    }
}


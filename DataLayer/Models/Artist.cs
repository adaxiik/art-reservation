namespace DataLayer.Models;


[DruidCRUD.TableName("artists")]
public class Artist
{
    [DruidCRUD.PrimaryKey]
    public int? Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Bio { get; set; }

    public Artist(int id, string firstName, string lastName, string bio)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Bio = bio;
    }

    public Artist(string firstName, string lastName, string bio)
    {
        FirstName = firstName;
        LastName = lastName;
        Bio = bio;
    }

    public Artist()
    {
        FirstName = String.Empty;
        LastName = String.Empty;
        Bio = String.Empty;
    }

    public override string ToString()
    {
        return $"{FirstName} {LastName}";
    }
}
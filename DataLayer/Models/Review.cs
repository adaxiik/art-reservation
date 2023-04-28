namespace DataLayer.Models;

[DruidCRUD.TableName("reviews")]
public class Review : IModel
{
    [DruidCRUD.PrimaryKey]
    public int? Id { get; set; }
    [DruidCRUD.ForeignColumn]
    public Artwork Artwork { get; set; }
    [DruidCRUD.ForeignColumn]
    public User User { get; set; }
    public string Comment { get; set; }
    public int Rating { get; set; }
    public DateTime Date { get; set; }

    public Review(int id, Artwork artWork, User user, string comment, int rating, DateTime date)
    {
        Id = id;
        Artwork = artWork;
        User = user;
        Comment = comment;
        Rating = rating;
        Date = date;
    }

    public Review(Artwork artWork, User user, string comment, int rating, DateTime date)
    {
        Artwork = artWork;
        User = user;
        Comment = comment;
        Rating = rating;
        Date = date;
    }

    public Review()
    {
        Artwork = new Artwork();
        User = new User();
        Comment = String.Empty;
        Rating = 0;
        Date = DateTime.Now;
    }

    public override string ToString()
    {
        return $"Review rating {Rating} for {Artwork} by {User} on {Date}";
    }
}

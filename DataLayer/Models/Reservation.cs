namespace DataLayer.Models;

[DruidCRUD.TableName("reservations")]
public class Reservation : IModel
{
    [DruidCRUD.PrimaryKey]
    public int? Id { get; set; }
    [DruidCRUD.ForeignColumn]
    public Artwork Artwork { get; set; }
    [DruidCRUD.ForeignColumn]
    public User User { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public bool Returned { get; set; }
    [DruidCRUD.Ignore]
    public double TotalPrice { get => (DateTo - DateFrom).Days * this.Artwork.PricePerDay; }
    [DruidCRUD.Ignore]
    public bool IsExpired { get => DateTime.Now > DateTo; }
    [DruidCRUD.Ignore]
    public int DaysLeft { get => (DateTo - DateTime.Now).Days; }

    public Reservation(int id, Artwork artWork, User user, DateTime dateFrom, DateTime dateTo, bool returned)
    {
        this.Id = id;
        this.Artwork = artWork;
        this.User = user;
        this.DateFrom = dateFrom;
        this.DateTo = dateTo;
        this.Returned = returned;
    }

    public Reservation(Artwork artWork, User user, DateTime dateFrom, DateTime dateTo, bool returned)
    {
        this.Artwork = artWork;
        this.User = user;
        this.DateFrom = dateFrom;
        this.DateTo = dateTo;
        this.Returned = returned;
    }

    public Reservation()
    {
        this.Artwork = new Artwork();
        this.User = new User();
        this.DateFrom = DateTime.Now;
        this.DateTo = DateTime.Now;
    }

    public override string ToString()
    {
        return $"Reservation {Id} for {User} from {DateFrom} to {DateTo}";
    }
}
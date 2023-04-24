namespace DataLayer.Models;

[DruidCRUD.TableName("reservations")]
public class Reservation
{
    [DruidCRUD.PrimaryKey]
    public int? Id { get; set; }
    [DruidCRUD.ForeignColumn]
    public Artwork Artwork { get; set; }
    [DruidCRUD.ForeignColumn]
    public User User { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public double TotalPrice { get => (DateTo - DateFrom).Days * this.Artwork.PricePerDay; }

    public Reservation(int id, Artwork artWork, User user, DateTime dateFrom, DateTime dateTo)
    {
        this.Id = id;
        this.Artwork = artWork;
        this.User = user;
        this.DateFrom = dateFrom;
        this.DateTo = dateTo;
    }

    public Reservation(Artwork artWork, User user, DateTime dateFrom, DateTime dateTo)
    {
        this.Artwork = artWork;
        this.User = user;
        this.DateFrom = dateFrom;
        this.DateTo = dateTo;
    }
}
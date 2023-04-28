namespace DataLayer.Models;

[DruidCRUD.TableName("artworks")]
public class Artwork : IModel
{
    [DruidCRUD.PrimaryKey]
    public int? Id { get; set; }
    public string Title { get; set; }
    [DruidCRUD.ForeignColumn]
    public Artist Artist { get; set; }

    public ArtworkType Type { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public double PricePerDay { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime DateCreated { get; set; }


    public Artwork(int id
                , string title
                , Artist artist
                , ArtworkType type
                , string? imageUrl
                , string? description
                , double pricePerDay
                , bool isAvailable
                , DateTime dateCreated)
    {
        Id = id;
        Title = title;
        Artist = artist;
        Type = type;
        ImageUrl = imageUrl;
        Description = description;
        PricePerDay = pricePerDay;
        IsAvailable = isAvailable;
        DateCreated = dateCreated;
    }

    public Artwork(string title
                , Artist artist
                , ArtworkType type
                , string? imageUrl
                , string? description
                , double pricePerDay
                , bool isAvailable
                , DateTime dateCreated)
    {
        Title = title;
        Artist = artist;
        Type = type;
        ImageUrl = imageUrl;
        Description = description;
        PricePerDay = pricePerDay;
        IsAvailable = isAvailable;
        DateCreated = dateCreated;
    }

    public Artwork()
    {
        Title = String.Empty;
        Artist = new Artist();
        Type = ArtworkType.Painting;
        ImageUrl = String.Empty;
        Description = String.Empty;
        PricePerDay = 0;
        IsAvailable = true;
        DateCreated = DateTime.Now;
    }
    
    public override string ToString()
    {
        return $"{Title} - {Type}";
    }
}

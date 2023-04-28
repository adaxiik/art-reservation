using Avalonia;
using DataLayer;
using DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdminUI;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "filldata")
        {
            Console.WriteLine("Creating tables...");
            CreateTables();
            Console.WriteLine("Creating users...");
            CreateUsers();
            Console.WriteLine("Creating artists...");
            CreateArtists();
            Console.WriteLine("Creating artworks...");
            CreateArtworks();
            Console.WriteLine("Creating reservations...");
            CreateReservations();
            Console.WriteLine("Creating reviews...");
            CreateReviews();
            return;
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();


    private static void CreateTables()
    {
        using (var connection = new DruidConnection(GlobalConfig.ConnectionString))
        {
            connection.DropTableIfExists<Review>();
            connection.DropTableIfExists<Reservation>();
            connection.DropTableIfExists<Artwork>();
            connection.DropTableIfExists<Artist>();
            connection.DropTableIfExists<User>();

            connection.CreateTable<Review>();
            connection.CreateTable<Reservation>();
            connection.CreateTable<Artwork>();
            connection.CreateTable<Artist>();
            connection.CreateTable<User>();
        }
    }
    private static void CreateUsers()
    {
        List<User> users = new List<User>(){
            new User("Adam", "Dekadent", "a.a@aaa.cz", DruidCRUD.ToMD5("admin"), null, true),
            new User("Boris", "Morče", "b.morce@mail.com", DruidCRUD.ToMD5("morce"), null, false),
            new User("Cyril", "Krysa", "c.krysa@seznam.com", DruidCRUD.ToMD5("krysa"), null, false),
            new User("David", "Kočka", "d.kocka@gmail.cz", DruidCRUD.ToMD5("kocka"), null, false),
            new User("Eva", "Pes", "e.pes@mail.sk", DruidCRUD.ToMD5("pes"), null, false)
        };
        using (var connection = new DruidConnection(GlobalConfig.ConnectionString))
        {
            foreach (var user in users)
                connection.Insert(user);
        }
    }

    private static void CreateArtists()
    {
        List<Artist> artists = new List<Artist>
        {
            new Artist("Vincent", "van Gogh", "Vincent van Gogh was a Dutch post-impressionist painter who is among the most famous and influential figures in the history of Western art."),
            new Artist("Pablo", "Picasso", "Pablo Picasso was a Spanish painter, sculptor, printmaker, ceramicist and stage designer who spent most of his adult life in France."),
            new Artist("Leonardo", "da Vinci", "Leonardo da Vinci was an Italian polymath whose areas of interest included invention, painting, sculpting, architecture, science, music, mathematics, engineering, literature, anatomy, geology, astronomy, botany, writing, history, and cartography."),
            new Artist("Michelangelo", "Buonarroti", "Michelangelo di Lodovico Buonarroti Simoni was an Italian sculptor, painter, architect, and poet of the High Renaissance."),
            new Artist("Rembrandt", "van Rijn", "Rembrandt Harmenszoon van Rijn was a Dutch draughtsman, painter, and printmaker."),
            new Artist("Salvador", "Dali", "Salvador Domingo Felipe Jacinto Dalí i Domènech, 1st Marquess of Dalí de Púbol was a Spanish surrealist artist renowned for his technical skill, precise draftsmanship and the striking and bizarre images in his work."),
            new Artist("Claude", "Monet", "Oscar-Claude Monet was a French painter, a founder of French Impressionist painting and the most consistent and prolific practitioner of the movement's philosophy of expressing one's perceptions before nature, especially as applied to plein air landscape painting."),
            new Artist("Henri", "Matisse", "Henri Émile Benoît Matisse was a French artist, known for both his use of colour and his fluid and original draughtsmanship. He was a draughtsman, printmaker, and sculptor, but is known primarily as a painter."),
            new Artist("Georgia", "O'Keeffe", "Georgia Totto O'Keeffe was an American artist. She was known for her paintings of enlarged flowers, New York skyscrapers, and New Mexico landscapes."),
            new Artist("Edvard", "Munch", "Edvard Munch was a Norwegian painter. His best-known work, The Scream, has become one of Western art's most iconic images."),
        };

        using (var connection = new DruidConnection(GlobalConfig.ConnectionString))
        {
            foreach (var artist in artists)
                connection.Insert(artist);
        }
    }

    private static void CreateArtworks()
    {
        using (var connection = new DruidConnection(GlobalConfig.ConnectionString))
        {

            var artworks = new List<Artwork>
            {
                new Artwork(
                    title: "Starry Night",
                    artist: connection.GetByProperty<Artist>("LastName", "van Gogh").First(),
                    type: ArtworkType.Painting,
                    imageUrl: "https://upload.wikimedia.org/wikipedia/commons/thumb/e/ea/Van_Gogh_-_Starry_Night_-_Google_Art_Project.jpg/1280px-Van_Gogh_-_Starry_Night_-_Google_Art_Project.jpg",
                    description: "Starry Night is an oil painting by Dutch post-impressionist artist Vincent van Gogh. The painting depicts the view outside his sanatorium room window at night, although it was painted from memory during the day.",
                    pricePerDay: 50.0,
                    isAvailable: true,
                    dateCreated: new DateTime(1889, 6, 18)
                ),
                new Artwork(
                    title: "Guernica",
                    artist: connection.GetByProperty<Artist>("LastName", "Picasso").First(),
                    type: ArtworkType.Painting,
                    imageUrl: "https://upload.wikimedia.org/wikipedia/en/7/74/PicassoGuernica.jpg",
                    description: "Guernica is a large oil painting on canvas by Spanish artist Pablo Picasso completed in June 1937. It is considered one of the most powerful anti-war paintings in history.",
                    pricePerDay: 75.0,
                    isAvailable: true,
                    dateCreated: new DateTime(1937, 6, 18)
                ),
                new Artwork(
                title: "Mona Lisa",
                artist: connection.GetByProperty<Artist>("LastName", "da Vinci").First(),
                type: ArtworkType.Painting,
                imageUrl: "https://upload.wikimedia.org/wikipedia/commons/thumb/6/6a/Mona_Lisa.jpg/662px-Mona_Lisa.jpg",
                description: "The Mona Lisa is a half-length portrait painting by Italian artist Leonardo da Vinci. It is one of the most famous paintings in the world and is known for its enigmatic smile.",
                pricePerDay: 100.0,
                isAvailable: true,
                dateCreated: new DateTime(1503, 6, 18)
                ),
                new Artwork(
                    title: "David",
                    artist: connection.GetByProperty<Artist>("LastName", "Buonarroti").First(),
                    type: ArtworkType.Sculpture,
                    imageUrl: "https://upload.wikimedia.org/wikipedia/commons/thumb/7/7f/Michelangelo%27s_David_2015.jpg/480px-Michelangelo%27s_David_2015.jpg",
                    description: "David is a masterpiece of Renaissance sculpture created in marble by Italian artist Michelangelo. The statue represents the biblical hero David, who defeated the giant Goliath with a stone from his sling.",
                    pricePerDay: 125.0,
                    isAvailable: true,
                    dateCreated: new DateTime(1504, 6, 18)
                ),
                new Artwork(
                    title: "The Night Watch",
                    artist: connection.GetByProperty<Artist>("LastName", "van Rijn").First(),
                    type: ArtworkType.Painting,
                    imageUrl: "https://upload.wikimedia.org/wikipedia/commons/thumb/4/4b/Nachtwacht_Meister.jpg/1024px-Nachtwacht_Meister.jpg",
                    description: "The Night Watch is a painting by Dutch artist Rembrandt. It is one of his most famous works and depicts a group portrait of a militia company. It is notable for its use of dramatic lighting and composition.",
                    pricePerDay: 85.0,
                    isAvailable: true,
                    dateCreated: new DateTime(1642, 6, 18)
                ),
                new Artwork(
                    title: "The Persistence of Memory",
                    artist: connection.GetByProperty<Artist>("LastName", "Dali").First(),
                    type: ArtworkType.Painting,
                    imageUrl: "https://upload.wikimedia.org/wikipedia/en/9/93/The_Persistence_of_Memory.jpg",
                    description: "The Persistence of Memory is a painting by Spanish surrealist artist Salvador Dali. It is known for its surreal imagery, including melting watches and a distorted landscape.",
                    pricePerDay: 65.0,
                    isAvailable: true,
                    dateCreated: new DateTime(1931, 6, 18)
                ),
                new Artwork(
                    title: "Water Lilies",
                    artist: connection.GetByProperty<Artist>("LastName", "Monet").First(),
                    type: ArtworkType.Painting,
                    imageUrl: "https://upload.wikimedia.org/wikipedia/commons/thumb/b/bf/Monet_-_Water_Lilies_%281910%29.jpg/1024px-Monet_-_Water_Lilies_%281910%29.jpg",
                    description: "Water Lilies is a series of approximately 250 oil paintings by French impressionist artist Claude Monet. The paintings depict his flower garden at his home in Giverny, and are known for their use of light and color.",
                    pricePerDay: 90.0,
                    isAvailable: true,
                    dateCreated: new DateTime(1910, 6, 18)
                ),
                new Artwork(
                    title: "Les Demoiselles d'Avignon",
                    artist: connection.GetByProperty<Artist>("LastName", "Picasso").First(),
                    type: ArtworkType.Painting,
                    imageUrl: "https://upload.wikimedia.org/wikipedia/en/4/4c/Les_Demoiselles_d%27Avignon.jpg",
                    description: "Les Demoiselles d'Avignon is a large oil painting created in 1907 by the Spanish artist Pablo Picasso. It depicts five nude prostitutes from a brothel on Carrer d'Avinyó in Barcelona.",
                    pricePerDay: 70.0,
                    isAvailable: true,
                    dateCreated: new DateTime(1907, 6, 1)
                )
            };

            foreach (var artwork in artworks)
                connection.Insert(artwork);
        }
    }

    private static void CreateReservations()
    {
        using(var connection = new DruidConnection(GlobalConfig.ConnectionString))
        {
            List<Artwork> artworks = connection.GetAll<Artwork>();
            List<User> users = connection.GetAll<User>();
            Random rng = new Random(69);
            List<int> artworkIds = new List<int>();
            for(int i = 0; i < artworks.Count/2; i++)
            {
                int id = artworks[rng.Next(artworks.Count)].Id ?? throw new Exception("UNREACHABLE");
                while(artworkIds.Contains(id))
                    id = artworks[rng.Next(artworks.Count)].Id ?? throw new Exception("UNREACHABLE");
                
                artworkIds.Add(id);
                Artwork artwork = artworks.First(a => a.Id == id);

                User user = users[rng.Next(users.Count)];
                DateTime dateFrom = DateTime.UtcNow.AddDays(rng.Next(30));
                DateTime dateTo = dateFrom.AddDays(rng.Next(60));

                Reservation reservation = new Reservation(artwork, user, dateFrom, dateTo);
                artwork.IsAvailable = false;
                connection.Update(artwork);
                connection.Insert(reservation);
            }
        }
    }

    private static void CreateReviews()
    {
        using(var connection = new DruidConnection(GlobalConfig.ConnectionString))
        {
            List<Artwork> artworks = connection.GetAll<Artwork>();
            List<User> users = connection.GetAll<User>();
            Random rng = new Random(69);
            foreach(var artwork in artworks)
            {
                int numReviews = rng.Next(5);
                for(int i = 0; i < numReviews; i++)
                {
                    User user = users[rng.Next(users.Count)];
                    int rating = rng.Next(1, 6);
                    string comment = "Lorem ipsum dolor sit amet, consectetur";
                    Review review = new Review(artwork, user, comment, rating, DateTime.Now);
                    connection.Insert(review);
                }
            }
        }
    }
}


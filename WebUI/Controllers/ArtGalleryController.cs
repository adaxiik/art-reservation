using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebUI.Models;
using DataLayer.Models;
using DataLayer;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebUI.Controllers;

public class ArtGalleryController : Controller
{
    public ArtGalleryController()
    {
    }

    public IActionResult Index()
    {
        List<Artwork> artworks = new List<Artwork>();

        using (var connection = new DruidConnection(GlobalConfig.ConnectionString))
        {
            artworks = connection.GetAll<Artwork>();
        }

        ViewBag.artworks = artworks;

        return View();
    }

    public IActionResult Details(int id)
    {
        Artwork? artwork = new Artwork();
        List<Review> reviews = new List<Review>();
        using (var connection = new DruidConnection(GlobalConfig.ConnectionString))
        {
            artwork = connection.GetById<Artwork>(id);
            if (artwork == null)
                return RedirectToAction("Index");

            reviews = connection.GetByForeignObject<Review>(artwork);
        }

        ViewBag.artwork = artwork;
        ViewBag.reviews = reviews;

        return View();
    }

    public IActionResult Reserve(int id)
    {
        Artwork? artwork = new Artwork();
        using(var connection = new DruidConnection(GlobalConfig.ConnectionString))
        {
            artwork = connection.GetById<Artwork>(id);
            if (artwork == null)
                return RedirectToAction("Index");
            
        }
        ViewBag.artwork = artwork;
        return View();
    }

    [HttpPost]
    public IActionResult Reserve(ReservationViewModel model, int id)
    {
        Artwork? artwork = null;
        using(var connection = new DruidConnection(GlobalConfig.ConnectionString))
        {
            artwork = connection.GetById<Artwork>(id);
            if (artwork == null)
                return RedirectToAction("Index");
            
        }
        ViewBag.artwork = artwork;

        if (!ModelState.IsValid)
            return View();
        
        if (model.DateFrom > model.DateTo)
        {
            ModelState.AddModelError("DateFrom", "Date from must be before date to");
            return View();
        }
        if(model.DateFrom < DateTime.Now)
        {
            ModelState.AddModelError("DateFrom", "Date from must be in the future");
            return View();
        }

        if(model.DateTo < DateTime.Now)
        {
            ModelState.AddModelError("DateTo", "Date to must be in the future");
            return View();
        }

        artwork.IsAvailable = false;
        Reservation reservation = new Reservation(
            artWork: artwork,
            user: LoginManager.Instance.LoggedUser!,
            dateFrom: model.DateFrom,
            dateTo: model.DateTo,
            returned: false
        );

        using(var connection = new DruidConnection(GlobalConfig.ConnectionString))
        {
            connection.Update(artwork);
            connection.Insert(reservation);
        }
 
        return RedirectToAction("MyReservations","Account");
    }


    [HttpPost]
    public IActionResult CreateReview(ReviewViewModel model)
    {
        Artwork? artwork = null;
        List<Review> reviews = new List<Review>();

         if (!ModelState.IsValid)
            return View("Details", new { id = model.ArtworkId });
 
        using(var connection = new DruidConnection(GlobalConfig.ConnectionString))
        {
            artwork = connection.GetById<Artwork>(model.ArtworkId ?? throw new Exception("UNREACHABLE"));
            if (artwork == null)
                return RedirectToAction("Index");

            reviews = connection.GetByForeignObject<Review>(artwork);
        }
        ViewBag.artwork = artwork;
        ViewBag.reviews = reviews;

       
        
        Review review = new Review(
            artWork: artwork,
            user: LoginManager.Instance.LoggedUser!,
            rating: model.Rating ?? throw new Exception("UNREACHABLE"),
            comment: model.Comment ?? string.Empty,
            date: DateTime.Now
        );

        using(var connection = new DruidConnection(GlobalConfig.ConnectionString))
        {
            connection.Insert(review);
        }
 
        return RedirectToAction("Details", new { id = model.ArtworkId });
    }


    [HttpPost]
    public IActionResult ReturnArtwork(int id)
    {
        Reservation? reservation = new Reservation();
        using(var connection = new DruidConnection(GlobalConfig.ConnectionString))
        {
            reservation = connection.GetById<Reservation>(id);
            if (reservation == null)
                return RedirectToAction("MyReservations","Account");
            
        }

        reservation.Returned = true;
        reservation.Artwork.IsAvailable = true;
        using(var connection = new DruidConnection(GlobalConfig.ConnectionString))
        {
            connection.Update(reservation);
        }
        return RedirectToAction("MyReservations","Account");
    }

    [HttpPost]
    public IActionResult DeleteReview(ReviewViewModel model)
    {
        Artwork? artwork = null;
        List<Review> reviews = new List<Review>();


        using(var connection = new DruidConnection(GlobalConfig.ConnectionString))
        {
            artwork = connection.GetById<Artwork>(model.ArtworkId??throw new Exception("UNREACHABLE"));
            if (artwork == null)
                return RedirectToAction("Index");

            reviews = connection.GetByForeignObject<Review>(artwork);
        }
        ViewBag.artwork = artwork;
        ViewBag.reviews = reviews;
        
        Review? review = null;
        using(var connection = new DruidConnection(GlobalConfig.ConnectionString))
        {
            review = connection.GetById<Review>(model.ReviewId ?? throw new Exception("UNREACHABLE"));
            if (review == null)
                return RedirectToAction("Details", new { id = model.ArtworkId });

            if (review.User.Id != LoginManager.Instance.LoggedUser!.Id)
                return RedirectToAction("Details", new { id = model.ArtworkId });
            
            connection.Delete(review);
        }

        return RedirectToAction("Details", new { id = model.ArtworkId });
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!LoginManager.Instance.IsLoggedIn)
            context.Result = RedirectToAction("Login", "Account");

        base.OnActionExecuting(context);
    }
   
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

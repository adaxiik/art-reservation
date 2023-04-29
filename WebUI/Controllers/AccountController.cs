using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebUI.Models;
using DataLayer.Models;
using DataLayer;
using System.Text.RegularExpressions;

namespace WebUI.Controllers;

public class AccountController : Controller
{

    public AccountController()
    {
    }

    public IActionResult Login()
    {
        if (LoginManager.Instance.IsLoggedIn)
            return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        if(!ModelState.IsValid)
            return View(model);

        if (!LoginManager.Instance.Login(model.Email!, model.Password!, out string ErrorMessage))
        {
            ViewBag.ErrorMessage = ErrorMessage;
            return View(model);
        }
        
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        LoginManager.Instance.Logout();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(RegisterViewModel model)
    {
        if(!ModelState.IsValid)
            return View(model);

        if(model.Password.Length < 3)
        {
            ModelState.AddModelError("Password", "Password must be at least 3 characters");
            return View(model);
        }

        Regex mailRegex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", RegexOptions.IgnoreCase);
        if(!mailRegex.IsMatch(model.Email!))
        {
            ModelState.AddModelError("Email", "Invalid email");
            return View(model);
        }

        using(var connection = new DruidConnection(GlobalConfig.ConnectionString))
        {
            List<User> same_email = connection.GetByProperty<User>("Email", model.Email!);

            if(same_email.Count > 0)
            {
                ModelState.AddModelError("Email", "Email already in use");
                return View(model);
            }

            var user = new User(firstName: model.FirstName!
                                , lastName: model.LastName!
                                , email: model.Email!
                                , password: DruidCRUD.ToMD5(model.Password!)
                                , address: model.Address!
                                , isAdmin: false);

            connection.Insert(user);
        }
        LoginManager.Instance.Login(model.Email!, model.Password!, out string ErrorMessage);

        
        return RedirectToAction("Index", "Home");
    }

    public IActionResult MyReservations()
    {
        if (!LoginManager.Instance.IsLoggedIn)
            return RedirectToAction("Login");
        
        List<Reservation> reservations = new List<Reservation>();

        using(var connection = new DruidConnection(GlobalConfig.ConnectionString))
        {
            reservations = connection.GetByForeignObject<Reservation>(LoginManager.Instance.LoggedUser!);
        }

        ViewBag.reservations = reservations;
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

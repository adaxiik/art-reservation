using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DataLayer;
using DataLayer.Models;

namespace AdminUI;

public partial class AdminDashboard : Window
{
    public AdminDashboard()
    {
        InitializeComponent();
        var logged_as_name = this.FindControl<TextBlock>("LoggedAsName_");
        string first_name = LoginManager.Instance.LoggedUser!.FirstName;
        string last_name = LoginManager.Instance.LoggedUser!.LastName;
        logged_as_name.Text = $"{first_name} {last_name}";
    }


    void ManageUsersCommand(object sender, RoutedEventArgs e)
    {
        new Manage<User>(this).Show();
        this.Hide();
    }

    void ManageArtistsCommand(object sender, RoutedEventArgs e)
    {
        new Manage<Artist>(this).Show();
        this.Hide();
    }

    void ManageArtworksCommand(object sender, RoutedEventArgs e)
    {
        new Manage<Artwork>(this).Show();
        this.Hide();
    }

    void ManageReservationsCommand(object sender, RoutedEventArgs e)
    {
        new Manage<Reservation>(this).Show();
        this.Hide();
    }

    void ManageReviewsCommand(object sender, RoutedEventArgs e)
    {
        new Manage<Review>(this).Show();
        this.Hide();
    }

    void LogoutCommand(object sender, RoutedEventArgs e)
    {
        LoginManager.Instance.Logout();
        new Login().Show();
        this.Close();
    }
}
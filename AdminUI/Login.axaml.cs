using Avalonia.Controls;

namespace AdminUI;
using DataLayer.Models;
using DataLayer;
using System.Collections.Generic;
using System;
using Avalonia.Interactivity;


public partial class Login : Window
{
    public Login()
    {
        InitializeComponent();
        this.DataContext = this;
        ErrorMessage = this.FindControl<TextBlock>("ErrorMessage_");

        // Email = "t@t.cz";
        // Password = "aa";
    }

    public string Email { get; set; } = String.Empty;
    public string Password { get; set; } = String.Empty;

    public TextBlock ErrorMessage { get; set; }

    public void LoginCommand(object sender, RoutedEventArgs e)
    {
        Email = "t@t.cz";
        Password = "aa";
        LoginManager.Instance.Login(Email, Password, out string ErrorMess);
        if (LoginManager.Instance.IsLoggedIn)
        {
            new AdminDashboard().Show();
            this.Close();
        }
        else
        {
            ErrorMessage.Text = ErrorMess;
        }
    }
}
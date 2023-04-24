using Avalonia.Controls;

namespace AdminUI;
using DataLayer.Models;
using DataLayer;
using System.Collections.Generic;

public partial class MainWindow : Window
{
    public List<Artist> Artists { get; set; }
    public MainWindow()
    {
        InitializeComponent();
    
        using(var connection = new DruidConnection(GlobalConfig.ConnectionString))
        {
            Artists = connection.GetAll<Artist>();

        }
        DataContext = this;
    }
}
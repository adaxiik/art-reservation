using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DataLayer;
using DataLayer.Models;
namespace AdminUI;

public partial class ManageBase : Window
{
    public ManageBase()
    {
        InitializeComponent();
    }
}

public class Manage<T> : ManageBase where T : class, new()
{
    Window origin;
    bool IsModel(Type type)
    {
        return type.GetInterfaces().Contains(typeof(IModel));
    }

    List<T> items = new List<T>();
    List<T> deleted_items = new List<T>();

    public Manage(Window origin)
    {
        this.origin = origin;
        Closing += (_, _) =>
        {
            origin.Show();
        };

        InitializeComponent();

        this.Title = $"Manage {typeof(T).Name}s";
        this.FindControl<Button>("Back_").Click += BackCommand;
        this.FindControl<Button>("Export_").Click += ExportCommand;
        this.FindControl<Button>("Add_").Click += AddCommand;
        this.FindControl<Button>("Edit_").Click += EditCommand;
        this.FindControl<Button>("Delete_").Click += DeleteCommand;

        using (var connection = new DruidConnection(GlobalConfig.ConnectionString))
        {
            items = connection.GetAll<T>();
        }

        var dataGrid = this.FindControl<DataGrid>("DataGrid_");
        dataGrid.Columns.Clear();

        foreach (var property in typeof(T).GetProperties())
        {
            if (property.Name == "Id") continue;

            if (property.PropertyType == typeof(bool))
            {
                var column = new DataGridCheckBoxColumn();
                column.Header = property.Name;
                column.Binding = new Binding(property.Name);
                dataGrid.Columns.Add(column);
            }
            else if (IsModel(property.PropertyType))
            {
                var column = new DataGridTemplateColumn();
                column.Header = property.Name;
                column.CellTemplate = new FuncDataTemplate<object>((_, __) =>
                {
                    var button = new Button();
                    button.Content = "Open " + property.Name + "s";
                    var manageWindow = (ManageBase)Activator.CreateInstance(typeof(Manage<>).MakeGenericType(property.PropertyType), this)!;
                    button.Click += (_, __) =>
                    {
                       manageWindow.Show();
                       this.Hide();
                    };
                    return button;
                });
                dataGrid.Columns.Add(column);
            }
            else
            {
                var column = new DataGridTextColumn();
                column.Header = property.Name;
                column.Binding = new Binding(property.Name);
                dataGrid.Columns.Add(column);
            }
        }

        dataGrid.Items = items;
    }

    async void BackCommand(object? sender, RoutedEventArgs e)
    {
        if (deleted_items.Count > 0)
        {
            var result = await MsgBox.MessageBox.Show(this, "Do you want to save changes?", "Save changes", MsgBox.MessageBox.MessageBoxButtons.YesNoCancel);

            if (result == MsgBox.MessageBox.MessageBoxResult.Yes)
            {
                using (var connection = new DruidConnection(GlobalConfig.ConnectionString))
                {
                    foreach (var item in deleted_items)
                    {
                        connection.Delete(item);
                    }
                }
            }
            else if (result == MsgBox.MessageBox.MessageBoxResult.Cancel)
            {
                return;
            }
        }

        origin.Show();
        this.Close();
    }

    void ExportCommand(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    async void AddCommand(object? sender, RoutedEventArgs e)
    {
        var new_item = new T();
        var window = new EditWindow<T>(new_item, (T itm) => {
            using (var connection = new DruidConnection(GlobalConfig.ConnectionString))
            {
                connection.Insert(itm);
            }
        });
        await window.ShowDialog(this);

        items.Add(new_item);
        var dataGrid = this.FindControl<DataGrid>("DataGrid_");
        dataGrid.Items = null;
        dataGrid.Items = items;

        dataGrid.SelectedItem = new_item;
    }

    async void EditCommand(object? sender, RoutedEventArgs e)
    {
        var dataGrid = this.FindControl<DataGrid>("DataGrid_");
        var item = (T)dataGrid.SelectedItem!;
        if (item == null) return;

        var window = new EditWindow<T>(item,(T itm) => {
            using (var connection = new DruidConnection(GlobalConfig.ConnectionString))
            {
                connection.Update(itm);
            }
        });
        await window.ShowDialog(this);

        dataGrid.Items = null;
        dataGrid.Items = items;
    }

    void DeleteCommand(object? sender, RoutedEventArgs e)
    {
        var dataGrid = this.FindControl<DataGrid>("DataGrid_");
        var item = (T)dataGrid.SelectedItem!;
        if (item == null) return;

        items.Remove(item);
        dataGrid.Items = items;
        dataGrid.SelectedItem = null;

        if(!DruidCRUD.HasId(item)) return;

        deleted_items.Add(item);
    }

}
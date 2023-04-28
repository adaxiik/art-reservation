using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using DataLayer;
using DataLayer.Models;

namespace AdminUI;

public partial class EditWindowBase : Window
{
    public EditWindowBase()
    {
        InitializeComponent();
    }
}

public class EditWindow<T> : EditWindowBase where T : class, new()
{
    T item;
    public delegate void Command(T item);

    Command command;
    public EditWindow(T item, Command command)
    {
        this.item = item;
        this.command = command;
        InitializeComponent();
        this.Title = $"Edit {typeof(T).Name}";

        var stackPanel = this.FindControl<StackPanel>("MainPanel_");
        stackPanel.Children.Clear();

        stackPanel.Children.Add(new TextBlock() { Text = "EDIT " + typeof(T).Name, Margin = new Thickness(10)});

        CreateEditable(item, stackPanel);

        stackPanel.Children.Add(new TextBlock() { Text = " " });

        
        var buttons = new StackPanel();
        buttons.Orientation = Orientation.Horizontal;
        stackPanel.Children.Add(buttons);

        var button = new Button();
        button.Content = "Save";
        button.Click += ExecuteCommand;
        button.Margin = new Thickness(10);
        buttons.Children.Add(button);


        var button2 = new Button();
        button2.Content = "Cancel";
        button2.Click += CancelCommand;
        button2.Margin = new Thickness(10);
        buttons.Children.Add(button2);
    }

    private void CreateEditable(object itemm, StackPanel stackPanel)
    {
        var type = itemm.GetType();

        foreach (var property in type.GetProperties())
        {
            if (property.Name == "Id") continue;

            if(DruidCRUD.IsIgnored(property)) continue;

            if(property.PropertyType.GetInterfaces().Contains(typeof(IModel)))
            {
                var model_label = new TextBlock();
                model_label.Text = "Select " + property.Name;
                stackPanel.Children.Add(model_label);

                var model_dropdown = new ComboBox();
                var available_models = new List<object>();
                using (var connection = new DruidConnection(GlobalConfig.ConnectionString))
                {
                    available_models = connection.GetAll(property.PropertyType);
                }

                model_dropdown.Items = available_models;
                
                model_dropdown.SelectedItem = available_models.FirstOrDefault(x => DruidCRUD.AreSame(x, property.GetValue(itemm)!));
                model_dropdown.SelectionChanged += (sender, e) =>
                {
                    property.SetValue(itemm, model_dropdown.SelectedItem);
                };
                stackPanel.Children.Add(model_dropdown);


                continue;
            }

            if(DruidCRUD.IsEnum(property))
            {
                var enum_label = new TextBlock();
                enum_label.Text = "Select " + property.Name;
                stackPanel.Children.Add(enum_label);

                var enum_dropdown = new ComboBox();
                var available_enums = Enum.GetValues(property.PropertyType).Cast<object>().ToList();

                enum_dropdown.Items = available_enums;
                enum_dropdown.SelectedItem = property.GetValue(itemm);
                enum_dropdown.SelectionChanged += (sender, e) =>
                {
                    property.SetValue(itemm, enum_dropdown.SelectedItem);
                };
                stackPanel.Children.Add(enum_dropdown);
                            
                continue;
            }

            if (property.PropertyType == typeof(bool))
            {
                var bool_label = new CheckBox();
                bool_label.Content = property.Name;
                bool_label.IsChecked = (bool)property.GetValue(itemm)!;
                bool_label.Checked += (sender, e) =>
                {
                    property.SetValue(itemm, true);
                };
                bool_label.Unchecked += (sender, e) =>
                {
                    property.SetValue(itemm, false);
                };
                stackPanel.Children.Add(bool_label);
                continue;
            }

            var label = new TextBlock();
            label.Text = property.Name;
            stackPanel.Children.Add(label);

           var textBox = new TextBox();
            Binding binding = new Binding(property.Name);
            binding.Source = itemm;
            binding.Mode = BindingMode.TwoWay;
            textBox.Bind(TextBox.TextProperty, binding);
            stackPanel.Children.Add(textBox);
        }

    }

    private void ExecuteCommand(object? sender, RoutedEventArgs e)
    {
        command(item);
        this.Close();
    }

    private void CancelCommand(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
    
       

}
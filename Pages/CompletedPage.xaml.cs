using todolist_application.Services;
using todolist_application.Models;
using Microsoft.Maui.ApplicationModel;
using System.Linq;

namespace todolist_application.Pages;

public partial class CompletedPage : ContentPage
{
    public CompletedPage()
    {
        InitializeComponent();
        completedList.ItemsSource = DataService.CompletedItems;
    }

    private async void Item_Tapped(object sender, ItemTappedEventArgs e)
    {
        var item = e.Item as ToDoClass;

        // clear selection to avoid re-triggering when returning
        if (completedList != null)
            completedList.SelectedItem = null;

        await Navigation.PushAsync(new EditCompletedPage(item));
    }

    private async void CompletedList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection?.FirstOrDefault() as ToDoClass;

        // clear selection to avoid re-triggering when returning
        if (completedList != null)
            completedList.SelectedItem = null;

        if (item != null)
            await Navigation.PushAsync(new EditCompletedPage(item));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Clean up any items that had their status changed while this page was not active.
        // Use ToList() to avoid modifying the collection while enumerating.
        var moved = DataService.CompletedItems.ToList();
        foreach (var item in moved)
        {
            try
            {
                if (item.status == "Pending")
                {
                    if (!DataService.TodoItems.Contains(item))
                        DataService.TodoItems.Add(item);

                    if (DataService.CompletedItems.Contains(item))
                        DataService.CompletedItems.Remove(item);
                }
                else if (item.status == "Deleted")
                {
                    if (DataService.CompletedItems.Contains(item))
                        DataService.CompletedItems.Remove(item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CompletedPage OnAppearing cleanup exception: {ex}");
            }
        }
    }
    private async void Delete_Clicked(object sender, EventArgs e)
    {
        Image btn = sender as Image;
        ToDoClass item = btn?.BindingContext as ToDoClass;

        try
        {
            // Ask the user to confirm before deleting
            bool answer = await DisplayAlert("Delete Task", $"Are you sure you want to delete '{item.item_name}'?", "Yes", "No");

            if (answer)
            {
                await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (item != null)
                    {
                        if (DataService.CompletedItems.Contains(item))
                            DataService.CompletedItems.Remove(item);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Delete_Clicked exception: {ex}");
            _ = Application.Current?.MainPage?.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
using todolist_application.Services;
using todolist_application.Models;
using System.Linq;

namespace todolist_application.Pages;

public partial class CompletedPage : ContentPage
{
    public CompletedPage()
    {
        InitializeComponent();
        completedList.ItemsSource = DataService.CompletedItems;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCompletedItemsAsync();
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

    private async void Delete_Clicked(object sender, EventArgs e)
    {
        Image btn = sender as Image;
        ToDoClass item = btn?.BindingContext as ToDoClass;

        try
        {
            if (item is null)
            {
                return;
            }

            // Ask the user to confirm before deleting
            bool answer = await DisplayAlert("Delete Task", $"Are you sure you want to delete '{item.item_name}'?", "Yes", "No");

            if (answer)
            {
                var deleteResult = await ApiService.DeleteItemAsync(item.item_id);
                if (!deleteResult.IsSuccess)
                {
                    await DisplayAlert("Delete failed", deleteResult.Message, "OK");
                    return;
                }

                DataService.CompletedItems.Remove(item);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Delete_Clicked exception: {ex}");
            _ = Application.Current?.MainPage?.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task LoadCompletedItemsAsync()
    {
        if (!DataService.IsSignedIn || DataService.CurrentUser is null)
        {
            return;
        }

        try
        {
            var response = await ApiService.GetItemsAsync("inactive", DataService.CurrentUser.id);
            if (!response.IsSuccess)
            {
                await DisplayAlert("Load Failed", response.Message, "OK");
                return;
            }

            DataService.CompletedItems.Clear();
            foreach (var item in response.Items)
            {
                DataService.CompletedItems.Add(item);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
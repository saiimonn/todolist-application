
using todolist_application.Models;
using todolist_application.Services;

namespace todolist_application.Pages;

public partial class AddToDoPage : ContentPage
{
    public AddToDoPage()
    {
        InitializeComponent();
    }

    private async void Add_Clicked(object sender, EventArgs e)
    {
        var title = titleEntry.Text?.Trim() ?? string.Empty;
        var details = detailEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(details))
        {
            await DisplayAlert("Required", "Please enter both title and details.", "OK");
            return;
        }

        if (DataService.CurrentUser is null)
        {
            await DisplayAlert("Not signed in", "Please sign in before adding tasks.", "OK");
            return;
        }

        try
        {
            var response = await ApiService.AddItemAsync(title, details, DataService.CurrentUser.id);
            if (!response.IsSuccess || response.Item is null)
            {
                await DisplayAlert("Add failed", response.Message, "OK");
                return;
            }

            DataService.TodoItems.Add(response.Item);
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }

    }
}
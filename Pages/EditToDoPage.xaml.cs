using todolist_application.Models;
using todolist_application.Services;

namespace todolist_application.Pages;

public partial class EditToDoPage : ContentPage
{
    ToDoClass currentItem;

    public EditToDoPage(ToDoClass item)
    {
        InitializeComponent();

        currentItem = item;

        titleEntry.Text = item.item_name;
        detailEntry.Text = item.item_description;
    }

    private async void Update_Clicked(object sender, EventArgs e)
    {
        var title = titleEntry.Text?.Trim() ?? string.Empty;
        var details = detailEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(details))
        {
            await DisplayAlert("Required", "Please enter both title and details.", "OK");
            return;
        }

        var updateResponse = await ApiService.UpdateItemAsync(currentItem.item_id, title, details);
        if (!updateResponse.IsSuccess)
        {
            await DisplayAlert("Update failed", updateResponse.Message, "OK");
            return;
        }

        currentItem.item_name = title;
        currentItem.item_description = details;

        await Navigation.PopAsync();
    }

    private async void Complete_Clicked(object sender, EventArgs e)
    {
        var updateResponse = await ApiService.UpdateStatusAsync(currentItem.item_id, "inactive");
        if (!updateResponse.IsSuccess)
        {
            await DisplayAlert("Unable to complete", updateResponse.Message, "OK");
            return;
        }

        DataService.TodoItems.Remove(currentItem);

        currentItem.status = "inactive";
        DataService.CompletedItems.Add(currentItem);

        await Navigation.PopAsync();
    }

    private async void Delete_Clicked(object sender, EventArgs e)
    {
        var deleteResponse = await ApiService.DeleteItemAsync(currentItem.item_id);
        if (!deleteResponse.IsSuccess)
        {
            await DisplayAlert("Delete failed", deleteResponse.Message, "OK");
            return;
        }

        DataService.TodoItems.Remove(currentItem);

        await Navigation.PopAsync();
    }
}
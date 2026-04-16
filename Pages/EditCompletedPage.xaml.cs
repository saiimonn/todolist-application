using todolist_application.Models;
using todolist_application.Services;

namespace todolist_application.Pages;

public partial class EditCompletedPage : ContentPage
{
    private readonly ToDoClass currentItem;

    public EditCompletedPage(ToDoClass item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        InitializeComponent();

        currentItem = item;

        // Guard against designer/runtime nulls for the named controls
        if (titleEntry != null)
            titleEntry.Text = item.item_name ?? string.Empty;

        if (detailEntry != null)
            detailEntry.Text = item.item_description ?? string.Empty;
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

    private async void Incomplete_Clicked(object sender, EventArgs e)
    {
        var updateResponse = await ApiService.UpdateStatusAsync(currentItem.item_id, "active");
        if (!updateResponse.IsSuccess)
        {
            await DisplayAlert("Unable to update", updateResponse.Message, "OK");
            return;
        }

        DataService.CompletedItems.Remove(currentItem);
        currentItem.status = "active";
        DataService.TodoItems.Add(currentItem);

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

        DataService.CompletedItems.Remove(currentItem);

        await Navigation.PopAsync();
    }
}
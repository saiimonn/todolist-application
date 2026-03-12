using todolist_application.Models;
using todolist_application.Services;
using Microsoft.Maui.ApplicationModel;

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
        currentItem.item_name = titleEntry.Text;
        currentItem.item_description = detailEntry.Text;

        await Navigation.PopAsync();
    }

    private async void Incomplete_Clicked(object sender, EventArgs e)
    {
        // Avoid modifying the Completed collection while its renderers are active.
        // Instead update the item's status and let the CompletedPage handle moving it when it re-appears.
        if (currentItem != null)
        {
            currentItem.status = "Pending";
        }

        await Navigation.PopAsync();
    }

    private async void Delete_Clicked(object sender, EventArgs e)
    {
        // Mark the item as deleted and let the CompletedPage clean it up on re-appearing to avoid renderer races.
        if (currentItem != null)
        {
            currentItem.status = "Deleted";
        }

        await Navigation.PopAsync();
    }
}
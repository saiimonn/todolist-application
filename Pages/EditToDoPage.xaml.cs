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
        currentItem.item_name = titleEntry.Text;
        currentItem.item_description = detailEntry.Text;

        await Navigation.PopAsync();
    }

    private async void Complete_Clicked(object sender, EventArgs e)
    {
        DataService.TodoItems.Remove(currentItem);

        currentItem.status = "Completed";
        DataService.CompletedItems.Add(currentItem);

        await Navigation.PopAsync();
    }

    private async void Delete_Clicked(object sender, EventArgs e)
    {
        DataService.TodoItems.Remove(currentItem);

        await Navigation.PopAsync();
    }
}
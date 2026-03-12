
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
        ToDoClass newItem = new ToDoClass
        {
            item_name = titleEntry.Text,
            item_description = detailEntry.Text,
            status = "Pending"
        };

        DataService.TodoItems.Add(newItem);

        await Navigation.PopAsync();
    }
}
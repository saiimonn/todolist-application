namespace todolist_application.Pages;

using todolist_application.Services;
using todolist_application.Models;

public partial class ToDoPage : ContentPage
{
    public ToDoPage()
    {
        InitializeComponent();
        todoList.ItemsSource = DataService.TodoItems;
    }

    private async void Add_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddToDoPage());
    }

    private async void TodoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection?.FirstOrDefault() as ToDoClass;

        // clear selection to avoid re-triggering when returning
        if (todoList != null)
            todoList.SelectedItem = null;

        if (item != null)
            await Navigation.PushAsync(new EditToDoPage(item));
    }

    private async void Complete_Clicked(object sender, EventArgs e)
    {
        Image btn = sender as Image;
        ToDoClass item = btn?.BindingContext as ToDoClass;
        try
        {
            await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (item != null)
                {
                    if (DataService.TodoItems.Contains(item))
                        DataService.TodoItems.Remove(item);

                    item.status = "Completed";

                    if (!DataService.CompletedItems.Contains(item))
                        DataService.CompletedItems.Add(item);
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Complete_Clicked exception: {ex}");
            _ = Application.Current?.MainPage?.DisplayAlert("Error", ex.Message, "OK");
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
                        if (DataService.TodoItems.Contains(item))
                            DataService.TodoItems.Remove(item);
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
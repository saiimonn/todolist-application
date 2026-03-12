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
        Button btn = sender as Button;
        ToDoClass item = btn.BindingContext as ToDoClass;
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
}
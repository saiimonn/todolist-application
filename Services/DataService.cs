using System.Collections.ObjectModel;
using todolist_application.Models;

namespace todolist_application.Services
{
    public static class DataService
    {
        public static ObservableCollection<ToDoClass> TodoItems = new ObservableCollection<ToDoClass>();
        public static ObservableCollection<ToDoClass> CompletedItems = new ObservableCollection<ToDoClass>();
        public static UserInfo? CurrentUser { get; set; }

        public static bool IsSignedIn => CurrentUser is not null;

        public static void ClearSession()
        {
            CurrentUser = null;
            TodoItems.Clear();
            CompletedItems.Clear();
        }
    }
}
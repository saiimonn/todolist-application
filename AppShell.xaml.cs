using todolist_application.Pages;

namespace todolist_application;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(AddToDoPage), typeof(AddToDoPage));
        Routing.RegisterRoute(nameof(EditToDoPage), typeof(EditToDoPage));
        Routing.RegisterRoute(nameof(EditCompletedPage), typeof(EditCompletedPage));
        Routing.RegisterRoute(nameof(SignInPage), typeof(SignInPage));
        Routing.RegisterRoute(nameof(SignUpPage), typeof(SignUpPage));
    }
}
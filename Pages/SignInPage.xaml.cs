using todolist_application;

namespace todolist_application.Pages;

public partial class SignInPage : ContentPage
{
    public SignInPage()
    {
        InitializeComponent();
    }

    private void SignIn_Clicked(object sender, EventArgs e)
    {
        Application.Current.Windows[0].Page = new AppShell();
    }

    private async void SignUp_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignUpPage());
    }
}
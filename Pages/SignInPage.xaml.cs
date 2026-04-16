using todolist_application;
using todolist_application.Services;

namespace todolist_application.Pages;

public partial class SignInPage : ContentPage
{
    public SignInPage()
    {
        InitializeComponent();
    }

    private async void SignIn_Clicked(object sender, EventArgs e)
    {
        var email = emailEntry.Text?.Trim() ?? string.Empty;
        var password = passwordEntry.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Required", "Please provide your email and password.", "OK");
            return;
        }

        try
        {
            var result = await ApiService.SignInAsync(email, password);
            if (!result.IsSuccess || result.User is null)
            {
                await DisplayAlert("Sign In Failed", result.Message, "OK");
                return;
            }

            DataService.CurrentUser = result.User;
            Application.Current!.Windows[0].Page = new AppShell();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void SignUp_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignUpPage());
    }
}
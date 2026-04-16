using todolist_application.Services;

namespace todolist_application.Pages;

public partial class SignUpPage : ContentPage
{
	public SignUpPage()
	{
		InitializeComponent();
	}

	private async void Back_Clicked(object sender, EventArgs e)
	{
		await Navigation.PopAsync();

	}

    private async void SignUp_Clicked(object sender, EventArgs e)
    {
        var firstName = firstNameEntry.Text?.Trim() ?? string.Empty;
        var lastName = lastNameEntry.Text?.Trim() ?? string.Empty;
        var email = emailEntry.Text?.Trim() ?? string.Empty;
        var password = passwordEntry.Text ?? string.Empty;
        var confirmPassword = confirmPasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(confirmPassword))
        {
            await DisplayAlert("Required", "Please fill out all fields.", "OK");
            return;
        }

        try
        {
            var result = await ApiService.SignUpAsync(firstName, lastName, email, password, confirmPassword);
            if (!result.IsSuccess)
            {
                await DisplayAlert("Sign Up Failed", result.Message, "OK");
                return;
            }

            await DisplayAlert("Success", result.Message, "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
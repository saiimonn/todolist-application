using todolist_application.Services;

namespace todolist_application.Pages;

public partial class ProfilePage : ContentPage
{
	public ProfilePage()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var user = DataService.CurrentUser;
        nameLabel.Text = user is null ? "Not signed in" : $"{user.fname} {user.lname}";
        emailLabel.Text = user?.email ?? string.Empty;
    }

    private void SignOut_Clicked(object sender, EventArgs e)
    {
        DataService.ClearSession();
        Application.Current.MainPage = new NavigationPage(new SignInPage());
    }
}
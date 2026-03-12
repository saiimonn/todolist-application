namespace todolist_application.Pages;

public partial class ProfilePage : ContentPage
{
	public ProfilePage()
	{
		InitializeComponent();
	}
    private void SignOut_Clicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new NavigationPage(new SignInPage());
    }
}
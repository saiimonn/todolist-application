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
}
namespace todolist_application;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        // Global exception handlers to capture native Java exceptions (JavaProxyThrowable) and other unhandled errors.
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            HandleGlobalException(e.ExceptionObject);
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            HandleGlobalException(e.Exception);
        };
    }

    static void HandleGlobalException(object? exceptionObject)
    {
        try
        {
            if (exceptionObject is null)
                return;

            var ex = exceptionObject as Exception;
            string msg = ex?.ToString() ?? exceptionObject.ToString() ?? "<null>";

            // If this is a JavaProxyThrowable, try to reflect JavaStackTrace
            try
            {
                var t = exceptionObject.GetType();
                var prop = t.GetProperty("JavaStackTrace");
                if (prop != null)
                {
                    var javaTrace = prop.GetValue(exceptionObject) as string;
                    if (!string.IsNullOrEmpty(javaTrace))
                        msg += "\nJavaStackTrace:\n" + javaTrace;
                }
            }
            catch { }

            System.Diagnostics.Debug.WriteLine("Unhandled exception: " + msg);

            // Show a simple alert on the UI so it's visible during manual testing
            try
            {
                Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        await Application.Current?.MainPage?.DisplayAlert("Unhandled exception", ex?.Message ?? "See debug output", "OK");
                    }
                    catch { }
                });
            }
            catch { }
        }
        catch { }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(new Pages.SignInPage()));
    }
}
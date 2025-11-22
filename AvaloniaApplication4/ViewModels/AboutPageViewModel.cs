using System.Reflection;

namespace AvaloniaApplication4.ViewModels
{
    public class AboutPageViewModel : PageViewModelBase
    {
        public AboutPageViewModel()
        {
            Title = "About";
            Icon = "ℹ️";
        }

        public string AppName => "AvaloniaQR";
        public string AppVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        public string Description => "A modern, cross-platform QR code generator built with Avalonia UI";
        public string Copyright => "© 2024 PlashSpeed-Aiman";
        public string Framework => ".NET 6.0 with Avalonia UI";
        public string License => "MIT License";
    }
}

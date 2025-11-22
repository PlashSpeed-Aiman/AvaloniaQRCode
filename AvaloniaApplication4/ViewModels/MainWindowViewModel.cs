using ReactiveUI;
using System.Collections.ObjectModel;

namespace AvaloniaApplication4.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private PageViewModelBase _currentPage;

        public MainWindowViewModel()
        {
            // Initialize pages
            MainPage = new MainPageViewModel();
            SettingsPage = new SettingsPageViewModel();
            AboutPage = new AboutPageViewModel();

            // Set default page
            _currentPage = MainPage;

            // Create navigation items
            NavigationItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem { Label = "Home", Icon = "🏠", Page = MainPage },
                new NavigationItem { Label = "Settings", Icon = "⚙️", Page = SettingsPage },
                new NavigationItem { Label = "About", Icon = "ℹ️", Page = AboutPage }
            };
        }

        public ObservableCollection<NavigationItem> NavigationItems { get; }

        public PageViewModelBase CurrentPage
        {
            get => _currentPage;
            set => this.RaiseAndSetIfChanged(ref _currentPage, value);
        }

        public MainPageViewModel MainPage { get; }
        public SettingsPageViewModel SettingsPage { get; }
        public AboutPageViewModel AboutPage { get; }

        public void NavigateToPage(object parameter)
        {
            if (parameter is NavigationItem navItem)
            {
                CurrentPage = navItem.Page;
            }
        }
    }

    public class NavigationItem
    {
        public string Label { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public PageViewModelBase Page { get; set; } = null!;
    }
}

using ReactiveUI;

namespace AvaloniaApplication4.ViewModels
{
    public class PageViewModelBase : ViewModelBase
    {
        private string _title = string.Empty;
        private string _icon = string.Empty;

        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public string Icon
        {
            get => _icon;
            set => this.RaiseAndSetIfChanged(ref _icon, value);
        }
    }
}

using KGallery.Abstractions;

namespace KGallery.Pages.Settings_Page
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UiPage
    {
        public SettingsPage(SettingsPageViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}

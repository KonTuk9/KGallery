using KGallery.Abstractions;

namespace KGallery.Pages.About_Page
{
    /// <summary>
    /// Interaction logic for AboutPage.xaml
    /// </summary>
    public partial class AboutPage : UiPage
    {
        public AboutPage(AboutPageViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}

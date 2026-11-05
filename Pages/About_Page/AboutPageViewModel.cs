using CommunityToolkit.Mvvm.Input;
using KGallery.Abstractions;
using System.Diagnostics;
using System.Windows;

namespace KGallery.Pages.About_Page
{
    public partial class AboutPageViewModel : ViewModel
    {
        public string Version => Application.ResourceAssembly.GetName().Version.ToString();

        [RelayCommand]
        private void OpenLink(string link)
        {
            ProcessStartInfo process = new ProcessStartInfo(link)
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(process);
        }
    }
}

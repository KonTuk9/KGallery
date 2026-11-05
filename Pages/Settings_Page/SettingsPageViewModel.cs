using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KGallery.Abstractions;
using KGallery.Services;
using Microsoft.Win32;
using System.IO;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace KGallery.Pages.Settings_Page
{
    public partial class SettingsPageViewModel : ViewModel
    {
        private readonly SettingsService _settingsService;
        private readonly ISnackbarService _snackbarService;

        public SettingsPageViewModel(SettingsService settingsService, ISnackbarService snackbarService)
        {
            _snackbarService = snackbarService;
            _settingsService = settingsService;
            DarkThemeEnabled = _settingsService.Configuration.DarkModeEnabled;
        }

        [ObservableProperty]
        private bool _darkThemeEnabled;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitDirectoryCommand))]
        private string _baseDirectoryPath;

        public bool CanSubmitDirectory => Directory.Exists(BaseDirectoryPath) && BaseDirectoryPath != _settingsService.Configuration.BaseDirectory;

        [RelayCommand]
        private async Task SwitchThemeMode()
        {
            await _settingsService.ToggleThemeAsync();
        }

        [RelayCommand(CanExecute = nameof(CanSubmitDirectory))]
        private async Task SubmitDirectory()
        {
            await _settingsService.ChangeImagesBaseDirectoryAsync(BaseDirectoryPath);

            _snackbarService.Show("Changed directory", "Base directory for images has been changed. Images from previous folder will stay unchanged", ControlAppearance.Primary, new SymbolIcon(SymbolRegular.Info24, 24), TimeSpan.FromSeconds(5));
        }

        [RelayCommand]
        private void BrowseDirectory()
        {
            OpenFolderDialog folderDialog = new OpenFolderDialog()
            {
                Multiselect = false,
                ValidateNames = true,
                Title = "Browse directory"
            };

            if (folderDialog.ShowDialog() == true)
            {
                BaseDirectoryPath = folderDialog.FolderName;
            }
        }
    }
}

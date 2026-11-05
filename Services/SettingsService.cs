using KGallery.Abstractions;
using KGallery.Models;
using Newtonsoft.Json;
using System.IO;
using System.Reactive.Subjects;
using Wpf.Ui.Appearance;

namespace KGallery.Services
{
    public class SettingsService : IAsyncInitializer
    {
        public SettingsConfiguration Configuration { get; private set; }
        public Subject<SettingsConfiguration> SettingsSubject { get; } = new Subject<SettingsConfiguration>();

        private const string _path = "Preferences\\Settings.dat";

        public async Task InitializeAsync()
        {
            string directory = Path.GetDirectoryName(_path);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            if (!File.Exists(_path))
            {
                File.Create(_path, 0).Close();
                Configuration = new SettingsConfiguration();
                return;
            }

            string json = await File.ReadAllTextAsync(_path);
            Configuration = JsonConvert.DeserializeObject<SettingsConfiguration>(json) ?? new SettingsConfiguration();
        }

        private async Task SaveChangesAsync()
        {
            SettingsSubject.OnNext(Configuration);
            await File.WriteAllTextAsync(_path, JsonConvert.SerializeObject(Configuration));
        }

        public async Task ToggleThemeAsync()
        {
            Configuration.DarkModeEnabled = !Configuration.DarkModeEnabled;
            ApplicationThemeManager.Apply(Configuration.DarkModeEnabled ? ApplicationTheme.Dark : ApplicationTheme.Light);
            await SaveChangesAsync();
        }

        public async Task ChangeImagesBaseDirectoryAsync(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            Configuration.BaseDirectory = path;
            await SaveChangesAsync();
        }
    }
}

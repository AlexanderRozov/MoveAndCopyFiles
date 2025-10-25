using System.IO;
using System.Text.Json;
using System.Windows;

namespace SearchMoveAndCopyFiles.Helpers
{
    public class AppSettings
    {
        public string? LastSearchDirectory { get; set; }
        public string SearchPattern { get; set; } = "*.txt;*.png, *.pdf, *.doc, *.docx, *.dwg, *.jpg, *.jpeg";
        public string? FileNameSearchText { get; set; }
        public bool PreserveStructure { get; set; } = true;
        public WindowState WindowState { get; set; } = WindowState.Normal;
        public double WindowWidth { get; set; } = 1000;
        public double WindowHeight { get; set; } = 700;
        public double WindowLeft { get; set; } = 100;
        public double WindowTop { get; set; } = 100;
    }

    public static class SettingsManager
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SearchMoveAndCopyFiles",
            "settings.json");

        private static AppSettings? _settings;
        private static readonly object _lock = new object();

        public static AppSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    lock (_lock)
                    {
                        if (_settings == null)
                        {
                            _settings = LoadSettings();
                        }
                    }
                }
                return _settings;
            }
        }

        public static void SaveSettings()
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var json = JsonSerializer.Serialize(_settings, options);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при сохранении настроек: {ex.Message}");
            }
        }

        private static AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    return settings ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке настроек: {ex.Message}");
            }

            return new AppSettings();
        }

        public static void UpdateWindowSettings(Window window)
        {
            Settings.WindowState = window.WindowState;
            Settings.WindowWidth = window.Width;
            Settings.WindowHeight = window.Height;
            Settings.WindowLeft = window.Left;
            Settings.WindowTop = window.Top;
            SaveSettings();
        }
    }
}

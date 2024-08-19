using CompressedFileViewer.Settings;
using System.Windows;

namespace CompressedFileViewer.Windows;
/// <summary>
/// Interaktionslogik für GZipSettingsDialog.xaml
/// </summary>
public partial class BrotliSettingsDialog : Window, ISettingsDialog
{
    static BrotliSettingsDialog() => SettingsDialog.RegistSettingsDialog(Settings.BrotliSettings.ALGORITHM_NAME, typeof(BrotliSettingsDialog));

    public BrotliSettingsDialog()
    {
        InitializeComponent();
        txtComprLevel.ItemsSource = Enum.GetValues<System.IO.Compression.CompressionLevel>();
    }

    private Settings.BrotliSettings? settings;

    public CompressionSettings? CompressionSettings
    {
        get => settings;
        set
        {
            settings = (Settings.BrotliSettings?)value;
            txtComprLevel.SelectedItem = settings!.CompressionLevel;
            lstSuffix.Items.Clear();
            foreach (string suffix in settings.Extensions)
                _ = lstSuffix.Items.Add(suffix);
        }
    }

    private void AddSuffix(object sender, RoutedEventArgs e)
    {
        string suffix = txtSuffix.Text.Trim();
        if (string.IsNullOrWhiteSpace(suffix)) return;
        if (!suffix.StartsWith('.'))
            suffix = '.' + suffix;
        foreach (string items in lstSuffix.Items)
            if (items.Equals(suffix, StringComparison.OrdinalIgnoreCase))
                return;
        _ = lstSuffix.Items.Add(suffix);
    }

    private void DeleteSuffix(object sender, RoutedEventArgs e)
    {
        if (lstSuffix.SelectedIndex != -1)
            lstSuffix.Items.RemoveAt(lstSuffix.SelectedIndex);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        settings!.Extensions.Clear();
        settings.Extensions.AddRange(lstSuffix.Items.Cast<string>());
        settings.CompressionLevel = (System.IO.Compression.CompressionLevel)txtComprLevel.SelectedItem;
        DialogResult = true;
        Close();
    }

    private void Cancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Default(object sender, RoutedEventArgs e)
    {
        BrotliSettings settings = Preferences.Default.BrotliSettings;
        txtComprLevel.Text = settings.CompressionLevel.ToString();
        lstSuffix.Items.Clear();
        foreach (string suffix in settings.Extensions)
            _ = lstSuffix.Items.Add(suffix);

    }
}

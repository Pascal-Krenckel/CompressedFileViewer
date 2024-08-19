using CompressedFileViewer.Settings;
using System.Windows;

namespace CompressedFileViewer.Windows;
/// <summary>
/// Interaktionslogik für ZstdSettingsDialog.xaml
/// </summary>
public partial class ZstdSettingsDialog : Window, ISettingsDialog
{
    static ZstdSettingsDialog() => SettingsDialog.RegistSettingsDialog(ZstdSettings.ALGORITHM_NAME, typeof(ZstdSettingsDialog));

    public ZstdSettingsDialog() => InitializeComponent();

    private Settings.ZstdSettings? settings;

    public CompressionSettings? CompressionSettings
    {
        get => settings;
        set
        {
            settings = (Settings.ZstdSettings?)value;
            txtBufferSize.Text = settings!.BufferSize.ToString();
            txtComprLevel.Text = settings.CompressionLevel.ToString();
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
        int comprLevel = int.Parse(txtComprLevel.Text);
        int bufferSize = int.Parse(txtBufferSize.Text);
        settings!.Extensions.Clear();
        settings.Extensions.AddRange(lstSuffix.Items.Cast<string>());
        settings.CompressionLevel = comprLevel;
        settings.BufferSize = bufferSize;
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
        ZstdSettings settings = Preferences.Default.ZstdSettings;
        txtBufferSize.Text = settings!.BufferSize.ToString();
        txtComprLevel.Text = settings.CompressionLevel.ToString();
        lstSuffix.Items.Clear();
        foreach (string suffix in settings.Extensions)
            _ = lstSuffix.Items.Add(suffix);

    }
}

using CompressedFileViewer.Settings;
using System.Windows;

namespace CompressedFileViewer.Windows;
/// <summary>
/// Interaktionslogik für GZipSettingsDialog.xaml
/// </summary>
public partial class BZip2SettingsDialog : Window, ISettingsDialog
{
    static BZip2SettingsDialog() => SettingsDialog.RegistSettingsDialog(Settings.BZip2Settings.ALGORITHM_NAME, typeof(BZip2SettingsDialog));

    public BZip2SettingsDialog() => InitializeComponent();

    private Settings.BZip2Settings? settings;

    public CompressionSettings? CompressionSettings
    {
        get => settings;
        set
        {
            settings = (Settings.BZip2Settings?)value;
            txtComprLevel.Text = settings!.CompressionLevel.ToString();
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
        settings!.Extensions.Clear();
        settings.Extensions.AddRange(lstSuffix.Items.Cast<string>());
        settings.CompressionLevel = comprLevel;
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
        BZip2Settings settings = Preferences.Default.BZip2Settings;
        txtComprLevel.Text = settings.CompressionLevel.ToString();
        lstSuffix.Items.Clear();
        foreach (string suffix in settings.Extensions)
            _ = lstSuffix.Items.Add(suffix);

    }
}

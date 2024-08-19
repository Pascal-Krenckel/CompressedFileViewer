using CompressedFileViewer.Settings;
using System.Windows;

namespace CompressedFileViewer.Windows;
/// <summary>
/// Interaktionslogik für XZSettingsDialog.xaml
/// </summary>
public partial class XZSettingsDialog : Window, ISettingsDialog
{
    static XZSettingsDialog() => SettingsDialog.RegistSettingsDialog(XZSettings.ALGORITHM_NAME, typeof(XZSettingsDialog));

    public XZSettingsDialog() => InitializeComponent();

    private Settings.XZSettings? settings;

    public CompressionSettings? CompressionSettings
    {
        get => settings;
        set
        {
            settings = (Settings.XZSettings?)value;
            txtBufferSize.Text = settings!.BufferSize.ToString();
            txtComprLevel.Text = ((int)settings.CompressionLevel).ToString();
            txtThreads.Text = settings.Threads.ToString();
            chkMultiThreading.IsChecked = settings.MultiThreading;
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
        int threads = int.Parse(txtThreads.Text);

        settings!.Extensions.Clear();
        settings.Extensions.AddRange(lstSuffix.Items.Cast<string>());
        settings.CompressionLevel = (Joveler.Compression.XZ.LzmaCompLevel)comprLevel;
        settings.BufferSize = bufferSize;
        settings.MultiThreading = chkMultiThreading.IsChecked == true;
        settings.Threads = threads;
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
        XZSettings settings = Preferences.Default.XZSettings;
        txtBufferSize.Text = settings!.BufferSize.ToString();
        txtComprLevel.Text = settings.CompressionLevel.ToString();
        lstSuffix.Items.Clear();
        foreach (string suffix in settings.Extensions)
            _ = lstSuffix.Items.Add(suffix);

    }
}

using CompressedFileViewer.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CompressedFileViewer.Windows;
/// <summary>
/// Interaktionslogik für GZipSettingsDialog.xaml
/// </summary>
public partial class BrotliSettingsDialog : Window, ISettingsDialog
{
    static BrotliSettingsDialog()
    {
        SettingsDialog.RegistSettingsDialog(Settings.BrotliSettings.ALGORITHM_NAME, typeof(BrotliSettingsDialog));
    }

    public BrotliSettingsDialog()
    {
        InitializeComponent();
        txtComprLevel.ItemsSource = Enum.GetValues<System.IO.Compression.CompressionLevel>();
    }

    Settings.BrotliSettings? settings;

    public CompressionSettings? CompressionSettings 
    {
        get => settings;
        set
        {
            settings = (Settings.BrotliSettings?)value;
            txtComprLevel.SelectedItem = settings!.CompressionLevel;
            lstSuffix.Items.Clear();
            foreach (var suffix in settings.Extensions)
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
        if(lstSuffix.SelectedIndex != -1)
            lstSuffix.Items.RemoveAt(lstSuffix.SelectedIndex);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        settings!.Extensions.Clear();
        settings.Extensions.AddRange(lstSuffix.Items.Cast<string>());
        settings.CompressionLevel = (System.IO.Compression.CompressionLevel)txtComprLevel.SelectedItem;
        this.DialogResult = true;
        Close();
    }

    private void Cancel(object sender, RoutedEventArgs e)
    {
        this.DialogResult = false;
        Close();
    }

    private void Default(object sender, RoutedEventArgs e)
    {
        var settings = Preferences.Default.BrotliSettings;
        txtComprLevel.Text = settings.CompressionLevel.ToString();
        lstSuffix.Items.Clear();
        foreach (var suffix in settings.Extensions)
            _ = lstSuffix.Items.Add(suffix);

    }
}

using System.Windows;

namespace CompressedFileViewer.Windows;
/// <summary>
/// Interaktionslogik für CompressDialog.xaml
/// </summary>
public partial class DecompressDialog : Window
{
    public DecompressDialog() => InitializeComponent();


    private void Cancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private Settings.CompressionSettings[]? compressionSettings;
    public Settings.CompressionSettings[]? CompressionSettings
    {
        get => compressionSettings;
        set
        {
            compressionSettings = value;
            lstAlg.Items.Clear();
            if (compressionSettings == null) return;
            foreach (Settings.CompressionSettings item in compressionSettings)
                _ = lstAlg.Items.Add(item.AlgorithmName);
        }
    }

    public Settings.CompressionSettings? SelectedCompression => lstAlg.SelectedIndex == -1 ? null : compressionSettings![lstAlg.SelectedIndex];



    private void Compress(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}

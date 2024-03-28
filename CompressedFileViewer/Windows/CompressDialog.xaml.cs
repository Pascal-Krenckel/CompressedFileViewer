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
/// Interaktionslogik für CompressDialog.xaml
/// </summary>
public partial class CompressDialog : Window
{
    public CompressDialog()
    {
        InitializeComponent();
    }


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
            foreach (var item in compressionSettings)
                _ = lstAlg.Items.Add(item.AlgorithmName);
        }
    }

    public Settings.CompressionSettings? SelectedCompression
    {
        get
        {
            if (lstAlg.SelectedIndex == -1)
                return null;
            return compressionSettings![lstAlg.SelectedIndex];
        }
    }
        


    private void Compress(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}

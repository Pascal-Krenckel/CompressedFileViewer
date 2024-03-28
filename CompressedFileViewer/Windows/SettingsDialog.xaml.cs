using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Xml.Linq;
using System.Xml.Serialization;
using Preferences = CompressedFileViewer.Preferences;

namespace CompressedFileViewer.Windows;
/// <summary>
/// Interaktionslogik für SettingsDialog.xaml
/// </summary>
public partial class SettingsDialog : Window
{
    static Dictionary<string,Type> typeDict = [];
    public static void RegistSettingsDialog(string algName, Type windowType)
    {
        if (!windowType.IsSubclassOf(typeof(Window)))
            throw new ArgumentException($"The type must be of type {typeof(Window)}.", nameof(windowType));
        if (windowType.GetInterface(nameof(ISettingsDialog)) == null)
            throw new ArgumentException($"The type must implement {typeof(ISettingsDialog)}.", nameof(windowType));
        typeDict[algName] = windowType;
    }

    static SettingsDialog()
    {
        foreach (var t in Assembly.GetCallingAssembly().GetTypes().Where(t => t.GetInterface(nameof(ISettingsDialog)) != null))
            try
            {
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(t.TypeHandle);
            }
            catch (Exception ex)
            {
                Logging.Log(ex);
            }
    }

    public SettingsDialog()
    {
        InitializeComponent();
    }

    Preferences preferences = new();

    public Preferences Preferences
    {
        get
        {
            preferences.DecompressAll = chk_decompressAll.IsChecked == true;
            preferences.UpdateStatusBar = chk_updateStatusBar.IsChecked == true;
            preferences.EnabledCompressionAlgorithms.Clear();
            foreach (CheckBox chkCmpr in lstAlg.Items)
                if (chkCmpr.IsChecked == true && chkCmpr.IsEnabled)
                    preferences.EnabledCompressionAlgorithms.Add(chkCmpr.Name);
            return preferences;
        }
        set
        {
            preferences = Clone(value);
            chk_decompressAll.IsChecked = preferences.DecompressAll;
            chk_updateStatusBar.IsChecked = preferences.UpdateStatusBar;
            foreach (var alg in preferences.CompressionAlgorithms)
                _ = lstAlg.Items.Add(new CheckBox()
                {
                    Name = alg.AlgorithmName,
                    Content = alg.AlgorithmName,
                    IsChecked = preferences.EnabledCompressionAlgorithms.Contains(alg.AlgorithmName),
                    IsEnabled = alg.IsSupported,
                    ToolTip = alg.IsSupported ? String.Empty : "The algorithm is not supported.\nCheck the log there might be more information.",
                });
        }
    }

    private static Preferences Clone(Preferences preferences)
    {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(Preferences));
        using MemoryStream memoryStream = new();
        xmlSerializer.Serialize(memoryStream, preferences);
        _ = memoryStream.Seek(0, SeekOrigin.Begin);
        return (xmlSerializer.Deserialize(memoryStream) as Preferences)!;
    }

    private void ComprSettingsClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            if (lstAlg.SelectedItem != null)
            {
                string name = ((CheckBox)lstAlg.SelectedItem).Name;
                if (!typeDict.ContainsKey(name))
                    _ = MessageBox.Show($"No dialog registerd with {name}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                {
                    Type windowType = typeDict[name];
                    Window window = (Window)Activator.CreateInstance(windowType)!;
                    ((ISettingsDialog)window).CompressionSettings = preferences.CompressionAlgorithms.Where(compr => compr.AlgorithmName == name).First();
                    _ = window.ShowDialog();
                }
            }

        }
        catch (Exception ex)
        {
            Logging.Log(ex);
            _ = MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        }
    }

    private void Up(object sender, RoutedEventArgs e)
    {
        int selectedIndex = lstAlg.SelectedIndex;
        if (selectedIndex <= 0) return;
        Swap(selectedIndex, selectedIndex - 1);
        lstAlg.SelectedIndex = selectedIndex - 1;
    }
    private void Swap(int index1, int index2)
    {
        (index1, index2) = (Math.Min(index1, index2), Math.Max(index1, index2));
        CheckBox box1 = (CheckBox)lstAlg.Items[index1];
        CheckBox box2 = (CheckBox)lstAlg.Items[index2];
        lstAlg.Items.Remove(box1);
        lstAlg.Items.Remove(box2);
        lstAlg.Items.Insert(index1, box2);
        lstAlg.Items.Insert(index2, box1);
    }

    private void Down(object sender, RoutedEventArgs e)
    {
        int selectedIndex = lstAlg.SelectedIndex;
        if (selectedIndex == -1 || selectedIndex >= lstAlg.Items.Count - 1) return;
        Swap(selectedIndex, selectedIndex + 1);
        lstAlg.SelectedIndex = selectedIndex + 1;
    }

    private void OK(object sender, RoutedEventArgs e)
    {
        this.DialogResult = true;
        Close();
    }

    private void Default(object sender, RoutedEventArgs e)
    {
        Preferences = Preferences.Default;
    }

    private void Cancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

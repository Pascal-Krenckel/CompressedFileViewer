﻿using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace CompressedFileViewer.Windows;
/// <summary>
/// Interaktionslogik für SettingsDialog.xaml
/// </summary>
public partial class SettingsDialog : Window
{
    private static readonly Dictionary<string,Type> typeDict = [];
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
        foreach (Type? t in Assembly.GetCallingAssembly().GetTypes().Where(t => t.GetInterface(nameof(ISettingsDialog)) != null))
            try
            {
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(t.TypeHandle);
            }
            catch (Exception ex)
            {
                Logging.Log(ex);
            }
    }

    public SettingsDialog() => InitializeComponent();

    private Preferences preferences = new();

    public Preferences Preferences
    {
        get
        {
            preferences.DecompressAll = chk_decompressAll.IsChecked == true;
            preferences.UpdateStatusBar = chk_updateStatusBar.IsChecked == true;

            for (int i = 0; i < lstAlg.Items.Count; i++)
            {
                AlgEntry compr = (AlgEntry)lstAlg.Items[i];
                Settings.CompressionSettings settings = preferences.GetCompressionByName(compr.Name)!;
                settings.IsActive = compr.IsActive;
                if (!settings.IsEnabled && compr.IsEnabled)
                    settings.Initialize();
                settings.IsEnabled = compr.IsEnabled;
                settings.SortOrder = i;
            }

            return preferences;
        }
        set
        {
            preferences = Clone(value);
            chk_decompressAll.IsChecked = preferences.DecompressAll;
            chk_updateStatusBar.IsChecked = preferences.UpdateStatusBar;
            lstAlg.Items.Clear();
            foreach (Settings.CompressionSettings alg in preferences.CompressionAlgorithms)
                _ = lstAlg.Items.Add(new AlgEntry(alg.AlgorithmName, alg.IsEnabled, alg.IsSupported, alg.IsActive));
        }
    }

    private static Preferences Clone(Preferences preferences)
    {
        XmlSerializer xmlSerializer = new(typeof(Preferences));
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
                string name = ((AlgEntry)lstAlg.SelectedItem).Name;
                if (!typeDict.TryGetValue(name, out Type? value))
                    _ = MessageBox.Show($"No dialog registerd with {name}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                {
                    Type windowType = value;
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
        DialogResult = true;
        Close();
    }

    private void Default(object sender, RoutedEventArgs e) => Preferences = Preferences.Default;

    private void Cancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}


public class AlgEntry(string name, bool isEnabled, bool isSupported, bool isActive) : INotifyPropertyChanged, IEquatable<AlgEntry?>
{
    private string name = name;
    private bool isEnabled = isEnabled;
    private bool isSupported = isSupported;
    private bool isActive = isActive;

    public string Name { get => name; set => SetValue(ref name, value); }
    public bool IsEnabled { get => isEnabled; set => SetValue(ref isEnabled, value); }
    public bool IsSupported { get => isSupported; set => SetValue(ref isSupported, value); }
    public bool IsActive { get => isActive; set => SetValue(ref isActive, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    public override bool Equals(object? obj) => Equals(obj as AlgEntry);
    public bool Equals(AlgEntry? other) => other is not null && name == other.name;
    public override int GetHashCode() => HashCode.Combine(name);
    private void RaisePropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void SetValue<T>(ref T property, T value, [CallerMemberName] string? propertyName = null)
    {
        property = value;
        RaisePropertyChanged(propertyName);
    }

    public static bool operator ==(AlgEntry? left, AlgEntry? right) => EqualityComparer<AlgEntry>.Default.Equals(left, right);
    public static bool operator !=(AlgEntry? left, AlgEntry? right) => !(left == right);
}
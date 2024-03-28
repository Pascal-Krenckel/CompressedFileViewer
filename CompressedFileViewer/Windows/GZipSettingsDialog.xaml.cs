﻿using CompressedFileViewer.Settings;
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
public partial class GZipSettingsDialog : Window, ISettingsDialog
{
    static GZipSettingsDialog()
    {
        SettingsDialog.RegistSettingsDialog("gzip", typeof(GZipSettingsDialog));
    }

    public GZipSettingsDialog()
    {
        InitializeComponent();
    }

    Settings.GZipSettings? settings;

    public CompressionSettings? CompressionSettings 
    {
        get => settings;
        set
        {
            settings = (Settings.GZipSettings?)value;
            txtBufferSize.Text = settings!.BufferSize.ToString();
            txtComprLevel.Text = settings.CompressionLevel.ToString();
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
        int comprLevel = int.Parse(txtComprLevel.Text);
        int bufferSize = int.Parse(txtBufferSize.Text);
        settings!.Extensions.Clear();
        settings.Extensions.AddRange(lstSuffix.Items.Cast<string>());
        settings.CompressionLevel = comprLevel;
        settings.BufferSize = bufferSize;
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
        var settings = Preferences.Default.GZipSettings;
        txtBufferSize.Text = settings!.BufferSize.ToString();
        txtComprLevel.Text = settings.CompressionLevel.ToString();
        lstSuffix.Items.Clear();
        foreach (var suffix in settings.Extensions)
            _ = lstSuffix.Items.Add(suffix);

    }
}

using CompressedFileViewer.PluginInfrastructure;
using CompressedFileViewer.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CompressedFileViewer;
public class Plugin
{
    public static readonly string PluginName = "CompressedFileViewer";
    public static NotepadPPGateway NppGateway { get; } = new();
    public static ScintillaGateway ScintillaGateway => PluginBase.GetCurrentScintilla();
    public static FileTracker FileTracker = new();
    public static Preferences Preferences { get; set; } = new();

    internal static void OnNotification(ScNotification notification)
    {
        try
        {
            switch (notification.Header.Code)
            {
                case (uint)NppMsg.NPPN_FILEOPENED:                    
                    FileHelper.OpenFile(notification); 
                    break;
                case (uint)NppMsg.NPPN_FILEBEFORESAVE:
                    Logging.Log("Event: FileBeforeSave");
                    FileHelper.BeforeSave(notification);
                    break;
                case (uint)NppMsg.NPPN_FILESAVED:
                    Logging.Log("Event: FileSaved");
                    FileHelper.FileSaved(notification);
                    break;
                case (uint)NppMsg.NPPN_FILECLOSED:
                    FileHelper.Remove(notification.Header.IdFrom);                   
                    break;
                case (uint)NppMsg.NPPN_BUFFERACTIVATED:                    
                    UpdateStatusbar(notification.Header.IdFrom);
                    UpdateCommandChecked(notification.Header.IdFrom);
                    break;
            }
        }
        catch(Exception ex)
        {
            Logging.Log(ex);
        }
    }


    internal static void PluginInit()
    {
        Logging.Log("Plugin Started");
        // you have to specify at least one command, no icon needed
        LoadSettings();
      

        AddCommands();
    }

    private static void AddCommands()
    {
        using var iconStream = System.Reflection.Assembly.GetCallingAssembly().GetManifestResourceStream(typeof(Plugin).Namespace!+".icons.gzip-filled16.png")!;
        Bitmap icon = (Bitmap)Bitmap.FromStream(iconStream);

        PluginBase.AddCommand("Toogle Compression", ToogleCompress, icon);
        PluginBase.AddMenuSeperator();
        PluginBase.AddCommand("Compress", Compress);
        PluginBase.AddCommand("Decompress", Decompress);
        PluginBase.AddMenuSeperator();
        PluginBase.AddCommand("Show Log", () => new Windows.Log().ShowDialog());
        PluginBase.AddMenuSeperator();
        PluginBase.AddCommand("Settings", ShowSettings);
        PluginBase.AddCommand("About", () => new Windows.AboutDialog().ShowDialog());
        PluginBase.AddCommand("Credits", () => new Windows.Credits().ShowDialog());
        PluginBase.AddMenuSeperator();
        foreach (var compr in Preferences.SupportedCompressionAlgorithms)
            PluginBase.AddCommand(compr.AlgorithmName, () =>
            {
                IntPtr bufferId = NppGateway.GetCurrentBufferId();
                var compressor = Preferences.SupportedCompressionAlgorithms.FirstOrDefault(alg => alg.AlgorithmName == compr.AlgorithmName);

                compressor = FileTracker.GetCompressor(bufferId) == compressor ? null : compressor; // if already compressed (same compressor) disable compression

                CompressionHelper.SetCompression(FileTracker,bufferId,compressor);

                UpdateCommandChecked(bufferId);
                UpdateStatusbar(bufferId, true);
            });
        
    }



    private static void ToogleCompress()
    {
        IntPtr bufferId = NppGateway.GetCurrentBufferId();
        var compressor = Preferences.GetNextCompressor(FileTracker.GetCompressor(bufferId)?.AlgorithmName, Preferences.GetCompressionBySuffix(NppGateway.GetFullPathFromBufferId(bufferId)));

        CompressionHelper.SetCompression(FileTracker,bufferId, compressor);

        UpdateCommandChecked(bufferId);
        UpdateStatusbar(bufferId, true);
    }

 

    private static void UpdateStatusbar(IntPtr from, bool resetStatusbar = false)
    {
        if (Preferences.UpdateStatusBar)
        {
            if (FileTracker.IsIncluded(from))
            {
                var enc = FileTracker.GetEncoding(from);
                string str = $"{FileTracker?.GetCompressor(from)?.AlgorithmName}/{enc?.WebName?.ToUpper()}";
                if (enc?.GetPreamble().Length > 0)
                    str += " BOM";
                _ = NppGateway.SetStatusBar(NppMsg.STATUSBAR_UNICODE_TYPE, str);
            }
            else if (resetStatusbar)
            {
                var enc = CompressionHelper.ToEncoding((NppEncoding)NppGateway.GetBufferEncoding(from));
                string str = $"{enc.WebName.ToUpper()}";
                if (enc.GetPreamble().Length > 0)
                    str += " BOM";
                _ = NppGateway.SetStatusBar(NppMsg.STATUSBAR_UNICODE_TYPE, str);
            }
        }
    }
    private static void UpdateCommandChecked(IntPtr from)
    {
        NppGateway.SetMenuItemCheck(0, FileTracker.IsIncluded(from));
        var compr = FileTracker.GetCompressor(from);
        foreach (var posCompr in Preferences.SupportedCompressionAlgorithms)
            NppGateway.SetMenuItemCheck(posCompr.AlgorithmName, compr?.AlgorithmName == posCompr.AlgorithmName);
    }

    private static void Compress()
    {
        Windows.CompressDialog compressForm = new()
        {
            CompressionSettings = Preferences.SupportedCompressionAlgorithms.ToArray(),
        };
        if (compressForm.ShowDialog() == true && compressForm.SelectedCompression != null)
        {
            IntPtr bufferId = NppGateway.GetCurrentBufferId();
            var compr = compressForm.SelectedCompression;
            using var contentStream = CompressionHelper.GetCurrentContentStream();
            var enc = FileTracker.GetEncoding(bufferId) ?? CompressionHelper.ToEncoding((NppEncoding)NppGateway.GetBufferEncoding(bufferId));
            using var encodedContentStream = CompressionHelper.Encode(contentStream, enc, compr);
            CompressionHelper.SetEncodedText(encodedContentStream);
            NppGateway.SendMenuEncoding(NppEncoding.UTF8); // Set MenuEncoding to match scintillas internal buffer encoding
                                                           // if it's not UTF-8... who cares
        }
    }

    private static void Decompress()
    {
        Windows.DecompressDialog decompressForm = new()
        {
            CompressionSettings = Preferences.SupportedCompressionAlgorithms.ToArray(),
        };
        if (decompressForm.ShowDialog() == true && decompressForm.SelectedCompression != null)
        {
            IntPtr bufferId = NppGateway.GetCurrentBufferId();
            var compr = decompressForm.SelectedCompression;
            using var contentStream = CompressionHelper.GetCurrentContentStream();
            using var decodedContentStream = CompressionHelper.Decode(contentStream, compr);
            var enc = CompressionHelper.SetDecodedText(decodedContentStream);
            var nppEnc = CompressionHelper.ToNppEncoding(enc);
            NppGateway.SendMenuEncoding(nppEnc);
        }
    }

    #region Settings
    private static void ShowSettings()
    {
        var window = new Windows.SettingsDialog() { Preferences = Preferences };
        if (window.ShowDialog() == true)
        {
            Preferences = window.Preferences;
            SaveSettings();
        }
    }
    private static void LoadSettings()
    {
        string initFilePath = NppGateway.GetPluginsConfigDir();
        _ = Directory.CreateDirectory(initFilePath);
        initFilePath = Path.Combine(initFilePath, PluginName + ".config");
        try { Preferences = Preferences.Deserialize(initFilePath); } catch (Exception ex) { Logging.Log(ex); Preferences = Preferences.Default; }
    }
    private static void SaveSettings()
    {
        string initFilePath = NppGateway.GetPluginsConfigDir();
        _ = Directory.CreateDirectory(initFilePath);
        initFilePath = Path.Combine(initFilePath, PluginName + ".config");
        try { Preferences.Serialize(initFilePath); }
        catch (Exception ex)
        {
            _ = System.Windows.MessageBox.Show(ex.Message, "Error while serialize settings", MessageBoxButton.OK, MessageBoxImage.Error);
            Logging.Log(ex);
        }
    }
    #endregion

    internal static void CleanUp()
    {
        //SaveSettings(); Not Needed, settings are saved when changed. See ShowSettings()
    }


}

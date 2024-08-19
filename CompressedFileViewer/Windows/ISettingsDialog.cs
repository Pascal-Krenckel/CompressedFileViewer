using CompressedFileViewer.Settings;

namespace CompressedFileViewer.Windows;
public interface ISettingsDialog
{
    public CompressionSettings? CompressionSettings { get; set; }
}

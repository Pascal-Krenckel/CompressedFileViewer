using CompressedFileViewer.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressedFileViewer.Windows;
public interface ISettingsDialog
{
    public CompressionSettings? CompressionSettings { get; set; }
}

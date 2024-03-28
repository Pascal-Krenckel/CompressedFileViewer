using ICSharpCode.SharpZipLib.GZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressedFileViewer.Settings;

[Serializable]
public class GZipSettings : CompressionSettings
{
    public static readonly string ALGORITHM_NAME = "gzip";
    static GZipSettings()
    {
        try
        {
            using GZipOutputStream outStream = new(Stream.Null);
            using GZipInputStream inStream = new(Stream.Null);
            isSupported = true;
        }
        catch (Exception ex)
        {
            isSupported = false;
            Logging.Log(ex);
        }
    }

    public int CompressionLevel { get; set; } = 9;
    public int BufferSize { get; set; } = 1024;

    public override string AlgorithmName => ALGORITHM_NAME;
    private static readonly bool isSupported;
    public override bool IsSupported => isSupported;

    public override Stream GetCompressionStream(Stream outStream)
    {
        GZipOutputStream outputStream = new(outStream, BufferSize)
        {
            IsStreamOwner = false
        };
        outputStream.SetLevel(CompressionLevel);
        return outputStream;
    }

    public override Stream GetDecompressionStream(Stream inStream) => new GZipInputStream(inStream)
    {
        IsStreamOwner = false
    };
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressedFileViewer.Settings;
[Serializable]
public class ZstdSettings : CompressionSettings
{
    public static readonly string ALGORITHM_NAME = "zstd";

    static bool isSupported;
    static ZstdSettings()
    {
        isSupported = true;
        try
        {
            using var compressor = new ZstdSharp.Compressor();
            using var decompressor = new ZstdSharp.Decompressor();
        }
        catch(Exception ex) 
        {
            Logging.Log(ex);
            isSupported = false;
        }
    }

    public int CompressionLevel { get; set; } = 11;
    public int BufferSize { get; set; } = 1024 * 1024;

    public override string AlgorithmName => ALGORITHM_NAME;

    public override bool IsSupported => isSupported;

    public override Stream GetCompressionStream(Stream outStream)
    {
        ZstdSharp.CompressionStream stream = new(outStream,CompressionLevel,BufferSize,true);
        return stream;
    }
    public override Stream GetDecompressionStream(Stream inStream)
    {
        ZstdSharp.DecompressionStream decompressionStream = new(inStream,BufferSize,true);
        return decompressionStream;
    }
}

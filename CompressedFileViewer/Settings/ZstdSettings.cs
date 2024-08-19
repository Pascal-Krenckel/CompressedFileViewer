using System.IO;

namespace CompressedFileViewer.Settings;
[Serializable]
public class ZstdSettings : CompressionSettings
{
    public static readonly string ALGORITHM_NAME = "zstd";
    private static bool isSupported = true;

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

    public override void Initialize()
    {
        isSupported = true;
        try
        {
            using ZstdSharp.Compressor compressor = new();
            using ZstdSharp.Decompressor decompressor = new();
        }
        catch (Exception ex)
        {
            Logging.Log(ex);
            isSupported = false;
        }
    }
}

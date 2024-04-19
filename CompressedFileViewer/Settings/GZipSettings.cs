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

    public int CompressionLevel { get; set; } = 9;
    public int BufferSize { get; set; } = 1024;

    public override string AlgorithmName => ALGORITHM_NAME;
    public override bool IsSupported => true;

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

    public override void Initialize() { }
}

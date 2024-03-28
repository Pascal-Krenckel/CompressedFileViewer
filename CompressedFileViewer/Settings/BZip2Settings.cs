using ICSharpCode.SharpZipLib.BZip2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressedFileViewer.Settings;
[Serializable]
public class BZip2Settings : CompressionSettings
{
    public static readonly string ALGORITHM_NAME = "bzip2";
    static BZip2Settings()
    {
        isSupported = true;
    }
    public override string AlgorithmName => ALGORITHM_NAME;
    public int CompressionLevel { get; set; } = 9;
    private static readonly bool isSupported;
    public override bool IsSupported => isSupported;

    public override Stream GetCompressionStream(Stream outStream) => new BZip2OutputStream(outStream, CompressionLevel)
    { IsStreamOwner = false };

    public override Stream GetDecompressionStream(Stream inStream) => new BZip2InputStream(inStream)
    { IsStreamOwner = false };
}

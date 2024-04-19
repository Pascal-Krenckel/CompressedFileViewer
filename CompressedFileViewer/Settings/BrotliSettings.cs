using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressedFileViewer.Settings;

[Serializable]
public class BrotliSettings : CompressionSettings
{
    public static readonly string ALGORITHM_NAME = "brotli";


    public override string AlgorithmName => ALGORITHM_NAME;

    public override bool IsSupported => true;

    public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Optimal;


    public override Stream GetCompressionStream(Stream outStream)
    {
        BrotliStream brotliStream = new(outStream,CompressionLevel,true);
        return brotliStream;
    }

    public override Stream GetDecompressionStream(Stream inStream)
    {
        BrotliStream brotliStream= new(inStream,CompressionMode.Decompress,true);
        return brotliStream;
    }

    public override void Initialize() { }
}

using Joveler.Compression.XZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CompressedFileViewer.Settings;
[Serializable]
public class XZSettings : CompressionSettings
{
    public static readonly string ALGORITHM_NAME = "xz";
    static bool isSupported = false;
    static XZSettings()
    {
        try
        {
            string currentDir = System.IO.Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location)!;
            string libPath = "";
            /*switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X86:
                    libPath = System.IO.Path.Combine(currentDir, "x86", "liblzma.dll");
                    break;
                case Architecture.X64:
                    libPath = System.IO.Path.Combine(currentDir, "x64", "liblzma.dll");
                    break;
                case Architecture.Arm64:
                    libPath = System.IO.Path.Combine(currentDir, "arm64", "liblzma.dll");
                    break;
            }*/
            libPath = System.IO.Path.Combine(currentDir, "liblzma.dll");
            XZInit.GlobalInit(libPath);
            Logging.Log("Finished loading liblzma.dll", $"Version: {XZInit.VersionString()}");
            isSupported = true;
        } catch (Exception ex)
        {
            isSupported = false;
            Logging.Log(ex);            
        }
    }

    public override string AlgorithmName => ALGORITHM_NAME;

    public int BufferSize { get; set; } = 1024 * 1024;

    public LzmaCheck ChecksumType { get; set; }

    public bool ExtremeFlag { get; set; } = false;

    public LzmaCompLevel CompressionLevel { get; set; } = LzmaCompLevel.Default;

    private XZCompressOptions CompressionOptions => new XZCompressOptions()
    {
        BufferSize = BufferSize,
        Check = ChecksumType,
        ExtremeFlag = ExtremeFlag,
        LeaveOpen = true,
        Level = CompressionLevel,

    };
    private XZDecompressOptions DecompressOptions => new XZDecompressOptions()
    {
        BufferSize = BufferSize,
        LeaveOpen = true,
    };

    public bool MultiThreading { get; set; } = false;
    public int Threads { get; set; } = Environment.ProcessorCount;

    private XZThreadedCompressOptions ThreadOptions => new XZThreadedCompressOptions() { BlockSize = (uint)BufferSize, Threads = Threads };
    private XZThreadedDecompressOptions ThreadDecompressOptions => new XZThreadedDecompressOptions() { Threads = Threads };

    public override bool IsSupported => isSupported;

    public override Stream GetCompressionStream(Stream outStream)
    {
        if (!MultiThreading)
            return new XZStream(outStream, CompressionOptions);
        else
            return new XZStream(outStream, CompressionOptions, ThreadOptions);
    }
    public override Stream GetDecompressionStream(Stream inStream)
    {
        if (!MultiThreading)
            return new XZStream(inStream, DecompressOptions);
        return new XZStream(inStream, DecompressOptions, ThreadDecompressOptions);
    }
}

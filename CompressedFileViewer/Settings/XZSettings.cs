using Joveler.Compression.XZ;
using System.IO;

namespace CompressedFileViewer.Settings;
[Serializable]
public class XZSettings : CompressionSettings
{
    public static readonly string ALGORITHM_NAME = "xz";
    private static bool isSupported = true;

    public override string AlgorithmName => ALGORITHM_NAME;

    public int BufferSize { get; set; } = 1024 * 1024;

    public LzmaCheck ChecksumType { get; set; }

    public bool ExtremeFlag { get; set; } = false;

    public LzmaCompLevel CompressionLevel { get; set; } = LzmaCompLevel.Default;

    private XZCompressOptions CompressionOptions => new()
    {
        BufferSize = BufferSize,
        Check = ChecksumType,
        ExtremeFlag = ExtremeFlag,
        LeaveOpen = true,
        Level = CompressionLevel,

    };
    private XZDecompressOptions DecompressOptions => new()
    {
        BufferSize = BufferSize,
        LeaveOpen = true,
    };

    public bool MultiThreading { get; set; } = false;
    public int Threads { get; set; } = Environment.ProcessorCount;

    private XZThreadedCompressOptions ThreadOptions => new() { BlockSize = (uint)BufferSize, Threads = Threads };
    private XZThreadedDecompressOptions ThreadDecompressOptions => new() { Threads = Threads };

    public override bool IsSupported => isSupported;

    public override Stream GetCompressionStream(Stream outStream) => !MultiThreading ? new XZStream(outStream, CompressionOptions) : (Stream)new XZStream(outStream, CompressionOptions, ThreadOptions);
    public override Stream GetDecompressionStream(Stream inStream) => !MultiThreading
            ? new XZStream(inStream, DecompressOptions)
            : (Stream)new XZStream(inStream, DecompressOptions, ThreadDecompressOptions);

    public override void Initialize()
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
        }
        catch (Exception ex)
        {
            isSupported = false;
            Logging.Log(ex);
        }
    }
}

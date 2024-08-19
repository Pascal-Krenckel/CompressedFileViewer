using CompressedFileViewer.Settings;
using System.Text;

namespace CompressedFileViewer;
public class FileTracker
{
    private readonly HashSet<IntPtr> compressedFiles = [];
    private readonly HashSet<IntPtr> excludedFiles = [];
    private readonly Dictionary<IntPtr, string> filePathes = [];
    private readonly Dictionary<IntPtr, Encoding> encodings = [];
    private readonly Dictionary<IntPtr, CompressionSettings> compressions = [];


    public void Include(IntPtr id, StringBuilder path, Encoding encoding, CompressionSettings compressor) => Include(id, path.ToString(), encoding, compressor);

    public void Exclude(IntPtr id, StringBuilder path) => Exclude(id, path.ToString());

    public void Include(IntPtr id, string path, Encoding encoding, CompressionSettings compressor)
    {
        _ = excludedFiles.Remove(id);
        _ = compressedFiles.Add(id);
        encodings[id] = encoding;
        filePathes[id] = path;
        compressions[id] = compressor;
    }
    public void Exclude(IntPtr id, string path)
    {
        _ = compressedFiles.Remove(id);
        _ = encodings.Remove(id);
        _ = excludedFiles.Add(id);
        _ = compressions.Remove(id);
        filePathes[id] = path;
    }


    public void Remove(IntPtr id)
    {
        _ = compressedFiles.Remove(id);
        _ = excludedFiles.Remove(id);
        _ = filePathes.Remove(id);
        _ = encodings.Remove(id);
    }

    public bool IsIncluded(IntPtr id) => compressedFiles.Contains(id);

    public bool IsExcluded(IntPtr id) => excludedFiles.Contains(id);

    public string? GetStoredPath(IntPtr id) => filePathes.GetValueOrDefault(id);

    public CompressionSettings? GetCompressor(IntPtr id) => compressions.GetValueOrDefault(id);

    public Encoding? GetEncoding(IntPtr id) => encodings.GetValueOrDefault(id);

}

using CompressedFileViewer.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressedFileViewer;
public class FileTracker
{
    HashSet<IntPtr> compressedFiles = new HashSet<IntPtr>();
    HashSet<IntPtr> excludedFiles = new HashSet<IntPtr> ();

    Dictionary<IntPtr, string> filePathes = new Dictionary<IntPtr, string>();
    Dictionary<IntPtr, Encoding> encodings = new Dictionary<IntPtr, Encoding>();
    Dictionary<IntPtr, CompressionSettings> compressions = new Dictionary<IntPtr, CompressionSettings>();


    public void Include(IntPtr id, StringBuilder path, Encoding encoding, CompressionSettings compressor)
    {
        Include(id, path.ToString(), encoding, compressor);
    }

    public void Exclude(IntPtr id, StringBuilder path)
    {
        Exclude(id, path.ToString());
    }

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

    public bool IsIncluded(IntPtr id) { return compressedFiles.Contains(id); }

    public bool IsExcluded(IntPtr id) { return excludedFiles.Contains(id); }

    public string? GetStoredPath(IntPtr id) => filePathes.GetValueOrDefault(id);

    public CompressionSettings? GetCompressor(IntPtr id) => compressions.GetValueOrDefault(id);

    public Encoding? GetEncoding(IntPtr id) => encodings.GetValueOrDefault(id);

}

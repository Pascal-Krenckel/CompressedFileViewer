using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressedFileViewer.Settings;

/// <summary>
/// An abstract class that should be implemented for every compression algorithm.
/// </summary>
[Serializable]
public abstract class CompressionSettings
{
    /// <summary>
    /// List of file extensions associated with this compression algorithm
    /// </summary>
    public List<string> Extensions { get; set; } = new List<string>();

    /// <summary>
    /// The name of the compression algorithm
    /// </summary>
    public abstract string AlgorithmName { get; }

    /// <summary>
    /// Compresses an input stream and writes the content to the given output stream.
    /// </summary>
    /// <param name="inStream">The stream containing the uncompressed input.</param>
    /// <param name="outStream">The stream the compressed data is written to.</param>
    public void Compress(Stream inStream, Stream outStream)
    {
        using var compressionStream = GetCompressionStream(outStream);
        inStream.CopyTo(compressionStream);
    }

    /// <summary>
    /// Decompressed an input stream and writed the content to the given output stream.
    /// </summary>
    /// <param name="inStream">The stream containing the compressed data.</param>
    /// <param name="outStream">The stream the uncompressed data is written to.</param>
    public void Decompress(Stream inStream, Stream outStream)
    {
        using var compressionStream = GetDecompressionStream(inStream);
        compressionStream.CopyTo(outStream);
    }


    /// <summary>
    /// Creates a compression stream.
    /// </summary>
    /// <param name="outStream">The stream where the compressed data should be written.</param>
    /// <returns>A Stream that can be used to write compressed data to the provided outStream.</returns>
    public abstract Stream GetCompressionStream(Stream outStream);

    /// <summary>
    /// Creates a decompression stream.
    /// </summary>
    /// <param name="inStream">The stream to read the compressed data from.</param>
    /// <returns>A Stream that can be used to read the decompressed data from the provided inStream.</returns>
    public abstract Stream GetDecompressionStream(Stream inStream);

    /// <summary>
    /// Whether the compression algorithm is supported or not.
    /// </summary>
    public abstract bool IsSupported { get; }
    public override string ToString() => AlgorithmName;

}

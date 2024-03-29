using CompressedFileViewer.PluginInfrastructure;
using CompressedFileViewer.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace CompressedFileViewer;

[Serializable]
/// <summary>
/// Represents user preferences for compression settings. <br/>
/// </summary>
public class Preferences
{
    public const int VERSION = 2;

    #region Properties
    /// <summary>
    /// Gets or sets the version of the preferences.
    /// </summary>
    public int Version { get; set; } = VERSION;

    /// <summary>
    /// Gets or sets a value indicating whether all files should be attempted to be decompressed independent of the files extension.
    /// </summary>
    public bool DecompressAll { get; set; }

    /// <summary>
    /// True if the statusbar should show (AlgName)/(Encoding) instead of just the Npp default (Encoding)
    /// </summary>
    public bool UpdateStatusBar { get; set; } = true;

    /// <summary>
    /// Gets or sets the list of enabled compression algorithms.
    /// </summary>
    public List<string> EnabledCompressionAlgorithms { get; set; } = new List<string>();

    /// <summary>
    /// Gets the compression algorithms available.
    /// </summary>
    public IEnumerable<CompressionSettings> CompressionAlgorithms => GetType().GetProperties()
                            .Where(m => m.PropertyType.IsSubclassOf(typeof(CompressionSettings)))
                            .Select(m => (CompressionSettings)m.GetValue(this)!);

    /// <summary>
    /// Gets the compression algorithms that are supported.
    /// </summary>
    public IEnumerable<CompressionSettings> SupportedCompressionAlgorithms => GetType().GetProperties()
                            .Where(m => m.PropertyType.IsSubclassOf(typeof(CompressionSettings)))
                            .Select(m => (CompressionSettings)m.GetValue(this)!).Where(alg => alg.IsSupported);
    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Preferences"/> class.
    /// </summary>
    public Preferences() : this(false) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Preferences"/> class with a specified decompress all setting.
    /// </summary>
    /// <param name="decompressAll">If set to <c>true</c>, all files will be decompressed.</param>
    public Preferences(bool decompressAll)
    {
        DecompressAll = decompressAll;
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serializes the preferences to a file.
    /// </summary>
    /// <param name="path">The file path to serialize to.</param>
    public void Serialize(string path)
    {
        using Stream streams = new FileStream(path, FileMode.Create, FileAccess.Write);
        Serialize(streams);
    }

    /// <summary>
    /// Serializes the preferences to a stream.
    /// </summary>
    /// <param name="to">The stream to serialize to.</param>
    public void Serialize(Stream to)
    {
            XmlSerializer serializer = new XmlSerializer(typeof(Preferences));
            serializer.Serialize(to, this);         
    }

    /// <summary>
    /// Deserializes the preferences from a file.
    /// </summary>
    /// <param name="path">The file path to deserialize from.</param>
    /// <returns>The deserialized preferences.</returns>
    public static Preferences Deserialize(string path)
    {
        using Stream streams = new FileStream(path, FileMode.Open, FileAccess.Read);
        return Deserialize(streams);
    }

    /// <summary>
    /// Deserializes the preferences from a stream.
    /// </summary>
    /// <param name="from">The stream to deserialize from.</param>
    /// <returns>The deserialized preferences.</returns>
    public static Preferences Deserialize(Stream from)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(Preferences));
        var pref = (Preferences)serializer.Deserialize(from)!;
        pref.Version = Preferences.VERSION;
        if(pref.Version < 1)
        {
            pref.BZip2Settings = Default.BZip2Settings;
            pref.GZipSettings = Default.GZipSettings;
        }
        if(pref.Version < 2)
        {
            pref.ZstdSettings = Default.ZstdSettings;
        }
        if(pref.Version < 3)
        {
            pref.XZSettings = Default.XZSettings;
        }

        return pref;
    }

    #endregion

    #region GetCompressionBySuffix

    /// <summary>
    /// Gets the compression settings for a file.
    /// </summary>
    /// <param name="file">The file to get the compression settings for.</param>
    /// <returns>The compression settings for the file.</returns>
    public CompressionSettings? GetCompressionBySuffix(FileInfo file) => GetCompressionBySuffix(file.Extension);

    /// <summary>
    /// Gets the compression settings for a file based by extension.
    /// </summary>
    /// <param name="path">The file path to get the compression settings for.</param>
    /// <returns>The compression settings for the file.</returns>
    public CompressionSettings? GetCompressionBySuffix(string? path)
    {
        return SupportedCompressionAlgorithms.FirstOrDefault(comp => comp.Extensions.Any(ext => path?.EndsWith(ext, StringComparison.OrdinalIgnoreCase) ?? false));
    }

    /// <summary>
    /// Gets the compression settings for a file based by extension.
    /// </summary>
    /// <param name="path">The file path to get the compression settings for.</param>
    /// <returns>The compression settings for the file.</returns>
    public CompressionSettings? GetCompressionBySuffix(StringBuilder path) => GetCompressionBySuffix(path.ToString());

    #endregion

    #region Compression Algorithms
    public BZip2Settings BZip2Settings { get; set; } = new();
    public GZipSettings GZipSettings { get; set; } = new();
    public ZstdSettings ZstdSettings { get; set; } = new();
    public XZSettings XZSettings { get; set; } = new();
    #endregion

    /// <summary>
    /// Gets the next enabled compression algoritm.
    /// </summary>
    /// <param name="compressionAlgorithm">The name of the current compression algorithm of the file.</param>
    /// <param name="compressionBySuffix">The compression settings based on the files extension.</param>
    /// <returns>The next compression algorithm</returns>
    public CompressionSettings? GetNextCompressor(string? compressionAlgorithm, CompressionSettings? compressionBySuffix)
    {
        string cAlg;
        if (string.IsNullOrWhiteSpace(compressionAlgorithm))
            cAlg = compressionBySuffix?.AlgorithmName ?? EnabledCompressionAlgorithms.FirstOrDefault(String.Empty);
        else
            cAlg = (compressionAlgorithm != compressionBySuffix?.AlgorithmName ?

                EnabledCompressionAlgorithms
                .SkipWhile(alg => alg != compressionAlgorithm)
                .Skip(1)
                :
                EnabledCompressionAlgorithms
                )
                .FirstOrDefault(algName => algName != compressionBySuffix?.AlgorithmName, String.Empty);


        return SupportedCompressionAlgorithms.FirstOrDefault(comp => comp.AlgorithmName == cAlg);

    }

    /// <summary>
    /// Gets the default preferences.
    /// </summary>
    public static Preferences Default
    {
        get
        {
            Preferences preferences = new(false);
            preferences.EnabledCompressionAlgorithms = preferences.SupportedCompressionAlgorithms.Select(c => c.AlgorithmName).ToList();
            preferences.GZipSettings.Extensions.AddRange([".gz", ".gzip"]);
            preferences.BZip2Settings.Extensions.AddRange([".bz2", ".bzip2"]);
            preferences.ZstdSettings.Extensions.Add(".zst");
            preferences.XZSettings.Extensions.Add(".xz");
            return preferences;
        }
    }
}


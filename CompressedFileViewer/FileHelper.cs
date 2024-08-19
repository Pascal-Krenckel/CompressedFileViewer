using CompressedFileViewer.PluginInfrastructure;
using CompressedFileViewer.Settings;
using System.IO;
using System.Text;

namespace CompressedFileViewer;
internal class FileHelper
{
    private static readonly Dictionary<IntPtr, Position> cursorPosition = [];
    private static readonly Dictionary<IntPtr, CompressionSettings?> compressionBeforeSave = [];
    private static Preferences Preferences => Plugin.Preferences;
    private static FileTracker FileTracker => Plugin.FileTracker;
    private static NotepadPPGateway NppGateway => Plugin.NppGateway;

    public static void OpenFile(ScNotification notification)
    {
        StringBuilder path = NppGateway.GetFullPathFromBufferId(notification.Header.IdFrom);
        CompressionSettings? sourceCompression = Preferences.GetCompressionBySuffix(NppGateway.GetFullPathFromBufferId(notification.Header.IdFrom));
        using MemoryStream gzContentStream = CompressionHelper.GetContentStream(notification, path);
        Logging.LogTree logEntry = new($"File opened: {path}");
        if (sourceCompression != null)
        {
            Logging.LogTree subTree = logEntry.AppendMessage(sourceCompression.AlgorithmName);
            if (gzContentStream.Length == 0) // Empty file:
            {
                Logging.Log(logEntry);
                Encoding encoding = CompressionHelper.ResetEncoding();
                FileTracker.Include(notification.Header.IdFrom, path, encoding, sourceCompression);
                return;
            }
            Encoding? enc = CompressionHelper.TryDecompress(gzContentStream, sourceCompression);
            if (enc != null)
            { // was able to decompress
                Logging.Log(logEntry);
                FileTracker.Include(notification.Header.IdFrom, path, enc, sourceCompression);
                return;
            }
            _ = subTree.AppendMessage("Failed to decompress");
        }
        if (Preferences.DecompressAll && gzContentStream.Length > 0)
        {
            Logging.LogTree subTree = logEntry.AppendMessage("Trying all supported compressions");
            foreach (CompressionSettings compression in Preferences.ActiveCompressionAlgorithms)
            {
                _ = gzContentStream.Seek(0, SeekOrigin.Begin);
                Encoding? enc = CompressionHelper.TryDecompress(gzContentStream, compression);
                if (enc != null)
                { // was able to decompress
                    _ = subTree.AppendMessage($"Compression found: {compression.AlgorithmName}");
                    Logging.Log(logEntry);
                    FileTracker.Include(notification.Header.IdFrom, path, enc, compression);
                    return;
                }
            }
            _ = logEntry.AppendMessage("no compression found");
        }
        Logging.Log(logEntry);
        // no compression found:
        if (sourceCompression != null) // could not compress file although it has a specifix suffix -> exclude this file
            FileTracker.Exclude(notification.Header.IdFrom, path);
    }

    public static void BeforeSave(ScNotification notification)
    {
        StringBuilder path = NppGateway.GetFullPathFromBufferId(notification.Header.IdFrom);
        ScintillaGateway scintillaGateway = Plugin.ScintillaGateway;
        CompressionSettings? compr = GetFileCompression(notification.Header.IdFrom);

        // store the current compression settings
        if (!compressionBeforeSave.TryAdd(notification.Header.IdFrom, compr))
            compressionBeforeSave[notification.Header.IdFrom] = compr;
        if (cursorPosition.ContainsKey(notification.Header.IdFrom))
            cursorPosition[notification.Header.IdFrom] = scintillaGateway.GetCurrentPos();
        else
            cursorPosition.Add(notification.Header.IdFrom, scintillaGateway.GetCurrentPos());

        if (compr == null) return;

        scintillaGateway.BeginUndoAction();


        using MemoryStream contentStream = CompressionHelper.GetContentStream(notification, path);
        Encoding fileEncoding = FileTracker.GetEncoding(notification.Header.IdFrom) ?? new UTF8Encoding(false);
        using MemoryStream encodedContentStream = CompressionHelper.Encode(contentStream, fileEncoding,compr);
        CompressionHelper.SetEncodedText(encodedContentStream);
        NppEncoding currentNppEncoding = (NppEncoding)Plugin.NppGateway.GetBufferEncoding(notification.Header.IdFrom);
        scintillaGateway.EndUndoAction();
    }

    private static CompressionSettings? GetFileCompression(IntPtr idFrom)
    {
        StringBuilder path = NppGateway.GetFullPathFromBufferId(idFrom);


        if (FileTracker.IsExcluded(idFrom))
            return null; // file excluded

        if (Preferences.GetCompressionBySuffix(path) == null && !FileTracker.IsIncluded(idFrom))
            return null; // neither suffix nor included -> no compression, nothing to do

        // either suffix or included:
        CompressionSettings? compr = FileTracker.GetCompressor(idFrom);
        compr ??= Preferences.GetCompressionBySuffix(NppGateway.GetFullPathFromBufferId(idFrom));

        return compr;
    }

    private static CompressionSettings? ShouldBeCompressed(ScNotification notification)
    {
        string newPath = NppGateway.GetFullPathFromBufferId(notification.Header.IdFrom).ToString();

        // no path change -> file tracked
        string? oldPath = FileTracker.GetStoredPath(notification.Header.IdFrom);
        if (newPath == oldPath)
        {
            if (FileTracker.IsIncluded(notification.Header.IdFrom)) // is tracked, so compress
                return FileTracker.GetCompressor(notification.Header.IdFrom);
            if (FileTracker.IsExcluded(notification.Header.IdFrom)) // is excluded, so don't compress
                return null;
            return Preferences.GetCompressionBySuffix(newPath); // no manually set information, store iff gz-suffix (should be tracked then, but who knows) 
        }

        // path changed

        // compression based on suffix changed: return compression for new path
        if (Preferences.GetCompressionBySuffix(oldPath) != Preferences.GetCompressionBySuffix(newPath))
            return Preferences.GetCompressionBySuffix(newPath);

        // same suffix type:

        // from gz to gz or non gz to non gz, use tracker

        if (FileTracker.IsIncluded(notification.Header.IdFrom))
            return FileTracker.GetCompressor(notification.Header.IdFrom);

        if (FileTracker.IsExcluded(notification.Header.IdFrom))
            return null;

        // not tracked -> go by suffix, should always return false, since gz-files should always be tracked
        return Preferences.GetCompressionBySuffix(newPath);
    }

    public static void Remove(IntPtr idFrom)
    {
        FileTracker.Remove(idFrom);
        _ = cursorPosition.Remove(idFrom);
        _ = compressionBeforeSave.Remove(idFrom);
    }

    public static void FileSaved(ScNotification notification)
    {
        ScintillaGateway scintillaGateway = Plugin.ScintillaGateway;
        StringBuilder path = NppGateway.GetFullPathFromBufferId(notification.Header.IdFrom);
        CompressionSettings? targetCompression = ShouldBeCompressed(notification);
        CompressionSettings? oldCompressed = compressionBeforeSave.TryGetValue(notification.Header.IdFrom, out CompressionSettings? value) ? value : null;

        if (oldCompressed != targetCompression)
        {
            // save again, but update file tracker based on toCompressed
            if (targetCompression != null)
            {
                Encoding encoding = FileTracker.GetEncoding(notification.Header.IdFrom) ?? CompressionHelper.ResetEncoding();
                FileTracker.Include(notification.Header.IdFrom, path, encoding, targetCompression);
            }
            else
            {
                FileTracker.Exclude(notification.Header.IdFrom, path);
                scintillaGateway.Undo(); //undo compression
                scintillaGateway.GotoPos(cursorPosition[notification.Header.IdFrom]);
                scintillaGateway.EmptyUndoBuffer();
            }
            NppGateway.SwitchToFile(path);
            scintillaGateway.Undo();
            scintillaGateway.GotoPos(cursorPosition[notification.Header.IdFrom]);
            NppGateway.MakeCurrentBufferDirty();
            NppGateway.SaveCurrentFile();
            return;
        }

        if (oldCompressed != null) // if compressed we need to undo the changes
        {
            scintillaGateway.Undo();
            scintillaGateway.GotoPos(cursorPosition[notification.Header.IdFrom]);

            scintillaGateway.EmptyUndoBuffer();
            scintillaGateway.SetSavePoint();
            scintillaGateway.SetSavePoint();

        }
    }

}

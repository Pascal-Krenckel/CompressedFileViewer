using CompressedFileViewer.PluginInfrastructure;
using CompressedFileViewer.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressedFileViewer;
internal class FileHelper
{
    static Dictionary<IntPtr, Position> cursorPosition = new Dictionary<IntPtr, Position>();
    static Dictionary<IntPtr, CompressionSettings?> compressionBeforeSave = new Dictionary<IntPtr, CompressionSettings?>();
    private static Preferences Preferences => Plugin.Preferences;
    private static FileTracker FileTracker => Plugin.FileTracker;
    private static NotepadPPGateway NppGateway => Plugin.NppGateway;

    public static void OpenFile(ScNotification notification)
    {
        var path = NppGateway.GetFullPathFromBufferId(notification.Header.IdFrom);
        var sourceCompression = Preferences.GetCompressionBySuffix(NppGateway.GetFullPathFromBufferId(notification.Header.IdFrom));
        using var gzContentStream = CompressionHelper.GetContentStream(notification, path);
        Logging.LogTree logEntry = new Logging.LogTree($"File opened: {path}");
        if (sourceCompression != null)
        {
            var subTree = logEntry.AppendMessage(sourceCompression.AlgorithmName);
            if (gzContentStream.Length == 0) // Empty file:
            {
                Logging.Log(logEntry);
                var encoding = CompressionHelper.ResetEncoding();
                FileTracker.Include(notification.Header.IdFrom, path, encoding, sourceCompression);                
                return;
            }
            var enc = CompressionHelper.TryDecompress(gzContentStream, sourceCompression);
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
            var subTree = logEntry.AppendMessage("Trying all supported compressions");
            foreach (var compression in Preferences.SupportedCompressionAlgorithms)
            {
                _ = gzContentStream.Seek(0, SeekOrigin.Begin);
                var enc = CompressionHelper.TryDecompress(gzContentStream, compression);
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
        var path = NppGateway.GetFullPathFromBufferId(notification.Header.IdFrom);
        var scintillaGateway = Plugin.ScintillaGateway;
        var compr = GetFileCompression(notification.Header.IdFrom);

        // store the current compression settings
        if (compressionBeforeSave.ContainsKey(notification.Header.IdFrom))
            compressionBeforeSave[notification.Header.IdFrom] = compr;
        else
            compressionBeforeSave.Add(notification.Header.IdFrom, compr);

        if (cursorPosition.ContainsKey(notification.Header.IdFrom))
            cursorPosition[notification.Header.IdFrom] = scintillaGateway.GetCurrentPos();
        else
            cursorPosition.Add(notification.Header.IdFrom, scintillaGateway.GetCurrentPos());

        if (compr == null) return;

        scintillaGateway.BeginUndoAction();


        using var contentStream = CompressionHelper.GetContentStream(notification, path);
        var fileEncoding = FileTracker.GetEncoding(notification.Header.IdFrom) ?? new UTF8Encoding(false);
        using var encodedContentStream = CompressionHelper.Encode(contentStream, fileEncoding,compr);
        CompressionHelper.SetEncodedText(encodedContentStream);
        var currentNppEncoding = (NppEncoding)Plugin.NppGateway.GetBufferEncoding(notification.Header.IdFrom);
        scintillaGateway.EndUndoAction();
    }

    private static CompressionSettings? GetFileCompression(IntPtr idFrom)
    {
        var path = NppGateway.GetFullPathFromBufferId(idFrom);


        if (FileTracker.IsExcluded(idFrom))
            return null; // file excluded

        if (Preferences.GetCompressionBySuffix(path) == null && !FileTracker.IsIncluded(idFrom))
            return null; // neither suffix nor included -> no compression, nothing to do

        // either suffix or included:
        var compr = FileTracker.GetCompressor(idFrom);
        if (compr == null)  // not included -> path
            compr = Preferences.GetCompressionBySuffix(NppGateway.GetFullPathFromBufferId(idFrom));

        return compr;
    }

    private static CompressionSettings? ShouldBeCompressed(ScNotification notification)
    {
        var newPath = NppGateway.GetFullPathFromBufferId(notification.Header.IdFrom).ToString();

        // no path change -> file tracked
        var oldPath = FileTracker.GetStoredPath(notification.Header.IdFrom);
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
        var scintillaGateway = Plugin.ScintillaGateway;
        var path = NppGateway.GetFullPathFromBufferId(notification.Header.IdFrom);
        var targetCompression = ShouldBeCompressed(notification);
        var oldCompressed = compressionBeforeSave.ContainsKey(notification.Header.IdFrom) ? compressionBeforeSave[notification.Header.IdFrom] : null;

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

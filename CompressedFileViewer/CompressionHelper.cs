using CompressedFileViewer.PluginInfrastructure;
using CompressedFileViewer.Settings;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CompressedFileViewer;
public static class CompressionHelper
{
    internal static MemoryStream GetContentStream(ScNotification notification, StringBuilder path) => GetContentStream(notification.Header.IdFrom, path.ToString());

    private static MemoryStream GetContentStream(IntPtr idFrom, string path)
    {
        Plugin.NppGateway.SwitchToFile(path, idFrom);
        ScintillaGateway scintilla = PluginBase.GetCurrentScintilla();
        int data_length = scintilla.GetLength();
        if (data_length <= 0)
            return new MemoryStream();

        nint pData = scintilla.GetCharacterPointer();
        if (pData == IntPtr.Zero)
            return new MemoryStream();
        MemoryStream memoryStream = new(data_length);
        memoryStream.SetLength(data_length);
        Marshal.Copy(pData, memoryStream.GetBuffer(), 0, data_length);
        return memoryStream;
    }

    internal static MemoryStream GetContentStream(ScNotification notification, string path) => GetContentStream(notification.Header.IdFrom, path);

    internal static MemoryStream GetCurrentContentStream()
    {
        ScintillaGateway scintilla = PluginBase.GetCurrentScintilla();
        int data_length = scintilla.GetLength();
        if (data_length <= 0)
            return new MemoryStream();

        nint pData = scintilla.GetCharacterPointer();
        if (pData == IntPtr.Zero)
            return new MemoryStream();
        MemoryStream memoryStream = new();
        memoryStream.SetLength(data_length);
        Marshal.Copy(pData, memoryStream.GetBuffer(), 0, data_length);
        return memoryStream;
    }

    internal static MemoryStream Decode(Stream gzStream, CompressionSettings compression)
    {
        MemoryStream decodedStream = new();
        compression.Decompress(gzStream, decodedStream);
        return decodedStream;
    }

    internal static Encoding ToEncoding(NppEncoding nppEncoding) => nppEncoding switch
    {
        NppEncoding.UTF16_LE => new UnicodeEncoding(false, true),
        NppEncoding.UTF8_BOM => new UTF8Encoding(true),
        NppEncoding.ANSI => new ASCIIEncoding(),
        NppEncoding.UTF16_BE => new UnicodeEncoding(true, true),
        _ => new UTF8Encoding(false),
    };

    internal static Encoding SetDecodedText(MemoryStream decodedContentStream)
    {
        ScintillaGateway scintillaGateway = PluginBase.GetCurrentScintilla();

        //var encoding = nppGateway.GetBufferEncoding(nppGateway.GetCurrentBufferId());

        decodedContentStream.Position = 0;
        Span<byte> bom = stackalloc byte[(int)Math.Min(4, decodedContentStream.Length)];

        _ = decodedContentStream.Read(bom);
        decodedContentStream.Position = 0;
        Encoding srcEncoding = BOMDetector.GetEncoding(bom) switch
        {
            BOM.UTF8 => new UTF8Encoding(true),
            BOM.UTF16LE => new UnicodeEncoding(false, true),
            BOM.UTF16BE => new UnicodeEncoding(true, true),
            _ => new UTF8Encoding(),
        };
        byte[] buffer = Encoding.Convert(srcEncoding, new UTF8Encoding(false), decodedContentStream.GetBuffer(), 0, (int)decodedContentStream.Length);


        scintillaGateway.ClearAll();
        scintillaGateway.SetCodePage(65001);
        scintillaGateway.AddText(buffer.AsSpan());
        return srcEncoding;
    }

    internal static NppEncoding ToNppEncoding(Encoding encoding) => encoding?.CodePage switch
    {
        // UTF-8
        65001 => encoding.GetPreamble().Length == 0 ? NppEncoding.UTF8 : NppEncoding.UTF8_BOM,
        // utf-16be
        1201 => NppEncoding.UTF16_BE,
        // utf-16le
        1200 => NppEncoding.UTF16_LE,
        // iso-8859-1
        1252 => NppEncoding.ANSI,
        // default
        _ => NppEncoding.UTF8,
    };


    internal static void SetEncodedText(MemoryStream encodedContentStream)
    {
        ScintillaGateway scintillaGateway = PluginBase.GetCurrentScintilla();
        scintillaGateway.ClearAll();
        scintillaGateway.AddText(encodedContentStream.GetBuffer().AsSpan(0, (int)encodedContentStream.Length));
    }

    internal static MemoryStream Encode(Stream stream, Encoding dstEncoding, CompressionSettings compression)
    {
        ScintillaGateway scintillaGateway = PluginBase.GetCurrentScintilla();
        MemoryStream encodedStream = new();
        using Stream compressionStream = compression.GetCompressionStream(encodedStream);

        Encoding srcEncoding = Encoding.GetEncoding(scintillaGateway.GetCodePage());


        if (srcEncoding == dstEncoding)
            stream.CopyTo(compressionStream);
        else
        {
            using MemoryStream mem = new();
            stream.CopyTo(mem);
            byte[] buffer = Encoding.Convert(srcEncoding, dstEncoding, mem.GetBuffer(), 0, (int)mem.Length);
            if (dstEncoding != new UTF8Encoding(false))
            {
                byte[] preamble = dstEncoding.GetPreamble();
                compressionStream.Write(preamble, 0, preamble.Length);
            }
            compressionStream.Write(buffer, 0, buffer.Length);
        }
        return encodedStream;
    }
    internal static Encoding ResetEncoding()
    {
        NotepadPPGateway gateway = new();
        ScintillaGateway scintillaGateway = PluginBase.GetCurrentScintilla();
        nint bufferID = gateway.GetCurrentBufferId();
        long nppEnc = gateway.GetBufferEncoding(bufferID);
        Encoding encoding = ToEncoding((NppEncoding)nppEnc);
        scintillaGateway.SetCodePage(65001);
        gateway.SendMenuEncoding(NppEncoding.UTF8);
        return encoding;
    }
    public static void SetCompression(FileTracker fileTracker, IntPtr bufferId, CompressionSettings? compressor)
    {
        if (null == compressor)
        {
            NppEncoding enc = CompressionHelper.ToNppEncoding(fileTracker.GetEncoding(bufferId) ?? new UTF8Encoding(false));
            fileTracker.Exclude(bufferId, Plugin.NppGateway.GetFullPathFromBufferId(bufferId));
            Plugin.NppGateway.SendMenuEncoding(enc);
            Plugin.NppGateway.MakeCurrentBufferDirty();
        }
        else
        {
            Encoding encoding = fileTracker.GetEncoding(bufferId) ?? CompressionHelper.ResetEncoding();
            fileTracker.Include(bufferId, Plugin.NppGateway.GetFullPathFromBufferId(bufferId), encoding, compressor);
            Plugin.NppGateway.MakeCurrentBufferDirty();
        }
    }

    public static Encoding? TryDecompress(Stream contentStream, CompressionSettings compression)
    {
        try
        {
            using MemoryStream decodedContentStream = CompressionHelper.Decode(contentStream, compression);
            Encoding encoding = CompressionHelper.SetDecodedText(decodedContentStream);

            Plugin.NppGateway.SendMenuEncoding(NppEncoding.UTF8);
            ScintillaGateway scintilla = PluginBase.GetCurrentScintilla();
            scintilla.GotoPos(new Position(0));
            scintilla.EmptyUndoBuffer();
            scintilla.SetSavePoint();
            return encoding;
        }
        catch
        {
            return null;
        }
    }


}

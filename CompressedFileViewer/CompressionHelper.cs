using CompressedFileViewer.PluginInfrastructure;
using CompressedFileViewer.Settings;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CompressedFileViewer;
public static class CompressionHelper
{
    internal static MemoryStream GetContentStream(ScNotification notification, StringBuilder path)
    {
        return GetContentStream(notification.Header.IdFrom, path.ToString());
    }

    private static MemoryStream GetContentStream(IntPtr idFrom, string path)
    {
        Plugin.NppGateway.SwitchToFile(path,idFrom);
        var scintilla = PluginBase.GetCurrentScintilla();
        int data_length = scintilla.GetLength();
        if (data_length <= 0)
            return new MemoryStream();

        var pData = scintilla.GetCharacterPointer();
        if (pData == IntPtr.Zero)
            return new MemoryStream();
        MemoryStream memoryStream = new(data_length);
        memoryStream.SetLength(data_length);
        Marshal.Copy(pData, memoryStream.GetBuffer(), 0, data_length);
        return memoryStream;
    }

    internal static MemoryStream GetContentStream(ScNotification notification, string path)
    {
        return GetContentStream(notification.Header.IdFrom, path);
    }

    internal static MemoryStream GetCurrentContentStream()
    {
        var scintilla = PluginBase.GetCurrentScintilla();
        int data_length = scintilla.GetLength();
        if (data_length <= 0)
            return new MemoryStream();

        var pData = scintilla.GetCharacterPointer();
        if (pData == IntPtr.Zero)
            return new MemoryStream();
        MemoryStream memoryStream = new MemoryStream();
        memoryStream.SetLength(data_length);
        Marshal.Copy(pData, memoryStream.GetBuffer(), 0, data_length);
        return memoryStream;
    }

    internal static MemoryStream Decode(Stream gzStream, CompressionSettings compression)
    {
        MemoryStream decodedStream = new MemoryStream();
        compression.Decompress(gzStream, decodedStream);
        return decodedStream;
    }

    internal static Encoding ToEncoding(NppEncoding nppEncoding)
    {
        switch (nppEncoding)
        {
            case NppEncoding.UTF16_LE: return new UnicodeEncoding(false, true);
            default:
            case NppEncoding.UTF8: return new UTF8Encoding(false);
            case NppEncoding.UTF8_BOM: return new UTF8Encoding(true);
            case NppEncoding.ANSI: return new ASCIIEncoding();
            case NppEncoding.UTF16_BE: return new UnicodeEncoding(true, true);
        }

    }

    internal static Encoding SetDecodedText(MemoryStream decodedContentStream)
    {
        ScintillaGateway scintillaGateway = PluginBase.GetCurrentScintilla();

        //var encoding = nppGateway.GetBufferEncoding(nppGateway.GetCurrentBufferId());

        decodedContentStream.Position = 0;
        Span<byte> bom = stackalloc byte[(int)Math.Min(4, decodedContentStream.Length)];

        _ = decodedContentStream.Read(bom);
        decodedContentStream.Position = 0;
        Encoding srcEncoding;
        switch (BOMDetector.GetEncoding(bom))
        {
            case BOM.UTF8:
                srcEncoding = new UTF8Encoding(true);
                break;
            case BOM.UTF16LE:
                srcEncoding = new UnicodeEncoding(false, true);
                break;
            case BOM.UTF16BE:
                srcEncoding = new UnicodeEncoding(true, true);
                break;
            case BOM.UTF7:
            case BOM.UTF32LE:
            case BOM.UTF32BE:
            case BOM.None:
            default:
                srcEncoding = new UTF8Encoding();
                break;
        }

        byte[] buffer = Encoding.Convert(srcEncoding, new UTF8Encoding(false), decodedContentStream.GetBuffer(), 0, (int)decodedContentStream.Length);

       
        scintillaGateway.ClearAll();
        scintillaGateway.SetCodePage(65001);
        scintillaGateway.AddText(buffer.AsSpan());
        return srcEncoding;
    }

    internal static NppEncoding ToNppEncoding(Encoding encoding)
    {
        return encoding?.CodePage switch
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
    }


    internal static void SetEncodedText(MemoryStream encodedContentStream)
    {
        ScintillaGateway scintillaGateway = PluginBase.GetCurrentScintilla();
        scintillaGateway.ClearAll();
        scintillaGateway.AddText(encodedContentStream.GetBuffer().AsSpan(0, (int)encodedContentStream.Length));
    }

    internal static MemoryStream Encode(Stream stream, Encoding dstEncoding, CompressionSettings compression)
    {
        var scintillaGateway = PluginBase.GetCurrentScintilla();
        MemoryStream encodedStream = new MemoryStream();
        using Stream compressionStream = compression.GetCompressionStream(encodedStream);

        Encoding srcEncoding = Encoding.GetEncoding(scintillaGateway.GetCodePage());


        if (srcEncoding == dstEncoding)
            stream.CopyTo(compressionStream);
        else
        {
            using MemoryStream mem = new MemoryStream();
            stream.CopyTo(mem);
            byte[] buffer = Encoding.Convert(srcEncoding, dstEncoding, mem.GetBuffer(), 0, (int)mem.Length);
            if (dstEncoding != new UTF8Encoding(false))
            {
                var preamble = dstEncoding.GetPreamble();
                compressionStream.Write(preamble, 0, preamble.Length);
            }
            compressionStream.Write(buffer, 0, buffer.Length);
        }
        return encodedStream;
    }
    internal static Encoding ResetEncoding()
    {
        NotepadPPGateway gateway = new NotepadPPGateway();
        ScintillaGateway scintillaGateway = PluginBase.GetCurrentScintilla();
        var bufferID = gateway.GetCurrentBufferId();
        long nppEnc = gateway.GetBufferEncoding(bufferID);
        Encoding encoding = ToEncoding((NppEncoding)nppEnc);
        scintillaGateway.SetCodePage(65001);
        gateway.SendMenuEncoding(NppEncoding.UTF8);
        return encoding;
    }
    public static void SetCompression(FileTracker fileTracker,IntPtr bufferId, CompressionSettings? compressor)
    {
        if (null == compressor)
        {
            var enc = CompressionHelper.ToNppEncoding(fileTracker.GetEncoding(bufferId) ?? new UTF8Encoding(false));
            fileTracker.Exclude(bufferId, Plugin.NppGateway.GetFullPathFromBufferId(bufferId));
            Plugin.NppGateway.SendMenuEncoding(enc);
            Plugin.NppGateway.MakeCurrentBufferDirty();
        }
        else
        {
            var encoding = fileTracker.GetEncoding(bufferId) ?? CompressionHelper.ResetEncoding();
            fileTracker.Include(bufferId, Plugin.NppGateway.GetFullPathFromBufferId(bufferId), encoding, compressor);
            Plugin.NppGateway.MakeCurrentBufferDirty();
        }
    }

    public static Encoding? TryDecompress(Stream contentStream, CompressionSettings compression)
    {
        try
        {
            using var decodedContentStream = CompressionHelper.Decode(contentStream, compression);
            Encoding encoding = CompressionHelper.SetDecodedText(decodedContentStream);

            Plugin.NppGateway.SendMenuEncoding(NppEncoding.UTF8);
            var scintilla = PluginBase.GetCurrentScintilla();
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

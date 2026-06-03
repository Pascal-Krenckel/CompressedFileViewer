using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace PluginCheck;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Program started");

        try
        {
            string path = args.Length > 0 ? args[0] : Environment.ProcessPath!;
            if(path == "--help" || path == "-h" || path == "-?" || path == "/?" || path == "/h")
            {
                Console.WriteLine("Usage: PluginCheck [path_to_plugin]");
                Console.WriteLine("If no path is provided, the current executable's path will be used.");
                return;
            }
            Console.WriteLine(path);
            DirectoryInfo currentDir = Directory.Exists(path) ? new DirectoryInfo(path) : new DirectoryInfo(Path.GetDirectoryName(path)!);

            DirectoryInfo pluginDir = currentDir.Parent!;
            DirectoryInfo notepadppDir = pluginDir.Parent!;

            FileInfo notepadpp = new(Path.Combine(notepadppDir.FullName, "notepad++.exe"));
            CheckNotepadPP(notepadpp);
            Console.WriteLine();


            FileInfo pluginFile = new(Path.Combine(currentDir.FullName, "CompressedFileViewer.plugin.dll"));
            CheckPlugin(pluginFile);

            CheckNativeDll(Path.Combine(currentDir.FullName, "CompressedFileViewer.dll"));

        }
        catch (Exception ex) { Console.WriteLine(ex); }

        Console.WriteLine("\n===========");
        Console.WriteLine("Finished");

        Console.ReadLine();
    }

    [StructLayout(LayoutKind.Sequential)]
    struct NppStruct { nint a, b, c; }
    private static void CheckNativeDll(string dll)
    {
        Console.WriteLine("Native-DLL: {0}", File.Exists(dll));
        if (!File.Exists(dll))
            Environment.Exit(1);

        using var stream = File.OpenRead(dll);
        using var reader = new StreamReader(stream, System.Text.Encoding.Latin1);
        Span<char> data = stackalloc char[1024];
        _ = reader.ReadBlock(data);
        int i = data.IndexOf("PE");
        string check64 = "PE\0\0d\u0086";
        if (i == -1)
        {
            Console.WriteLine("Error Reading dll-Header");
            Environment.Exit(1);
        }
        if (StringComparer.Ordinal.Equals(data[i..(i + 6)].ToString(), check64))
            Console.WriteLine("Dll: 64-bit");
        else
            Console.WriteLine("Dll: 32-bit");



        var dllHandle = NativeLibrary.Load(dll);

        var unicodeHandle = NativeLibrary.GetExport(dllHandle, "isUnicode");
        var unicodeDelegate = Marshal.GetDelegateForFunctionPointer<IsUnicodeDelegate>(unicodeHandle);

        try
        {
            Console.WriteLine("Unicode (1): {0}", unicodeDelegate.Invoke());
        }
        catch (Exception ex) { Console.WriteLine(ex); }

        var setInfoHandle = NativeLibrary.GetExport(dllHandle, "setInfo");
        var setInfoDelegate = Marshal.GetDelegateForFunctionPointer<SetInfo>(setInfoHandle);

        Console.WriteLine("SetInfo");
        try
        {
            setInfoDelegate.Invoke(new());
        }
        catch (Exception ex) { Console.WriteLine(ex); }



        NativeLibrary.Free(dllHandle);

    }

    delegate void SetInfo(NppStruct @struct);
    delegate int IsUnicodeDelegate();

    private static void CheckPlugin(FileInfo pluginFile)
    {
        if (!pluginFile.Exists)
        {
            Console.WriteLine("Plugin not found");
            Environment.Exit(1);
        }
        var assembly = Assembly.LoadFile(pluginFile.FullName);

        Console.WriteLine("Assembly: {0}", assembly);

    }

    private static void CheckNotepadPP(FileInfo notepadpp)
    {
        Console.WriteLine("Notepad++: {0}", notepadpp.Exists);
        if (!notepadpp.Exists)
            Environment.Exit(1);

        using var stream = notepadpp.OpenRead();
        using var reader = new StreamReader(stream, System.Text.Encoding.Latin1);
        Span<char> data = stackalloc char[1024];
        _ = reader.ReadBlock(data);
        int i = data.IndexOf("PE");
        string check64 = "PE\0\0d\u0086";
        if (i == -1)
        {
            Console.WriteLine("Error Reading exe-Header");
            Environment.Exit(1);
        }
        if (StringComparer.Ordinal.Equals(data[i..(i + 6)].ToString(), check64))
            Console.WriteLine("Notepad++: 64-bit");
        else
            Console.WriteLine("Notepad++: 32-bit");
    }
}

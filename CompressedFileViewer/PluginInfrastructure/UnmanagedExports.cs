using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace CompressedFileViewer.PluginInfrastructure;
public static class UnmanagedExports
{

    [UnmanagedCallersOnly(EntryPoint = "isUnicode", CallConvs = [typeof(CallConvCdecl)])]
    public static byte IsUnicode() => 1;

    [UnmanagedCallersOnly(EntryPoint = "setInfo", CallConvs = [typeof(CallConvCdecl)])]
    [DNNE.C99DeclCode("struct NppData{ void* _nppHandle; void* _scintillaMainHandle; void* _scintillaSecondHandle;};")]
    public static void SetInfo([DNNE.C99Type("struct NppData")] NppData notepadPlusData)
    {
        PluginBase.nppData = notepadPlusData;
        Plugin.PluginInit();
    }

    [UnmanagedCallersOnly(EntryPoint = "getFuncsArray", CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe nint GetFuncsArray(int* nbF)
    {
        *nbF = PluginBase._funcItems.Items.Count;
        return PluginBase._funcItems.NativePointer;
    }

    [UnmanagedCallersOnly(EntryPoint = "messageProc", CallConvs = [typeof(CallConvCdecl)])]
    public static uint MessageProc(uint Message, nint wParam, nint lParam) => 1;

    private static IntPtr _ptrPluginName = IntPtr.Zero;

    [UnmanagedCallersOnly(EntryPoint = "getName", CallConvs = [typeof(CallConvCdecl)])]
    public static nint GetName()
    {
        if (_ptrPluginName == IntPtr.Zero)
            _ptrPluginName = Marshal.StringToHGlobalUni(Plugin.PluginName);
        return _ptrPluginName;
    }

    [UnmanagedCallersOnly(EntryPoint = "beNotified", CallConvs = [typeof(CallConvCdecl)])]
    public static void BeNotified(nint notifyCode)
    {
        ScNotification notification = (ScNotification)Marshal.PtrToStructure(notifyCode, typeof(ScNotification))!;
        if (notification.Header.Code == (uint)NppMsg.NPPN_TBMODIFICATION)
        {
            PluginBase._funcItems.RefreshItems();
            PluginBase.SetToolbar();
        }
        else if (notification.Header.Code == (uint)NppMsg.NPPN_SHUTDOWN)
        {
            Plugin.CleanUp();
            Marshal.FreeHGlobal(_ptrPluginName);
            PluginBase.CleanUp();
        }
        else
            Plugin.OnNotification(notification);
    }
}

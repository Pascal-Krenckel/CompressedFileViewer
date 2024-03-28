// NPP plugin platform for .Net v0.94.00 by Kasper B. Graversen etc.
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CompressedFileViewer.PluginInfrastructure;
public interface INotepadPPGateway
{
    void FileNew();

    string GetCurrentFilePath();
    unsafe string GetFilePath(int bufferId);
    void SetCurrentLanguage(LangType language);

    bool SetStatusBar(NppMsg statusBarType, string status);
}

/// <summary>
/// This class holds helpers for sending messages defined in the Msgs_h.cs file. It is at the moment
/// incomplete. Please help fill in the blanks.
/// </summary>
public class NotepadPPGateway : INotepadPPGateway
{
    private const int Unused = 0;
    private nint NppHandle => PluginBase.nppData._nppHandle;

    public void FileNew() => Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_MENUCOMMAND, Unused, NppMenuCmd.IDM_FILE_NEW);

    /// <summary>
    /// Gets the path of the current document.
    /// </summary>
    public string GetCurrentFilePath()
    {
        StringBuilder path = new(Win32.MAX_PATH);
        _ = Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETFULLCURRENTPATH, 0, path);
        return path.ToString();
    }

    /// <summary>
    /// Gets the path of the current document.
    /// </summary>
    public unsafe string GetFilePath(int bufferId)
    {
        StringBuilder path = new(2000);
        _ = Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETFULLPATHFROMBUFFERID, bufferId, path);
        return path.ToString();
    }

    public void SetCurrentLanguage(LangType language) => Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_SETCURRENTLANGTYPE, Unused, (int)language);

    public bool SetStatusBar(NppMsg statusBarType, string str) => Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_SETSTATUSBAR, (int)statusBarType, str) == nint.Zero;

    public long GetBufferEncoding(nint bufferId) => Win32.SendMessage(NppHandle, NppMsg.NPPM_GETBUFFERENCODING, bufferId, 0).ToInt64();

    public void SetMenuItemCheck(int cmdIndex, bool @checked) => Win32.SendMessage(NppHandle, NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[cmdIndex]._cmdID, @checked ? 1 : 0);

    public void SendMenuEncoding(NppEncoding enc)
    {
        switch (enc)
        {
            case NppEncoding.ANSI:
                _ = Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_MENUCOMMAND, 0, (int)NppMenuCmd.IDM_FORMAT_ANSI); break;
            case NppEncoding.UTF8_BOM:
                _ = Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_MENUCOMMAND, 0, (int)NppMenuCmd.IDM_FORMAT_UTF_8); break;
            case NppEncoding.UTF16_LE:
                _ = Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_MENUCOMMAND, 0, (int)NppMenuCmd.IDM_FORMAT_UCS_2LE); break;
            case NppEncoding.UTF16_BE:
                _ = Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_MENUCOMMAND, 0, (int)NppMenuCmd.IDM_FORMAT_UCS_2BE); break;
            case NppEncoding.UTF8:
                _ = Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_MENUCOMMAND, 0, (int)NppMenuCmd.IDM_FORMAT_AS_UTF_8); break;
        }
    }

    public void SetMenuItemCheck(string commandName, bool @checked) => Win32.SendMessage(NppHandle, NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items.First(cmd => cmd._itemName == commandName)._cmdID, @checked ? 1 : 0);
    public void SetMenuItemCheck(FuncItem cmd, bool @checked) => Win32.SendMessage(NppHandle, NppMsg.NPPM_SETMENUITEMCHECK, cmd._cmdID, @checked ? 1 : 0);


    public StringBuilder GetFullPathFromBufferId(nint id)
    {
        StringBuilder path = new(Win32.MAX_PATH);
        _ = Win32.SendMessage(NppHandle, (uint)NppMsg.NPPM_GETFULLPATHFROMBUFFERID, id, path);
        return path;
    }

    public nint GetCurrentBufferId() => Win32.SendMessage(NppHandle, NppMsg.NPPM_GETCURRENTBUFFERID, 0, 0);

    internal void MakeCurrentBufferDirty() => Win32.SendMessage(NppHandle, NppMsg.NPPM_MAKECURRENTBUFFERDIRTY, 0, 0);

    internal void SwitchToFile(StringBuilder path) => Win32.SendMessage(NppHandle, NppMsg.NPPM_SWITCHTOFILE, 0, path);

    internal void SaveCurrentFile() => Win32.SendMessage(NppHandle, NppMsg.NPPM_SAVECURRENTFILE, 0, 0);

    internal void SwitchToFile(string path) => Win32.SendMessage(NppHandle, NppMsg.NPPM_SWITCHTOFILE, 0, path);

    internal void SwitchToFile(string path, IntPtr idFrom) => Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_SWITCHTOFILE, idFrom, path);

    /// <summary>
    /// Set an icon for a command. Use the right <see cref="PluginBase.AddCommand(string, NppFuncItemDelegate, Bitmap, Bitmap)"/> function.
    /// </summary>
    /// <param name="cmdId">The command id. Make sure it is the right id <see cref="FuncItem._cmdID"/></param>
    /// <param name="icon"></param>
    /// <param name="iconDarkMode"></param>
    internal void SetToolbarIcon(int cmdId, Bitmap icon, Bitmap iconDarkMode)
    {
        ToolbarIcons toolbar = new();
#pragma warning disable CA1416 // Plattformkompatibilität überprüfen
        toolbar.hToolbarBmp = icon.GetHbitmap();
        toolbar.hToolbarIcon = icon.GetHicon();
        toolbar.hToolbarIconDarkMode = iconDarkMode.GetHicon();
#pragma warning restore CA1416 // Plattformkompatibilität überprüfen
        IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(toolbar));
        Marshal.StructureToPtr(toolbar, pTbIcons, false);
        _ = Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_ADDTOOLBARICON_FORDARKMODE, cmdId, pTbIcons);
        Marshal.FreeHGlobal(pTbIcons);
    }

    /// <summary>
    /// Set an icon for a command. Use the right <see cref="PluginBase.AddCommand(string, NppFuncItemDelegate, Bitmap, Bitmap)"/> function.
    /// </summary>
    /// <param name="cmdId">The command id. Make sure it is the right id <see cref="FuncItem._cmdID"/></param>
    /// <param name="icon"></param>
    /// <param name="iconDarkMode"></param>
    internal void SetToolbarIcon(int cmdId, Bitmap icon)
    {
        ToolbarIcons toolbar = new();
#pragma warning disable CA1416 // Plattformkompatibilität überprüfen
        toolbar.hToolbarBmp = icon.GetHbitmap();
        toolbar.hToolbarIconDarkMode = toolbar.hToolbarIcon = icon.GetHicon();
#pragma warning restore CA1416 // Plattformkompatibilität überprüfen
        IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(toolbar));
        Marshal.StructureToPtr(toolbar, pTbIcons, false);
        _ = Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_ADDTOOLBARICON_FORDARKMODE, cmdId, pTbIcons);
        Marshal.FreeHGlobal(pTbIcons);
    }

    /// <summary>
    /// Retrieves the path of the plugin config directory.
    /// </summary>
    /// <returns>The plugin config directory.</returns>
    public string GetPluginsConfigDir()
    {
        int length = (int)Win32.SendMessage(NppHandle, NppMsg.NPPM_GETPLUGINSCONFIGDIR, 0, 0);
        StringBuilder sb = new(length+1);
        _ = Win32.SendMessage(NppHandle, NppMsg.NPPM_GETPLUGINSCONFIGDIR, length+1, sb);
        return sb.ToString();
    }
}

/// <summary>
/// This class holds helpers for sending messages defined in the Resource_h.cs file. It is at the moment
/// incomplete. Please help fill in the blanks.
/// </summary>
internal class NppResource
{
    private const int Unused = 0;

    public void ClearIndicator() => Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)Resource.NPPM_INTERNAL_CLEARINDICATOR, Unused, Unused);
}

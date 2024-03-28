// NPP plugin platform for .Net v0.94.00 by Kasper B. Graversen etc.
using System;
using System.Drawing;

namespace CompressedFileViewer.PluginInfrastructure;
internal class PluginBase
{
    internal static Dictionary<int,(Bitmap bmp, Bitmap? bmpDarkMode)> tbIcons = new();

    internal static NppData nppData;
    internal static FuncItems _funcItems = new();




    private static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer, ShortcutKey shortcut, bool checkOnInit)
    {
        FuncItem funcItem = new()
        {
            _cmdID = index,
            _itemName = commandName
        };
        if (functionPointer != null)
            funcItem._pFunc = new NppFuncItemDelegate(functionPointer);
        if (shortcut._key != 0)
            funcItem._pShKey = shortcut;
        funcItem._init2Check = checkOnInit;
        _funcItems.Add(funcItem);
    }
    private static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer, ShortcutKey shortcut, bool checkOnInit, Bitmap toolbarIcon)
    {
        SetCommand(index, commandName, functionPointer, shortcut, checkOnInit);
        tbIcons[index] = (toolbarIcon, null);
    }
    private static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer, ShortcutKey shortcut, bool checkOnInit, Bitmap toolbarIcon, Bitmap toolbarIconDarkMode)
    {
        SetCommand(index, commandName, functionPointer, shortcut, checkOnInit);
        tbIcons[index] = (toolbarIcon, toolbarIconDarkMode);
    }

    /// <summary>
    /// Adds a command to the menu. You should call this method in PluginInit.
    /// </summary>
    /// <param name="commandName">The name of of the command or "---" for a horizontal line.</param>
    /// <param name="functionPointer">The function that should be called.</param>
    /// <param name="shortcut">The shortcut to call the command.</param>
    /// <param name="checkOnInit">Whether to the menu item is checked. To change the checked state use <see cref="NotepadPPGateway.SetMenuItemCheck(string, bool)"/>.</param>
    /// <param name="toolbarIcon">The icon that should be added to the toolbar, as a shortcut for the command.<br/>
    /// The bitmap should be 16x16. Do not dispose of the bitmap.
    /// </param>
    /// /// <param name="toolbarIconDarkMode">The icon that should be added to the toolbar in dark mode, as a shortcut for the command.<br/>
    /// The bitmap should be 16x16. Do not dispose of the bitmap.
    /// </param>
    internal static void AddCommand(string commandName, NppFuncItemDelegate functionPointer) => SetCommand(_funcItems.Items.Count, commandName, functionPointer, new ShortcutKey(), false);
    /// <summary>
    /// Adds a command to the menu. You should call this method in PluginInit.
    /// </summary>
    /// <param name="commandName">The name of of the command or "---" for a horizontal line.</param>
    /// <param name="functionPointer">The function that should be called.</param>
    /// <param name="shortcut">The shortcut to call the command.</param>
    /// <param name="checkOnInit">Whether to the menu item is checked. To change the checked state use <see cref="NotepadPPGateway.SetMenuItemCheck(string, bool)"/>.</param>
    /// <param name="toolbarIcon">The icon that should be added to the toolbar, as a shortcut for the command.<br/>
    /// The bitmap should be 16x16. Do not dispose of the bitmap.
    /// </param>
    /// /// <param name="toolbarIconDarkMode">The icon that should be added to the toolbar in dark mode, as a shortcut for the command.<br/>
    /// The bitmap should be 16x16. Do not dispose of the bitmap.
    /// </param>
    internal static void AddCommand(string commandName, NppFuncItemDelegate functionPointer, Bitmap toolbarIcon) => SetCommand(_funcItems.Items.Count, commandName, functionPointer, new ShortcutKey(), false, toolbarIcon);
    /// <summary>
    /// Adds a command to the menu. You should call this method in PluginInit.
    /// </summary>
    /// <param name="commandName">The name of of the command or "---" for a horizontal line.</param>
    /// <param name="functionPointer">The function that should be called.</param>
    /// <param name="shortcut">The shortcut to call the command.</param>
    /// <param name="checkOnInit">Whether to the menu item is checked. To change the checked state use <see cref="NotepadPPGateway.SetMenuItemCheck(string, bool)"/>.</param>
    /// <param name="toolbarIcon">The icon that should be added to the toolbar, as a shortcut for the command.<br/>
    /// The bitmap should be 16x16. Do not dispose of the bitmap.
    /// </param>
    /// /// <param name="toolbarIconDarkMode">The icon that should be added to the toolbar in dark mode, as a shortcut for the command.<br/>
    /// The bitmap should be 16x16. Do not dispose of the bitmap.
    /// </param>
    internal static void AddCommand(string commandName, NppFuncItemDelegate functionPointer, Bitmap toolbarIcon, Bitmap toolbarIconDarkMode) => SetCommand(_funcItems.Items.Count, commandName, functionPointer, new ShortcutKey(), false, toolbarIcon, toolbarIconDarkMode);

    /// <summary>
    /// Adds a command to the menu. You should call this method in PluginInit.
    /// </summary>
    /// <param name="commandName">The name of of the command or "---" for a horizontal line.</param>
    /// <param name="functionPointer">The function that should be called.</param>
    /// <param name="shortcut">The shortcut to call the command.</param>
    /// <param name="checkOnInit">Whether to the menu item is checked. To change the checked state use <see cref="NotepadPPGateway.SetMenuItemCheck(string, bool)"/>.</param>
    /// <param name="toolbarIcon">The icon that should be added to the toolbar, as a shortcut for the command.<br/>
    /// The bitmap should be 16x16. Do not dispose of the bitmap.
    /// </param>
    /// /// <param name="toolbarIconDarkMode">The icon that should be added to the toolbar in dark mode, as a shortcut for the command.<br/>
    /// The bitmap should be 16x16. Do not dispose of the bitmap.
    /// </param>
    internal static void AddCommand(string commandName, NppFuncItemDelegate functionPointer, ShortcutKey shortcut) => SetCommand(_funcItems.Items.Count, commandName, functionPointer, shortcut, false);
    /// <summary>
    /// Adds a command to the menu. You should call this method in PluginInit.
    /// </summary>
    /// <param name="commandName">The name of of the command or "---" for a horizontal line.</param>
    /// <param name="functionPointer">The function that should be called.</param>
    /// <param name="shortcut">The shortcut to call the command.</param>
    /// <param name="checkOnInit">Whether to the menu item is checked. To change the checked state use <see cref="NotepadPPGateway.SetMenuItemCheck(string, bool)"/>.</param>
    /// <param name="toolbarIcon">The icon that should be added to the toolbar, as a shortcut for the command.<br/>
    /// The bitmap should be 16x16. Do not dispose of the bitmap.
    /// </param>
    /// /// <param name="toolbarIconDarkMode">The icon that should be added to the toolbar in dark mode, as a shortcut for the command.<br/>
    /// The bitmap should be 16x16. Do not dispose of the bitmap.
    /// </param>
    internal static void AddCommand(string commandName, NppFuncItemDelegate functionPointer, ShortcutKey shortcut, Bitmap toolbarIcon) => SetCommand(_funcItems.Items.Count, commandName, functionPointer, shortcut, false, toolbarIcon);
    /// <summary>
    /// Adds a command to the menu. You should call this method in PluginInit.
    /// </summary>
    /// <param name="commandName">The name of of the command or "---" for a horizontal line.</param>
    /// <param name="functionPointer">The function that should be called.</param>
    /// <param name="shortcut">The shortcut to call the command.</param>
    /// <param name="checkOnInit">Whether to the menu item is checked. To change the checked state use <see cref="NotepadPPGateway.SetMenuItemCheck(string, bool)"/>.</param>
    /// <param name="toolbarIcon">The icon that should be added to the toolbar, as a shortcut for the command.<br/>
    /// The bitmap should be 16x16. Do not dispose of the bitmap.
    /// </param>
    /// /// <param name="toolbarIconDarkMode">The icon that should be added to the toolbar in dark mode, as a shortcut for the command.<br/>
    /// The bitmap should be 16x16. Do not dispose of the bitmap.
    /// </param>
    internal static void AddCommand(string commandName, NppFuncItemDelegate functionPointer, ShortcutKey shortcut, Bitmap toolbarIcon, Bitmap toolbarIconDarkMode) => SetCommand(_funcItems.Items.Count, commandName, functionPointer, shortcut, false, toolbarIcon, toolbarIconDarkMode);

    /// <summary>
    /// Adds a command to the menu. You should call this method in PluginInit.
    /// </summary>
    /// <param name="commandName">The name of of the command or "---" for a horizontal line.</param>
    /// <param name="functionPointer">The function that should be called.</param>
    /// <param name="shortcut">The shortcut to call the command.</param>
    /// <param name="checkOnInit">Whether to the menu item is checked. To change the checked state use <see cref="NotepadPPGateway.SetMenuItemCheck(string, bool)"/>.</param>
    /// <param name="toolbarIcon">The icon that should be added to the toolbar, as a shortcut for the command.<br/>
    /// The bitmap should be 16x16. Do not dispose of the bitmap.
    /// </param>
    /// /// <param name="toolbarIconDarkMode">The icon that should be added to the toolbar in dark mode, as a shortcut for the command.<br/>
    /// The bitmap should be 16x16. Do not dispose of the bitmap.
    /// </param>
    internal static void AddCommand(string commandName, NppFuncItemDelegate functionPointer, bool checkOnInit) => SetCommand(_funcItems.Items.Count, commandName, functionPointer, new ShortcutKey(), checkOnInit);

    /// <summary>
    /// Adds a command to the menu. You should call this method in PluginInit.
    /// </summary>
    /// <param name="commandName">The name of of the command or "---" for a horizontal line.</param>
    /// <param name="functionPointer">The function that should be called.</param>
    /// <param name="shortcut">The shortcut to call the command.</param>
    /// <param name="checkOnInit">Whether to the menu item is checked. To change the checked state use <see cref="NotepadPPGateway.SetMenuItemCheck(string, bool)"/>.</param>
    /// <param name="toolbarIcon">The icon that should be added to the toolbar, as a shortcut for the command.<br/>
    /// The bitmap should be 16x16. Do not dispose of the bitmap.
    /// </param>
    /// /// <param name="toolbarIconDarkMode">The icon that should be added to the toolbar in dark mode, as a shortcut for the command.<br/>
    /// The bitmap should be 16x16. Do not dispose of the bitmap.
    /// </param>
    internal static void AddCommand(string commandName, NppFuncItemDelegate functionPointer, bool checkOnInit, Bitmap toolbarIcon) => SetCommand(_funcItems.Items.Count, commandName, functionPointer, new ShortcutKey(), checkOnInit, toolbarIcon);

    /// <summary>
    /// Adds a command to the menu. You should call this method in PluginInit.
    /// </summary>
    /// <param name="commandName">The name of of the command or "---" for a horizontal line.</param>
    /// <param name="functionPointer">The function that should be called.</param>
    /// <param name="checkOnInit">Whether to the menu item is checked. To change the checked state use <see cref="NotepadPPGateway.SetMenuItemCheck(string, bool)"/>.</param>
    /// <param name="toolbarIcon">The icon that should be added to the toolbar, as a shortcut for the command.<br/>
    /// The bitmap should be 16x16. Do not dispose of the bitmap.
    /// </param>
    /// /// <param name="toolbarIconDarkMode">The icon that should be added to the toolbar in dark mode, as a shortcut for the command.<br/>
    /// The bitmap should be 16x16. Do not dispose of the bitmap.
    /// </param>
    internal static void AddCommand(string commandName, NppFuncItemDelegate functionPointer, bool checkOnInit, Bitmap toolbarIcon, Bitmap toolbarIconDarkMode) => SetCommand(_funcItems.Items.Count, commandName, functionPointer, new ShortcutKey(), checkOnInit, toolbarIcon, toolbarIconDarkMode);

    /// <summary>
    /// Adds a command to the menu. You should call this method in PluginInit.
    /// </summary>
    /// <param name="commandName">The name of of the command or "---" for a horizontal line.</param>
    /// <param name="functionPointer">The function that should be called.</param>
    /// <param name="shortcut">The shortcut to call the command.</param>
    /// <param name="checkOnInit">Whether to the menu item is checked. To change the checked state use <see cref="NotepadPPGateway.SetMenuItemCheck(string, bool)"/>.</param>
    internal static void AddCommand(string commandName, NppFuncItemDelegate functionPointer, ShortcutKey shortcut, bool checkOnInit) => SetCommand(_funcItems.Items.Count, commandName, functionPointer, shortcut, checkOnInit);

    /// <summary>
    /// Adds a command to the menu. You should call this method in PluginInit.
    /// </summary>
    /// <param name="commandName">The name of of the command or "---" for a horizontal line.</param>
    /// <param name="functionPointer">The function that should be called.</param>
    /// <param name="shortcut">The shortcut to call the command.</param>
    /// <param name="checkOnInit">Whether to the menu item is checked. To change the checked state use <see cref="NotepadPPGateway.SetMenuItemCheck(string, bool)"/>.</param>
    /// <param name="toolbarIcon">The icon that should be added to the toolbar, as a shortcut for the command.<br/>
    /// The bitmap should be 16x16. Do not dispose of the bitmap.
    /// </param>
    internal static void AddCommand(string commandName, NppFuncItemDelegate functionPointer, ShortcutKey shortcut, bool checkOnInit, Bitmap toolbarIcon) => SetCommand(_funcItems.Items.Count, commandName, functionPointer, shortcut, checkOnInit, toolbarIcon);

    /// <summary>
    /// Adds a command to the menu. You should call this method in PluginInit.
    /// </summary>
    /// <param name="commandName">The name of of the command or "---" for a horizontal line.</param>
    /// <param name="functionPointer">The function that should be called.</param>
    /// <param name="shortcut">The shortcut to call the command.</param>
    /// <param name="checkOnInit">Whether to the menu item is checked. To change the checked state use <see cref="NotepadPPGateway.SetMenuItemCheck(string, bool)"/>.</param>
    /// <param name="toolbarIcon">The icon that should be added to the toolbar, as a shortcut for the command.<br/>
    /// The bitmap should be 16x16. Do not dispose of the bitmap.
    /// </param>
    /// /// <param name="toolbarIconDarkMode">The icon that should be added to the toolbar in dark mode, as a shortcut for the command.<br/>
    /// The bitmap should be 16x16. Do not dispose of the bitmap.
    /// </param>
    internal static void AddCommand(string commandName, NppFuncItemDelegate functionPointer, ShortcutKey shortcut, bool checkOnInit, Bitmap toolbarIcon, Bitmap toolbarIconDarkMode) => SetCommand(_funcItems.Items.Count, commandName, functionPointer, shortcut, checkOnInit, toolbarIcon, toolbarIconDarkMode);

    internal static void AddMenuSeperator() => AddCommand("---", null!);




    public static void SetToolbar()
    {
        foreach (var tbIcon in tbIcons)
        {
            if (tbIcon.Value.bmpDarkMode == null)
                Plugin.NppGateway.SetToolbarIcon(_funcItems.Items[tbIcon.Key]._cmdID, tbIcon.Value.bmp);
            else
                Plugin.NppGateway.SetToolbarIcon(_funcItems.Items[tbIcon.Key]._cmdID, tbIcon.Value.bmp, tbIcon.Value.bmpDarkMode);
        }
    }

    internal static nint GetCurrentScintillaHandle()
    {
        _ = Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_GETCURRENTSCINTILLA, 0, out int curScintilla);
        return curScintilla == 0 ? nppData._scintillaMainHandle : nppData._scintillaSecondHandle;
    }

    public static ScintillaGateway GetCurrentScintilla() => new(GetCurrentScintillaHandle());

    internal static void CleanUp()
    {
        _funcItems.Dispose();
        foreach (var tbIcon in tbIcons)
        {
            tbIcon.Value.bmp.Dispose();
            tbIcon.Value.bmpDarkMode?.Dispose();
        }
        tbIcons.Clear();
    }
}

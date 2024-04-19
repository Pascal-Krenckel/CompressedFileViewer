<img src="./CompressedFileViewer/icons/gzip.png" style="float:left;" width=64  />

# CompressedFileViewer
A Notepad++ plugin to open and save compressed files.
It's written in `C#` with Visual Studio for .Net 8.
Don't change the encoding (`UTF-8`, `ANSI`, etc.) of a compressed file. Change the file to uncompressed, then change the encoding and toggle compression on again.

## Compression algorithms
* GZip (`.gz`, `.gzip`)
* Bzip2 (`.bz2`, `.bzip2`)
* Zstd (`.zstd`)
* XZ (`.xz`)
* Brotli (`.br`)

## How to use

### Dll-Files
Download the zip file depending on your architecture. 
Unpack the zip file and copy all files into `%NotepadDir%/plugins/CompressedFileViewer`.
Make sure .Net 8 is installed.

### Terminus:
1. Tracked file: A file that has been decompressed or manually selected for compression. The icon and menu entry will be checked for this file.
2. Excluded file: A file for which compression has been manually disabled.
3. Ignored file: A file that is neither tracked for compression nor excluded.
4. gz: Any file with a suffix matching one in the settings.
5. non-gz: Any other file

### Settings
This plugin has two basic settings.
1. `Try to decompress all files`: If set, the plugin will try to decompress all files regardless of the extension. All decompressed files will be tracked and - if saved - automatically compressed. (If the path is still the same)
   Only active algorithms will be used. 
2. `Compression algorithms`: The ordered list of the compression algorithms. This does not affect file detection or 'decompress all'. It only affects the behaviour of the icon or 'Toggle Compression'. When clicking on the icon based on the suffix, the appropriate compression will be chosen.  Afterwards, you can iterate through all algorithms in the list by clicking again.
    * Is enabled: If unchecked, the algorithm will be disabled. No files will be compressed or uncompressed using this algorithm. To update the toolbar you need to restart npp.
    * Is active: If an algorithm is not active, it will not be used with the setting `Try decompress all files`. Additionally, using the toolbar icon will only switch to this algorithm if it has the correct suffix. Files with the correct suffix will still automatically be (de)compressed when opened/saved.

Selecting an algorithm and clicking on 'Settings' allows you to manage the algorithm-specific settings.
1. 'Suffixes': The list of suffixes that should automatically be decompressed. If a file is saved with such a suffix, it will also be compressed.
2. Other algorithm-specific settings

### Commands
In the menubar, there are 8+ Commands:
1. `Toggle Compression` (same as clicking on the icon): Changes the compression algorithm used when storing this file. You can iterate through different algorithms again. The order is determined by the order in the settings. If the file has an algorithm-specific suffix, it will select this algorithm first. By clicking again after the last algorithm was selected, the file compression will be disabled for this file.
2. `Compress`: Compresses the current file text in the editor. 
3. `Decompress`: Decompresses the current text in the editor.
4. `Show Log`: Opens the log window.
5. `Settings`: Opens the settings dialog.
6. `About`: Opens the about dialog.
7. `Credits`: Opens the credits dialog.
8. '*Compression Algorithm*': Sets the compression for this specific file or removes it.

### (De)Compression-File-Rules
On Open:
1. gz-suffix: It will be decompressed if possible. It will be tracked regardless.
2. no-gz-suffix: If `Try decompress all` then the plugin tries to decompress it. <br/>If possible it will be tracked. <br/>If not or not 'Try decompress all' the file will be ignored.

Save (same path):
1. Any tracked file will be compressed.
2. Any excluded file won't be compressed.
3. Any ignored file will be compressed if the suffix matches an algorithm suffix. (Won't happen since these files would be tracked)

Save (different path):
Npp will tell the plugin the old path when notifying `FileBeforeSaved`. If based on the suffix the compression toggles it might be saved two times.
1. from suffix type to other suffix type: This file will be compressed based on the new suffix.
2. same suffix type:<br/>
   1. If tracked, save compressed.
   2. If excluded, save uncompressed.
   3. If neither, go by suffix type.
   
Copy:
1. Will always be stored as seen in the editor since npp won't raise a `FileBeforeSave`/`FileSaved` event.

## Compiling yourself

To compile this plugin with Visual Studio, you need .Net 8 (6 or 7 might also work).
See DNNE (https://github.com/AaronRobinsonMSFT/DNNE) for additional requirements.
Change `NppDir32/64` to the correct Npp-Folders and make sure you have write access to the plugin dir.

### Implementing Additional Algorithms
* Implementing CompressionSettings
* Preferences:
   * Incrementing Version
   * Updating Deserialize(Stream)
   * Adding new CompressionSetting as Property
   * Updating Default
* Creating new SettingsDialog (see GZipSettingsDialog) to change compression settings

## Credits
Notepad++: https://notepad-plus-plus.org/

This project uses the notepad++ plugin template from https://github.com/Pascal-Krenckel/NppPluginTemplate
     based on: https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net
     but its using DNNE for dll exports: https://github.com/AaronRobinsonMSFT/DNNE

Visual Studio: https://visualstudio.microsoft.com/de/

The Icons were created by Freepik from www.flaticon.com.

This project uses SharpZipLib: https://github.com/icsharpcode/SharpZipLib

This project uses Joveler.Compression.XZ: https://github.com/ied206/Joveler.Compression

This project uses ZstdSharp: https://github.com/oleg-st/ZstdSharp



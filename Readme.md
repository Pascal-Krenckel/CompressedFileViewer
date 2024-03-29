<img src="./CompressedFileViewer/icons/gzip.png" style="float:left;" width=64  />

# CompressedFileViewer
A Notepad++ plugin to open and save files in the gzip or bzip2 format.
It's written in C# with Visual Studio for .Net 8.
Don't change the encoding of a zipped file. Change the file to uncompressed, then change the encoding and toggle compression on again.

## Compression algorithms
* GZip (.gz, .gzip)
* Bzip2 (.bz2, .bzip2)
* Zstd (.zstd)
* XZ (.xz)
## How to use

### Dll-Files
Download the zip file depending on your architecture. 
Unpack the zip file copy all files into %NotepadDir%/plugins/CompressedFileViewer.
Make sure .Net 8 is installed.

### Terminus:
1. tracked file: A tracked file is a file that was decompressed or is selected manually for compression. The Icon and Menu Entry will be checked for this file.
2. excluded file: A file for which the compression was manually disabled.
3. ignored file: neither tracked for compression nor excluded.
4. gz: Any file with a suffix matching one in the settings.
5. non-gz: Any other file

### Settings
This plugin has two settings.
1. 'Try to decompress all files': If set the plugin will try to decompress all files regardless of the extension. All decompressed files will be tracked and if saved automatically compressed. (If the path is still the same)
2. 'Compression algorithms': The ordered list of the compression algorithms. This does not affect file detection or 'decompress all'. It only affects the behaviour of the icon or 'Toogle Compression'. When clicking on the icon based on the suffix the appropriate compression will be chosen.  Afterwards you can iterate through all algorithms in the list by clicking again.

Selecting a algorithm and clicking on 'Settings' allows to manage the algorithm specific settings.
1. 'Suffixes': The list of suffixes that schould automatically be decompressed. If a file is saved with such suffix it will also be compressed.
2. Other algorithm specific settings

### Commands
In the menubar there are 6 Commands:
1. Toggle Compression (same as clicking on the icon): Changes the compression algorithm used when storing this file. You can iteratre through different algorithms again. The order is dertemined by the order in the settings. If the file has a algorithm specific suffix it will select this algorithm first. By click again after the last algorithm was selected, the file compression will be disable for this file.
2. Compress: Compresses the current file text in the editor. 
3. Decompress: Decompresses the current text in the editor.
4. Show Log: Opens the log window.
5. Settings: Opens the settings dialog.
6. About: Opens the about dialog.
7. Credits: Opens the credits dialog.
9. '*Compression Algorithm*': Sets the compression for this specific file or removes it.

### (De)Compression-File-Rules
On Open:
1. gz-suffix: It will be decompressed if possible. It will be tracked regardless.
2. no-gz-suffix: If 'Try decompress all' then the plugin tries to decompress it. <br/>If possible it will be tracked. <br/>If not or not 'Try decompress all' the file will be ignored.

Save (same path):
1. Any tracked file will be compressed.
2. Any excluded file won't be compressed.
3. Any ignored file will be compressed if the suffix matches an algorithm suffix. (Won't happen since this files would be tracked)

Save (diffrent path):
Npp will tell the plugin the old path when notifing "FileBeforeSaved". If based on the suffix the compression toggles it might be saved two times.
1. from suffix type to other suffix type: This file will be compressed based on the new suffix.
2. same suffix type:<br/>
   1. If tracked, save compressed..
   2. If excluded, save uncompressed.
   3. If neither go by suffix type.
   
Copy:
1. Will always be stored as seen in the editor since npp won't raise a FileBeforeSave/FileSaved event.

## Compiling yourself

To compile this plugin with Visual Studio, you need .Net 8 (6 or 7 might also work).
See DNNE (https://github.com/AaronRobinsonMSFT/DNNE) for additional requirements.
Change the NppDir32/64 to point to the right Npp-Folder and make soure you have write access to the plugin dir.

### Implementing Addtional Algorithms
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



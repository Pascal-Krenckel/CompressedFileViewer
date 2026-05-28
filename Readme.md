<img src="./CompressedFileViewer/icons/gzip.png" style="float:left;" width=64  />

# CompressedFileViewer

A Notepad++ plugin for opening and saving compressed files.

It is written in `C#` with Visual Studio for `.NET 8`.

Do not change the encoding (`UTF-8`, `ANSI`, etc.) of a compressed file directly.
First toggle compression off, then change the encoding, and finally enable compression again.

---

## Compression Algorithms

* GZip (`.gz`, `.gzip`)
* BZip2 (`.bz2`, `.bzip2`)
* Zstandard (`.zstd`)
* XZ (`.xz`)
* Brotli (`.br`)

---

## Typical Workflow

1. Open a compressed file such as `.gz`.
2. The plugin automatically decompresses the file in the editor.
3. Edit the file normally.
4. Save the file.
5. The plugin automatically recompresses the file.

Use `Toggle Compression` to manually enable or disable compression tracking for a file.

---

## How to Use

### DLL Files

Download the ZIP file matching your system architecture.

Extract the ZIP file and copy its contents into:

```text
%NotepadDir%/plugins/CompressedFileViewer
```

Make sure `.NET 8` is installed.

Open the plugin settings to enable the desired compression algorithms, since all algorithms are disabled by default.

---

## Terminology

1. **Tracked file**
   A file that has been decompressed or manually selected for compression.
   The toolbar icon and menu entry are checked for this file.

2. **Excluded file**
   A file for which compression has been manually disabled.

3. **Ignored file**
   A file that is neither tracked nor excluded.

4. **Compressed file**
   A file whose extension matches one of the configured compression suffixes.

5. **Non-compressed file**
   Any other file.

---

## Settings

This plugin has two primary settings.

### 1. `Try to decompress all files`

If enabled, the plugin attempts to decompress all files regardless of extension.

All successfully decompressed files become tracked and will automatically be recompressed when saved, as long as the file path remains unchanged.

Only active algorithms are used.

### Dll-Files
Download the zip file depending on your architecture. 
Unpack the zip file and copy all files into `%NotepadDir%/plugins/CompressedFileViewer`.
Make sure [.Net 8 - Desktop](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) is installed.
Go to settings to enable the different algorithms, since all are disabled by default.

### 2. `Compression algorithms`

Defines the ordered list of compression algorithms.

This setting does not affect automatic file detection or `Try to decompress all files`.
It only affects the behavior of the toolbar icon and the `Toggle Compression` command.

When clicking the toolbar icon, the plugin first selects the algorithm matching the current file suffix. Clicking again cycles through the remaining algorithms in the configured order.

Each algorithm has two independent states:

#### Enabled

If disabled:

* The algorithm is completely unavailable.
* Files will not be compressed or decompressed using it.
* Toolbar and menu entries require restarting Notepad++ after changes.

#### Active

If inactive:

* The algorithm is ignored by `Try to decompress all files`.
* The toolbar toggle skips this algorithm unless the file extension explicitly matches it.
* Files with matching extensions are still automatically compressed and decompressed.

That means:

* Disabled algorithms are never used.
* Enabled algorithms are used for matching file extensions and explicitly selected files.
* If `Try to decompress all files` is enabled, files without a matching extension may still be decompressed if an active algorithm succeeds.
* The toolbar icon and `Toggle Compression` cycle through all active algorithms.
* Restarting Notepad++ is required after changing enabled algorithms to update menus and toolbar entries.

---

### Algorithm-Specific Settings

Select an algorithm and click `Settings` to configure algorithm-specific options.

1. `Suffixes`
   The list of file suffixes that should automatically be decompressed.
   Files saved with these suffixes are automatically compressed.

2. Other algorithm-specific settings

---

## Commands

The plugin adds the following commands to the menu:

1. `Toggle Compression`
   Toggles compression tracking for the current file.
   Repeatedly invoking the command cycles through the configured algorithms in order.
   If the file has an algorithm-specific suffix, that algorithm is selected first.
   After the final algorithm, compression is disabled for the file.

2. `Compress`
   Compresses the current editor contents.

3. `Decompress`
   Decompresses the current editor contents.

4. `Show Log`
   Opens the log window.

5. `Settings`
   Opens the settings dialog.

6. `About`
   Opens the about dialog.

7. `Credits`
   Opens the credits dialog.

8. `*Compression Algorithm*`
   Sets or removes compression for the current file.

---

## File Compression Rules

### On Open

1. Matching compression suffix
   The file is decompressed if possible and becomes tracked.

2. Non-matching suffix
   If `Try to decompress all files` is enabled, the plugin attempts to decompress the file.

   * If decompression succeeds, the file becomes tracked.
   * Otherwise, the file is ignored.

---

### Save (same path)

1. Tracked files are compressed.
2. Excluded files are saved uncompressed.
3. Ignored files are compressed if the suffix matches a configured algorithm suffix.

Normally, ignored files with matching suffixes do not occur because they would already be tracked.

---

### Save (different path)

Notepad++ provides the old path during the `FileBeforeSaved` event.

If the new file suffix changes compression behavior, the file may be saved twice.

1. Different suffix type
   Compression is determined by the new suffix.

2. Same suffix type

   1. Tracked files are saved compressed.
   2. Excluded files are saved uncompressed.
   3. Otherwise, compression is determined by the suffix.

---

### Copy

Copied files are always stored exactly as shown in the editor because Notepad++ does not emit `FileBeforeSave` or `FileSaved` events during copy operations.

---

## Compiling

To compile this plugin with Visual Studio, you need `.NET 8`.
(`.NET 6` or `.NET 7` may also work.)

See DNNE for additional requirements:

https://github.com/AaronRobinsonMSFT/DNNE

Set `NppDir32` and `NppDir64` to the correct Notepad++ directories and ensure you have write access to the plugin directory.

---

## Implementing Additional Algorithms

Required steps:

1. Implement `CompressionSettings`

2. Update `Preferences`

   * Increment `Version`
   * Update `Deserialize(Stream)`
   * Add the new `CompressionSetting` property
   * Update `Default`

3. Create a settings dialog

   * See `GZipSettingsDialog` for an example

---

## Credits

* Notepad++
  https://notepad-plus-plus.org/

* This project uses the Notepad++ plugin template:
  https://github.com/Pascal-Krenckel/NppPluginTemplate

* Based on:
  https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net

* Uses DNNE for DLL exports:
  https://github.com/AaronRobinsonMSFT/DNNE

* Visual Studio
  https://visualstudio.microsoft.com/

* The icons were created by Freepik from Flaticon
  https://www.flaticon.com/

* This project uses SharpZipLib
  https://github.com/icsharpcode/SharpZipLib

* This project uses Joveler.Compression.XZ
  https://github.com/ied206/Joveler.Compression

* This project uses ZstdSharp
  https://github.com/oleg-st/ZstdSharp

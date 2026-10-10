using CodeWF.Toolkit.Logging;
using System;
using System.IO;

namespace CodeWF.Avalonia.Log.Platform;

internal static class LogFolderLauncher
{
    public static bool Open(string? logFolder)
    {
        if (string.IsNullOrWhiteSpace(logFolder)) return false;
        logFolder = Path.GetFullPath(logFolder);
        if (!Directory.Exists(logFolder))
        {
            Directory.CreateDirectory(logFolder);
        }

        FolderOpener.OpenFolder(logFolder);
        return true;
    }
}

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CodeWF.Avalonia.Log.Platform;

/// <summary>跨平台打开文件夹（原依赖基础库的同名能力就地内联，避免跨仓库依赖）。</summary>
internal static class FolderOpener
{
    public static void OpenFolder(string folderFullName)
    {
        if (!Directory.Exists(folderFullName))
        {
            throw new DirectoryNotFoundException("The specified folder does not exist：" + folderFullName);
        }

        var normalizedPath = folderFullName.Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo("Explorer.exe") { Arguments = normalizedPath });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", $"\"{normalizedPath}\"");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", $"\"{normalizedPath}\"");
        }
    }
}

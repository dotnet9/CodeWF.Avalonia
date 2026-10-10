using Avalonia;
using CodeWF.Avalonia.Log.Notifications.Styles;
using System;
using System.Linq;

namespace CodeWF.Avalonia.Log;

internal static class LogNotificationResources
{
    private static readonly object SyncRoot = new();

    public static void EnsureRegistered()
    {
        var application = Application.Current;
        if (application == null)
        {
            return;
        }

        lock (SyncRoot)
        {
            var dictionaries = application.Resources.MergedDictionaries;
            if (dictionaries.OfType<NotificationResources>().Any()) return;
            dictionaries.Insert(0, new NotificationResources());
        }
    }
}

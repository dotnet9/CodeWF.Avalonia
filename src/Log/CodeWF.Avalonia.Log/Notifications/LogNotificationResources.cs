using Avalonia;
using Avalonia.Styling;
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
            // 默认通知样式放在最低优先级，宿主资源及后加载的品牌主题可以直接覆盖同键。
            if (application.Styles.OfType<NotificationResourceStyles>().Any()) return;
            application.Styles.Insert(0, new NotificationResourceStyles());
        }
    }

    private sealed class NotificationResourceStyles : Styles
    {
        public NotificationResourceStyles() => Resources = new NotificationResources();
    }
}

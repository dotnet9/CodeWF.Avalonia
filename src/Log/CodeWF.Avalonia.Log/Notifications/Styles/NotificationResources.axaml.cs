using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CodeWF.Avalonia.Log.Notifications.Styles;

internal sealed partial class NotificationResources : ResourceDictionary
{
    public NotificationResources() => AvaloniaXamlLoader.Load(this);
}

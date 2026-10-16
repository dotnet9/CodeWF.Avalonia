using Avalonia.Media;

namespace CodeWF.Avalonia.Log;

internal sealed record LogViewPalette(
    IBrush TimestampForeground,
    IBrush ContentForeground,
    IBrush TraceForeground,
    IBrush InformationForeground,
    IBrush WarningForeground,
    IBrush ErrorForeground)
{
    internal static LogViewPalette Light { get; } = new(
        new SolidColorBrush(Color.Parse("#667085")),
        new SolidColorBrush(Color.Parse("#262626")),
        new SolidColorBrush(Color.Parse("#0969DA")),
        new SolidColorBrush(Color.Parse("#237B25")),
        new SolidColorBrush(Color.Parse("#9A6700")),
        new SolidColorBrush(Color.Parse("#CF222E")));
}

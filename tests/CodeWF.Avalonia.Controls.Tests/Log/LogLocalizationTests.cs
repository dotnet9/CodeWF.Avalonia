using CodeWF.Avalonia.Log;
using CodeWF.Toolkit.Lang.Json;
using Xunit;

namespace CodeWF.Avalonia.Controls.Tests.Log;

public sealed class LogLocalizationTests
{
    [Theory]
    [InlineData("en-US", "Copy", "Clear", "Open log folder")]
    [InlineData("zh-CN", "\u590d\u5236", "\u6e05\u7a7a", "\u6253\u5f00\u65e5\u5fd7\u76ee\u5f55")]
    [InlineData("fr-FR", "Copy", "Clear", "Open log folder")]
    public void EmbeddedMenusWorkWithoutHostResourceRegistration(string culture, string copy, string clear, string location)
    {
        var resources = new JsonLocalizer(typeof(LogView).Assembly, allowApplicationOverrides: false);
        Assert.Equal(copy, resources.GetResource("Log.View.Copy", culture));
        Assert.Equal(clear, resources.GetResource("Log.View.Clear", culture));
        Assert.Equal(location, resources.GetResource("Log.View.Location", culture));
    }
}

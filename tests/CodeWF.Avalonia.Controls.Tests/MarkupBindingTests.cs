using CodeWF.Avalonia.Controls.Markup;
using Avalonia.Data;
using System.Globalization;

namespace CodeWF.Avalonia.Controls.Tests;

public sealed class MarkupBindingTests
{
    [Fact]
    public void IfBindingReturnsTheTrueValue()
    {
        var binding = new IfBinding(true, "enabled", "disabled");

        var result = binding.ProvideValue(null!);

        Assert.Equal("enabled", result);
    }

    [Fact]
    public void IfBindingReturnsTheFalseValue()
    {
        var binding = new IfBinding(false, "enabled", "disabled");

        var result = binding.ProvideValue(null!);

        Assert.Equal("disabled", result);
    }

    [Fact]
    public void SwitchBindingMatchesCaseAndUsesDefault()
    {
        var binding = new SwitchBinding(
            "warning",
            [new SwitchCase { When = "warning", Then = 2 }],
            0);

        Assert.Equal(2, binding.ProvideValue(null!));

        var defaultBinding = new SwitchBinding(
            "unknown",
            [new SwitchCase { When = "warning", Then = 2 }],
            0);

        Assert.Equal(0, defaultBinding.ProvideValue(null!));
    }

    [Fact]
    public void SwitchBindingReevaluatesBoundCaseKeysAndResults()
    {
        var expression = new SwitchBinding("running",
            [new SwitchCase { When = new Binding("Expected"), Then = new Binding("Result") }], "unknown");
        var binding = Assert.IsType<MultiBinding>(expression.ProvideValue(null!));
        var converter = binding.Converter!;

        Assert.Equal("active", converter.Convert(["running", "active"], typeof(string), null, CultureInfo.InvariantCulture));
        Assert.Equal("unknown", converter.Convert(["idle", "active"], typeof(string), null, CultureInfo.InvariantCulture));
        Assert.Equal("translated", converter.Convert(["running", "translated"], typeof(string), null, CultureInfo.InvariantCulture));
    }
}

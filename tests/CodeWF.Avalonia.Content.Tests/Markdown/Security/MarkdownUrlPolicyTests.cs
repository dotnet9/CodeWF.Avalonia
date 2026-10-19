using CodeWF.Avalonia.Markdown.Shared.Rendering;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Security;

public sealed class MarkdownUrlPolicyTests
{
	[Theory]
	[InlineData("https://github.com", "https://evil.example", true)]
	[InlineData("https://github.com", "http://github.com.evil.io", true)]
	[InlineData("https://github.com", "https://github.com.evil.io", true)]
	[InlineData("www.example.com", "https://other.example", true)]
	[InlineData("http://github.com", "https://github.com/different-path", false)]
	[InlineData("点击这里", "https://evil.example", false)]
	[InlineData("https://github.com", "https://github.com", false)]
	[InlineData("", "https://github.com", false)]
	[InlineData(null, "https://github.com", false)]
	[InlineData("https://github.com", "", false)]
	[InlineData("github.com", "https://evil.example", false)]
	public void IsMisleadingLinkText_DetectsDisguisedLinks(
		string? linkText, string url, bool expected)
	{
		Assert.Equal(expected, MarkdownUrlPolicy.IsMisleadingLinkText(linkText, url));
	}

	[Theory]
	[InlineData("https://example.com", true)]
	[InlineData("mailto:a@b.c", true)]
	[InlineData("javascript:alert(1)", false)]
	[InlineData("file:///C:/x", false)]
	[InlineData("", false)]
	[InlineData(null, false)]
	public void IsAllowedExternalUri_WhitelistsSchemes(string? url, bool expected)
	{
		Assert.Equal(expected, MarkdownUrlPolicy.IsAllowedExternalUri(url));
	}
}

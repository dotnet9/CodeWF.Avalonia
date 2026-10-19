using CodeWF.Avalonia.Markdown;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Security;

/// <summary>
/// 本地图片白名单：配置根目录后，白名单之外的本地图片（含 file:// 形式）
/// 拒绝加载；清空后恢复不限制。
/// </summary>
public sealed class MarkdownImageLocalRootsTests : IDisposable
{
	private readonly string _root;
	private readonly string _outside;
	private readonly byte[] _pngBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

	public MarkdownImageLocalRootsTests()
	{
		_root = Path.Combine(Path.GetTempPath(), "codewf-md-roots-tests", "project", Guid.NewGuid().ToString("N"));
		_outside = Path.Combine(Path.GetTempPath(), "codewf-md-roots-tests", "outside", Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(_root);
		Directory.CreateDirectory(_outside);
	}

	public void Dispose()
	{
		MarkdownImageSourceLoader.ClearAllowedLocalImageRoots();
		try
		{
			Directory.Delete(Path.Combine(Path.GetTempPath(), "codewf-md-roots-tests"), recursive: true);
		}
		catch (IOException)
		{
		}
	}

	[Fact]
	public void Load_InsideRoot_IsAllowed()
	{
		var imagePath = Path.Combine(_root, "logo.png");
		File.WriteAllBytes(imagePath, _pngBytes);
		MarkdownImageSourceLoader.SetAllowedLocalImageRoots([_root]);

		var source = MarkdownImageSourceLoader.Load(imagePath);

		Assert.Equal(_pngBytes, source.Bytes);
	}

	[Fact]
	public void Load_OutsideRoot_Throws()
	{
		var imagePath = Path.Combine(_outside, "secret.png");
		File.WriteAllBytes(imagePath, _pngBytes);
		MarkdownImageSourceLoader.SetAllowedLocalImageRoots([_root]);

		Assert.Throws<UnauthorizedAccessException>(() => MarkdownImageSourceLoader.Load(imagePath));
		Assert.Throws<UnauthorizedAccessException>(
			() => MarkdownImageSourceLoader.Load(new Uri(imagePath).ToString()));
	}

	[Fact]
	public void Load_RelativePath_EscapingRoot_Throws()
	{
		MarkdownImageSourceLoader.SetAllowedLocalImageRoots([_root]);

		Assert.Throws<UnauthorizedAccessException>(
			() => MarkdownImageSourceLoader.Load("../outside/secret.png", _root));
	}

	[Fact]
	public void NoWhitelist_LoadsAnywhere()
	{
		var imagePath = Path.Combine(_outside, "secret.png");
		File.WriteAllBytes(imagePath, _pngBytes);
		MarkdownImageSourceLoader.ClearAllowedLocalImageRoots();

		var source = MarkdownImageSourceLoader.Load(imagePath);

		Assert.Equal(_pngBytes, source.Bytes);
	}
}

using CodeWF.Avalonia.Markdown;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

public sealed class MarkdownImageDiskCacheTests : IDisposable
{
	private readonly string _directory;

	public MarkdownImageDiskCacheTests()
	{
		_directory = Path.Combine(Path.GetTempPath(), "codewf-md-cache-tests", Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(_directory);
	}

	public void Dispose()
	{
		MarkdownImageDiskCache.CacheDirectory = null;
		try
		{
			Directory.Delete(_directory, recursive: true);
		}
		catch (IOException)
		{
		}
	}

	[Fact]
	public void Store_ThenRead_ReturnsBytes()
	{
		MarkdownImageDiskCache.CacheDirectory = _directory;
		var bytes = new byte[] { 1, 2, 3, 137, 80, 78, 71 };

		MarkdownImageDiskCache.Store("https://example.com/a.png", bytes);

		var read = MarkdownImageDiskCache.TryRead("https://example.com/a.png");
		Assert.NotNull(read);
		Assert.Equal(bytes, read);
	}

	[Fact]
	public void TryRead_Miss_ReturnsNull()
	{
		MarkdownImageDiskCache.CacheDirectory = _directory;

		Assert.Null(MarkdownImageDiskCache.TryRead("https://example.com/missing.png"));
	}

	[Fact]
	public void Disabled_ReturnsNullAndIgnoresStore()
	{
		MarkdownImageDiskCache.CacheDirectory = null;

		MarkdownImageDiskCache.Store("https://example.com/a.png", [1, 2, 3]);

		Assert.Null(MarkdownImageDiskCache.TryRead("https://example.com/a.png"));
		Assert.Empty(Directory.GetFiles(_directory));
	}

	[Fact]
	public void DifferentKeys_MapToDifferentFiles()
	{
		MarkdownImageDiskCache.CacheDirectory = _directory;

		MarkdownImageDiskCache.Store("https://a.example/1.png", [1]);
		MarkdownImageDiskCache.Store("https://a.example/2.png", [2]);

		Assert.Equal(2, Directory.GetFiles(_directory).Length);
		Assert.Equal(new byte[] { 1 }, MarkdownImageDiskCache.TryRead("https://a.example/1.png"));
		Assert.Equal(new byte[] { 2 }, MarkdownImageDiskCache.TryRead("https://a.example/2.png"));
	}

	[Fact]
	public void CorruptCacheFile_ReadsAsMiss()
	{
		MarkdownImageDiskCache.CacheDirectory = _directory;
		MarkdownImageDiskCache.Store("https://example.com/a.png", [1, 2, 3]);

		// 模拟缓存损坏：删除文件内容改成空。
		var files = Directory.GetFiles(_directory);
		var file = Assert.Single(files);
		File.WriteAllBytes(file, []);

		Assert.Equal([], MarkdownImageDiskCache.TryRead("https://example.com/a.png"));
	}
}

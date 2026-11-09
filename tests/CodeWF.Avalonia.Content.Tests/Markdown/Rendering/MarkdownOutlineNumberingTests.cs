using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Shared.Rendering;

using Markdig;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

public sealed class MarkdownOutlineNumberingTests
{
	private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
		.UseAdvancedExtensions()
		.Build();

	[Fact]
	public void WithNumbers_CountsPerLevelAndResetsDeeperLevels()
	{
		var items = new[]
		{
			new MarkdownOutlineItem(1, "A", 1, 0),
			new MarkdownOutlineItem(2, "B", 3, 5),
			new MarkdownOutlineItem(3, "C", 5, 11),
			new MarkdownOutlineItem(2, "D", 7, 20),
			new MarkdownOutlineItem(1, "E", 9, 30)
		};

		var numbered = items.WithNumbers();

		Assert.Equal(["1", "1.1", "1.1.1", "1.2", "2"], numbered.Select(item => item.Number));
	}

	[Fact]
	public void WithNumbers_WhenLevelIsSkipped_PadsMiddleLevelWithZero()
	{
		var items = new[]
		{
			new MarkdownOutlineItem(1, "A", 1, 0),
			new MarkdownOutlineItem(3, "B", 3, 5),
			new MarkdownOutlineItem(3, "C", 5, 11)
		};

		var numbered = items.WithNumbers();

		Assert.Equal(["1", "1.0.1", "1.0.2"], numbered.Select(item => item.Number));
	}

	[Fact]
	public void WithNumbers_WhenLevelExceedsRange_ClampsToSix()
	{
		// Extract 已把层级钳到 1-6，此输入不会出现；仅记录钳制后的行为与两宿主既有算法一致。
		var items = new[] { new MarkdownOutlineItem(9, "A", 1, 0), new MarkdownOutlineItem(0, "B", 3, 5) };

		var numbered = items.WithNumbers();

		Assert.Equal(["0.0.0.0.0.1", "1"], numbered.Select(item => item.Number));
	}

	[Fact]
	public void WithNumbers_WhenDocumentDoesNotStartAtH1_PadsMissingLevelsWithZero()
	{
		// 既有算法原样行为：孤立的 H2 得 0.1（与 Vex/Vox 原实现一致），不做智能补号。
		var items = new[] { new MarkdownOutlineItem(2, "B", 3, 5) };

		var numbered = items.WithNumbers();

		Assert.Equal(["0.1"], numbered.Select(item => item.Number));
	}

	[Fact]
	public void WithNumbers_KeepsOtherFieldsUntouched()
	{
		var items = new[] { new MarkdownOutlineItem(1, "A", 1, 0), new MarkdownOutlineItem(2, "B", 3, 5) };

		var numbered = items.WithNumbers();

		Assert.Equal(2, numbered.Count);
		Assert.Equal(new MarkdownOutlineItem(1, "A", 1, 0) { Number = "1" }, numbered[0]);
		Assert.Equal(new MarkdownOutlineItem(2, "B", 3, 5) { Number = "1.1" }, numbered[1]);
	}

	[Fact]
	public void WithNumbers_WhenEmpty_ReturnsEmpty()
	{
		Assert.Empty(Array.Empty<MarkdownOutlineItem>().WithNumbers());
	}

	[Fact]
	public void ExtractFromText_PrefersAstAndFallsBackToLineScan()
	{
		// AST 优先：正常文档走管线，行号 1 起。
		var normal = MarkdownOutlineExtractor.ExtractFromText("# A\n\n## B", Pipeline);
		Assert.Equal(2, normal.Count);
		Assert.Equal("A", normal[0].Title);

		// 解析失败/空结果时退化为行扫描：跳过围栏代码块内的 #。
		var fallback = MarkdownOutlineExtractor.ExtractFromText("```md\n# not a heading\n```\n\n## Real\n", Pipeline);
		var item = Assert.Single(fallback);
		Assert.Equal(2, item.Level);
		Assert.Equal("Real", item.Title);
		Assert.Equal(5, item.Line);
	}

	[Fact]
	public void ExpansionKeys_AreStableAcrossRebuildsAndDistinctForDuplicates()
	{
		(IReadOnlyList<string> Keys, IReadOnlyList<string> Again) Build()
		{
			var items = new List<(int Level, string Title)>
			{
				(1, "同名章节"),
				(2, "子"),
				(1, "同名章节")
			};
			var keys = OutlineExpansionKey.BuildSequence(items, "doc");
			return (keys, OutlineExpansionKey.BuildSequence(items, "doc"));
		}

		var (keys, again) = Build();

		Assert.Equal(3, keys.Count);
		Assert.Equal(keys, again);
		Assert.NotEqual(keys[0], keys[2]); // 同名章节按出现序号区分
	}
}

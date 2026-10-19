using CodeWF.Avalonia.Markdown.Streaming;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Streaming;

/// <summary>
/// 尾部容错补全测试（规格：docs/流式规则.md §4）。
/// </summary>
public sealed class MarkdownTailRemenderTests
{
	[Theory]
	[InlineData("**粗体", "**粗体**")]
	[InlineData("__下划", "__下划__")]
	[InlineData("*斜体", "*斜体*")]
	[InlineData("`代码", "`代码`")]
	[InlineData("~~删除", "~~删除~~")]
	[InlineData("[文字", "[文字](codewf-md:pending)")]
	[InlineData("[链接](https://example.com", "[链接](https://example.com)")]
	public void Apply_UnbalancedSyntax_CompletesIt(string tail, string expected)
	{
		Assert.Equal(expected, MarkdownTailRemender.Apply(tail).RemendedText);
	}

	[Fact]
	public void Apply_OpenMathBlock_AppendsClosingLine()
	{
		Assert.Equal("$$\nx\n$$", MarkdownTailRemender.Apply("$$\nx").RemendedText);
	}

	[Fact]
	public void Apply_OpenFence_AppendsClosingFenceOfSameCharacter()
	{
		Assert.Equal("~~~py\nx\n~~~", MarkdownTailRemender.Apply("~~~py\nx").RemendedText);
	}

	[Fact]
	public void Apply_OpenFence_SkipsCharacterHandlers()
	{
		// 围栏内容是字面文本，`**` 不参与配对，`<di` 也不剥离。
		Assert.Equal("```py\n**x <di\n```", MarkdownTailRemender.Apply("```py\n**x <di").RemendedText);
	}

	[Fact]
	public void Apply_HalfHtmlTagAtEnd_IsStripped()
	{
		Assert.Equal("正文 ", MarkdownTailRemender.Apply("正文 <div clas").RemendedText);
	}

	[Fact]
	public void Apply_CompleteHtmlTag_IsUntouched()
	{
		var result = MarkdownTailRemender.Apply("正文 <b>粗</b>");
		Assert.Equal("正文 <b>粗</b>", result.RemendedText);
	}

	[Fact]
	public void Apply_BalancedSyntax_IsIdentity()
	{
		var tail = "**粗体** 与 `代码` 和 [链接](https://example.com)";
		var result = MarkdownTailRemender.Apply(tail);

		Assert.Equal(tail, result.RemendedText);
		Assert.True(result.OffsetMap.IsSpanClean(0, tail.Length));
		Assert.Equal(tail.Length, result.OffsetMap.OriginalLength);
	}

	[Fact]
	public void Apply_EmptyTail_IsIdentity()
	{
		var result = MarkdownTailRemender.Apply(string.Empty);

		Assert.Equal(string.Empty, result.RemendedText);
		Assert.True(result.OffsetMap.IsSpanClean(0, 0));
	}

	[Fact]
	public void OffsetMap_MapsTempSpansBackToOriginal()
	{
		// 原始尾部 15 字符，补全 `)` 后 16 字符。
		var result = MarkdownTailRemender.Apply("[链接](https://ex");
		Assert.Equal("[链接](https://ex)", result.RemendedText);

		// 插入点之前的区间逐字符一致。
		Assert.Equal(0, result.OffsetMap.MapTempToOriginal(0));
		Assert.Equal(5, result.OffsetMap.MapTempToOriginal(5));
		Assert.True(result.OffsetMap.IsSpanClean(0, 5));

		// 覆盖插入点的区间不干净，端点收拢回原始长度。
		Assert.False(result.OffsetMap.IsSpanClean(0, result.RemendedText.Length));
		Assert.Equal(15, result.OffsetMap.MapTempToOriginal(result.RemendedText.Length));
	}

	[Fact]
	public void OffsetMap_ParagraphBeforeInsertion_StaysClean()
	{
		var result = MarkdownTailRemender.Apply("段落\n[链接](https://ex");

		// 第一块（段落）不包含插入点，映射为恒等。
		Assert.True(result.OffsetMap.IsSpanClean(0, 3));
		Assert.Equal(3, result.OffsetMap.MapTempToOriginal(3));
	}

	[Fact]
	public void Apply_MultipleCompletions_AllAppendedInOrder()
	{
		// `**` 奇数 + 反引号奇数 + 未闭合括号，全部在末尾按处理器优先级补齐。
		var result = MarkdownTailRemender.Apply("**粗 `代 [链](u");

		Assert.Equal("**粗 `代 [链](u)**`", result.RemendedText);
	}
}

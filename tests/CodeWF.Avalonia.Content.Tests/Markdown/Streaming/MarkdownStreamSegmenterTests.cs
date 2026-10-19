using CodeWF.Avalonia.Markdown.Streaming;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Streaming;

/// <summary>
/// 冻结边界切分器规则测试（规格：docs/流式规则.md §3）。
/// 断言口径：给定完整源码，<see cref="MarkdownStreamSegmenter.FrozenBoundary"/>
/// 应落在指定锚点文本的行首（或文档结尾）。
/// </summary>
public sealed class MarkdownStreamSegmenterTests
{
	[Fact]
	public void BlankLine_BeforeNewParagraph_Freezes()
	{
		var source = "第一段\n\n第二段\n";
		var boundary = ScanAll(source);

		Assert.Equal(source.IndexOf("第二段", StringComparison.Ordinal), boundary);
	}

	[Fact]
	public void UnclosedFence_BlankLineInside_DoesNotFreeze()
	{
		var source = "```python\nx = 1\n\ny = 2\n";
		Assert.Equal(0, ScanAll(source));
	}

	[Fact]
	public void ClosedFence_FreezesAfterCloseLine()
	{
		var source = "```python\nx = 1\n```\n\nafter\n";
		var boundary = ScanAll(source);

		// 闭合行后的空行仍会把边界推进到下一内容行行首。
		Assert.Equal(source.IndexOf("after", StringComparison.Ordinal), boundary);
	}

	[Fact]
	public void AtxHeading_FreezesAfterHeadingLine()
	{
		var source = "# 标题\n正文\n";
		var boundary = ScanAll(source);

		Assert.Equal(source.IndexOf("正文", StringComparison.Ordinal), boundary);
	}

	[Fact]
	public void SetextHeading_FreezesAfterUnderline()
	{
		var source = "标题\n===\n正文\n";
		var boundary = ScanAll(source);

		Assert.Equal(EndOfLineContaining(source, "==="), boundary);
	}

	[Fact]
	public void SetextDashHeading_WinsOverThematicBreak_InsideParagraph()
	{
		var source = "标题\n---\n正文\n";
		var boundary = ScanAll(source);

		Assert.Equal(EndOfLineContaining(source, "---"), boundary);
	}

	[Fact]
	public void ThematicBreak_AfterBlank_FreezesAfterLine()
	{
		var source = "段落\n\n---\n\n后续\n";
		var boundary = ScanAll(source);

		// 分隔线后的空行仍会把边界推进到下一内容行行首。
		Assert.Equal(source.IndexOf("后续", StringComparison.Ordinal), boundary);
	}

	[Fact]
	public void LooseList_BlankLineBetweenItems_DoesNotFreeze()
	{
		var source = "- 甲\n\n- 乙\n";
		Assert.Equal(0, ScanAll(source));
	}

	[Fact]
	public void List_ThenUnindentedParagraph_Freezes()
	{
		var source = "- 甲\n\n段落\n";
		var boundary = ScanAll(source);

		Assert.Equal(source.IndexOf("段落", StringComparison.Ordinal), boundary);
	}

	[Fact]
	public void ListContinuation_AfterBlankWithIndent_DoesNotFreeze()
	{
		var source = "- 甲\n\n  续行\n";
		Assert.Equal(0, ScanAll(source));
	}

	[Fact]
	public void Blockquote_LazyContinuationThenQuote_DoesNotFreeze()
	{
		// `> 甲\n续\n> 乙` 是同一个引用块（lazy continuation）。
		var source = "> 甲\n续\n> 乙\n";
		Assert.Equal(0, ScanAll(source));
	}

	[Fact]
	public void Blockquote_BlankThenQuote_DoesNotFreeze()
	{
		var source = "> 甲\n\n> 乙\n";
		Assert.Equal(0, ScanAll(source));
	}

	[Fact]
	public void Blockquote_ThenUnindentedParagraph_Freezes()
	{
		var source = "> 甲\n\n段落\n";
		var boundary = ScanAll(source);

		Assert.Equal(source.IndexOf("段落", StringComparison.Ordinal), boundary);
	}

	[Fact]
	public void DecisiveHeading_InterruptsParagraph_FreezesBeforeHeading()
	{
		var source = "段落\n# 标题\n";
		var boundary = ScanAll(source);

		// 标题行完成后整行随边界冻结，段落与标题就此分离。
		Assert.Equal(EndOfLineContaining(source, "# 标题"), boundary);
	}

	[Fact]
	public void DecisiveListItem_InterruptsParagraph_FreezesBeforeItem()
	{
		var source = "段落\n- 甲\n";
		var boundary = ScanAll(source);

		Assert.Equal(source.IndexOf("- 甲", StringComparison.Ordinal), boundary);
	}

	[Fact]
	public void OrderedListItem_OtherThanOne_AfterParagraph_DoesNotFreeze()
	{
		// `2. ` 不能打断段落，属于 lazy continuation。
		var source = "段落\n2. 甲\n";
		Assert.Equal(0, ScanAll(source));
	}

	[Fact]
	public void LinkReferenceDefinition_AfterBlank_DoesNotFreeze()
	{
		var source = "段落\n\n[引用]: https://example.com\n";
		Assert.Equal(0, ScanAll(source));
	}

	[Fact]
	public void MathBlock_BlankInside_DoesNotFreeze()
	{
		var source = "$$\nx\n\ny\n$$\n\nafter\n";
		var boundary = ScanAll(source);

		// 闭合行后的空行仍会把边界推进到下一内容行行首。
		Assert.Equal(source.IndexOf("after", StringComparison.Ordinal), boundary);
	}

	[Fact]
	public void MathBlock_SingleLine_FreezesAfterLine()
	{
		var source = "$$x + y$$\n\nafter\n";
		var boundary = ScanAll(source);

		Assert.Equal(source.IndexOf("after", StringComparison.Ordinal), boundary);
	}

	[Fact]
	public void HtmlRawBlock_BlankInside_DoesNotFreeze()
	{
			var source = "<pre>\n代码\n\n代码\n</pre>\n\nafter\n";
		var boundary = ScanAll(source);

		// 闭合行后的空行仍会把边界推进到下一内容行行首。
		Assert.Equal(source.IndexOf("after", StringComparison.Ordinal), boundary);
	}

	[Fact]
	public void IncompleteLastLine_DoesNotFreeze()
	{
		var source = "段落\n\n第二段\n";
		var segmenter = new MarkdownStreamSegmenter();
		segmenter.Scan("段落\n\n第二段", sourceComplete: false);

		Assert.Equal(0, segmenter.FrozenBoundary);

		segmenter.Scan(source, sourceComplete: false);
		Assert.Equal(source.IndexOf("第二段", StringComparison.Ordinal), segmenter.FrozenBoundary);
	}

	[Fact]
	public void IncrementalChunks_BoundaryNeverRegresses()
	{
		var source = "# 标题\n\n段落甲\n\n- 列表\n\n```csharp\nvar x = 1;\n```\n\n结尾段落\n";
		var boundary = 0;
		for (var take = 1; take <= 7; take++)
		{
			var segmenter = new MarkdownStreamSegmenter();
			for (var offset = 0; offset < source.Length; offset += take)
			{
				var end = Math.Min(offset + take, source.Length);
				segmenter.Scan(source[..end], sourceComplete: end == source.Length);
				Assert.True(segmenter.FrozenBoundary >= boundary, $"分片 {take} 时边界回退");
				boundary = segmenter.FrozenBoundary;
			}

			boundary = 0;
		}
	}

	[Fact]
	public void NeverFreezesPastLastBlockStart_SingleBlockDocument()
	{
		// 单块文档永不冻结（不变量 I2：尾部至少保留一个未冻结块）。
		Assert.Equal(0, ScanAll("只有一段\n"));
		Assert.Equal(0, ScanAll("只有一段没有换行"));
	}

	[Fact]
	public void Table_ThenBlank_FreezesAfterTable()
	{
		var source = "| 甲 | 乙 |\n| --- | --- |\n| 1 | 2 |\n\n后续\n";
		var boundary = ScanAll(source);

		Assert.Equal(source.IndexOf("后续", StringComparison.Ordinal), boundary);
	}

	private static int ScanAll(string source)
	{
		var segmenter = new MarkdownStreamSegmenter();
		segmenter.Scan(source, sourceComplete: true);
		return segmenter.FrozenBoundary;
	}

	/// <summary>包含 anchor 的那一行（含换行符）之后的偏移。</summary>
	private static int EndOfLineContaining(string source, string anchor)
	{
		var start = source.IndexOf(anchor, StringComparison.Ordinal);
		Assert.True(start >= 0, $"锚点 {anchor} 不存在");
		var newline = source.IndexOf('\n', start);
		return newline < 0 ? source.Length : newline + 1;
	}
}

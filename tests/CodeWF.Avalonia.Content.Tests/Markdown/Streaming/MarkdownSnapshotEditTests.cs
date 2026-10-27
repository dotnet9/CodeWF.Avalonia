using CodeWF.Avalonia.Markdown.Streaming;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Streaming;

/// <summary>
/// 快照差分（即时编辑的状态搬运）：公共前后缀剥离、中间段行内容 LCS 对齐、
/// 行级去向判定与旧偏移 → 新偏移映射。
/// </summary>
public sealed class MarkdownSnapshotEditTests
{
	[Fact]
	public void IdenticalText_IsIdentity()
	{
		var edit = MarkdownSnapshotEdit.Compare("# 标题\n\n正文\n", "# 标题\n\n正文\n");
		Assert.True(edit.IsIdentity);
		Assert.Empty(edit.Edits);
		Assert.Equal(4, edit.CommonPrefixLines); // 切分后共 4 段（尾随空行）
		Assert.Equal(0, edit.CommonSuffixLines);
	}

	[Fact]
	public void InsertAtTop_ShiftsEveryOldLine_AndMapsOffsets()
	{
		// 计划里的代表用例：前插一行 → 旧行全部 Shifted，偏移整体后移「新增\n」的长度。
		var edit = MarkdownSnapshotEdit.Compare("A\nB\nC", "新增\nA\nB\nC");

		Assert.False(edit.IsIdentity);
		Assert.Equal(new SnapshotLineEdit(SnapshotLineFate.Inserted, 0, 0, 0, 1), Assert.Single(edit.Edits));
		Assert.Equal(3, edit.CommonSuffixLines);
		Assert.All(new[] { 0, 1, 2 }, line => Assert.Equal(SnapshotLineFate.Shifted, edit.FateOf(line)));
		Assert.Equal(1, edit.OldLineToNewLine[0]);
		Assert.Equal(3, edit.MapOffset(0)); // "A" 行首 0 → 新偏移 3
	}

	[Fact]
	public void InsertAtTop_MapsInLineOffsets()
	{
		var edit = MarkdownSnapshotEdit.Compare("A\nBB\nCCC", "新增\nA\nBB\nCCC");
		// 新文本行首：0=新增 / 3=A / 5=BB / 8=CCC
		Assert.Equal(3, edit.MapOffset(0));
		Assert.Equal(5, edit.MapOffset(2));
		Assert.Equal(6, edit.MapOffset(3)); // 行内第 1 个字符跟着平移
		Assert.Equal(8, edit.MapOffset(5));
	}

	[Fact]
	public void ReplacedLine_IsReplaced_AndItsOffsetsAreNotMapped()
	{
		var edit = MarkdownSnapshotEdit.Compare("A\nB\nC", "A\nB2\nC");
		Assert.Equal(SnapshotLineFate.Unchanged, edit.FateOf(0));
		Assert.Equal(SnapshotLineFate.Replaced, edit.FateOf(1));
		Assert.Equal(SnapshotLineFate.Unchanged, edit.FateOf(2));
		var replaced = Assert.Single(edit.Edits);
		Assert.Equal(new SnapshotLineEdit(SnapshotLineFate.Replaced, 1, 1, 1, 1), replaced);
		Assert.Null(edit.MapOffset(2)); // 被替换行的行首
		Assert.Equal(0, edit.MapOffset(0));
	}

	[Fact]
	public void DeletedLine_IsDeleted_AndLaterLinesShift()
	{
		var edit = MarkdownSnapshotEdit.Compare("A\n删除我\nC", "A\nC");
		Assert.Equal(SnapshotLineFate.Unchanged, edit.FateOf(0));
		Assert.Equal(SnapshotLineFate.Deleted, edit.FateOf(1));
		Assert.Equal(SnapshotLineFate.Shifted, edit.FateOf(2));
		Assert.Equal(new SnapshotLineEdit(SnapshotLineFate.Deleted, 1, 1, 1, 0), Assert.Single(edit.Edits));
		Assert.Equal(2, edit.MapOffset(6)); // "C" 行首 6 → 2
		Assert.Null(edit.MapOffset(2)); // 被删行行首不可映射
	}

	[Fact]
	public void InsertInMiddle_KeepsBothSidesPaired()
	{
		var edit = MarkdownSnapshotEdit.Compare(
			"# 标题\n\n正文\n\n```csharp\nvar a = 1;\n```\n",
			"# 标题\n\n前言\n\n正文\n\n```csharp\nvar a = 1;\n```\n");

		Assert.Equal(2, edit.CommonPrefixLines);
		Assert.Equal(6, edit.CommonSuffixLines);
		Assert.Equal(new SnapshotLineEdit(SnapshotLineFate.Inserted, 2, 0, 2, 2), Assert.Single(edit.Edits));
		Assert.Equal(SnapshotLineFate.Shifted, edit.FateOf(2)); // 正文 2 → 4
		Assert.Equal(4, edit.OldLineToNewLine[2]);
		Assert.Equal(SnapshotLineFate.Shifted, edit.FateOf(4)); // 围栏开 4 → 6
		Assert.Equal(SnapshotLineFate.Unchanged, edit.FateOf(0));
	}

	[Fact]
	public void EmptyTexts_AreHandled()
	{
		var same = MarkdownSnapshotEdit.Compare(null, null);
		Assert.True(same.IsIdentity);
		Assert.Equal(1, same.OldLineCount);

		var grown = MarkdownSnapshotEdit.Compare(string.Empty, "正文");
		Assert.False(grown.IsIdentity);
		Assert.Equal(new SnapshotLineEdit(SnapshotLineFate.Replaced, 0, 1, 0, 1), Assert.Single(grown.Edits));
		Assert.Null(grown.MapOffset(0));
	}

	[Fact]
	public void LargeMiddle_FallsBackToPositionalComparison()
	{
		// 超过 LCS 预算（250k 单元）退化为逐位比较：不抛异常，内容不同的行整段记为替换。
		var oldText = string.Join('\n', Enumerable.Range(0, 600).Select(i => "旧行" + i));
		var newText = string.Join('\n', Enumerable.Range(0, 600).Select(i => "新行" + i));
		var edit = MarkdownSnapshotEdit.Compare(oldText, newText);
		Assert.False(edit.IsIdentity);
		var single = Assert.Single(edit.Edits);
		Assert.Equal(SnapshotLineFate.Replaced, single.Fate);
		Assert.Equal(600, single.OldLineCount);
	}

	[Fact]
	public void FateOf_RejectsOutOfRange()
	{
		var edit = MarkdownSnapshotEdit.Compare("A", "B");
		Assert.Throws<ArgumentOutOfRangeException>(() => edit.FateOf(-1));
		Assert.Throws<ArgumentOutOfRangeException>(() => edit.FateOf(1));
	}
}

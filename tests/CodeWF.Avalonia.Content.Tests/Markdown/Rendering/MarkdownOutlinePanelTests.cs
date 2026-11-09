using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Shared.Rendering;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

/// <summary>
/// 文档大纲侧栏面板：行生成/序号、筛选（匹配 + 父链）、折叠可见性与事件、序号显隐类。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownOutlinePanelTests(AvaloniaPlatformFixture platform)
{
	private static IReadOnlyList<MarkdownOutlineItem> Items() =>
	[
		new(1, "Alpha", 1, 0),
		new(2, "Bravo", 3, 10),
		new(3, "Charlie", 5, 20),
		new(2, "Delta", 7, 30)
	];

	[Fact]
	public void SetItems_BuildsRowsWithNumbersAndHierarchy() => platform.Run(() =>
	{
		var panel = new MarkdownOutlinePanel();
		var rows = panel.SetItems(Items());

		Assert.Equal(4, rows.Count);
		Assert.Equal(["1", "1.1", "1.1.1", "1.2"], rows.Select(r => r.Number));
		Assert.True(rows[0].HasChildren);
		Assert.False(rows[2].HasChildren);
		Assert.True(rows[0].IsTopLevel);
		Assert.Equal(4, panel.VisibleRows.Count);
	});

	[Fact]
	public void ApplyFilter_ShowsMatchesWithAncestors() => platform.Run(() =>
	{
		var panel = new MarkdownOutlinePanel();
		panel.SetItems(Items());

		panel.ApplyFilter("charlie");

		Assert.Equal(["Alpha", "Bravo", "Charlie"], panel.VisibleRows.Select(r => r.Title));

		panel.ApplyFilter("delta");
		Assert.Equal(["Alpha", "Delta"], panel.VisibleRows.Select(r => r.Title));

		panel.ApplyFilter(string.Empty);
		Assert.Equal(4, panel.VisibleRows.Count);
	});

	[Fact]
	public void Collapse_HidesDescendantsAndRaisesExpandedChanged() => platform.Run(() =>
	{
		var panel = new MarkdownOutlinePanel();
		var rows = panel.SetItems(Items());
		var changed = new List<OutlineRow>();
		panel.ExpandedChanged += (_, row) => changed.Add(row);

		rows[0].IsExpanded = false;

		Assert.Equal(["1"], panel.VisibleRows.Select(r => r.Number));
		Assert.Single(changed);
		Assert.Same(rows[0], changed[0]);

		rows[0].IsExpanded = true;
		Assert.Equal(4, panel.VisibleRows.Count);
		Assert.Equal(2, changed.Count);
	});

	[Fact]
	public void ShowNumbers_TogglesPanelClass() => platform.Run(() =>
	{
		var panel = new MarkdownOutlinePanel();
		Assert.False(panel.Classes.Contains("op-numbers"));

		panel.ShowNumbers = true;

		Assert.True(panel.Classes.Contains("op-numbers"));
	});

	[Fact]
	public void CurrentRow_StampsIsCurrent() => platform.Run(() =>
	{
		var panel = new MarkdownOutlinePanel();
		var rows = panel.SetItems(Items());

		panel.CurrentRow = rows[1];

		Assert.True(rows[1].IsCurrent);
		Assert.False(rows[0].IsCurrent);
		Assert.False(rows[3].IsCurrent);
	});
}

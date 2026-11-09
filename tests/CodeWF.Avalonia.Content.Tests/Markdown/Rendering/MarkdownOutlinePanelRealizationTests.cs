using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Shared.Rendering;

using Avalonia.Controls;
using Avalonia.VisualTree;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

/// <summary>
/// 面板挂到真实窗口后的容器实例化验证：50 行条目必须生成 ListBoxItem 容器并呈现行内容，
/// 该用例复现"面板有数据但视觉为空"的回归。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownOutlinePanelRealizationTests(AvaloniaPlatformFixture platform)
{
	[Fact]
	public void SetItems_RealizesRows_InRootedWindow() => platform.Run(() =>
	{
		var window = new Window { Width = 800, Height = 600 };
		// 与宿主 App 同口径：Semi 提供基础控件模板，MarkdownFullThemes 提供 Markdown 控件主题。
		window.Styles.Add(new global::Semi.Avalonia.SemiTheme());
		window.Styles.Add(new global::CodeWF.Avalonia.Markdown.Themes.MarkdownFullThemes());
		var panel = new MarkdownOutlinePanel();
		window.Content = panel;
		window.Show();

		try
		{
			var items = Enumerable.Range(1, 50)
				.Select(i => new MarkdownOutlineItem(1, $"章节 {i}", i, 0))
				.ToArray();
			panel.SetItems(items);
			window.UpdateLayout();

			var listBox = panel.GetVisualDescendants().OfType<ListBox>().SingleOrDefault();
			Assert.NotNull(listBox);
			Assert.Equal(50, listBox.ItemCount);
			Assert.Equal(50, panel.VisibleRows.Count);

			var containers = listBox.GetVisualDescendants().OfType<ListBoxItem>().Count();
			Assert.True(containers > 0, $"ListBoxItem containers: {containers}; items={listBox.ItemCount}");

			var titles = panel.GetVisualDescendants().OfType<TextBlock>()
				.Count(text => text.Text is not null && text.Text.StartsWith("章节"));
			Assert.True(titles > 0, $"Realized row titles: {titles}");
		}
		finally
		{
			window.Close();
		}
	});
}

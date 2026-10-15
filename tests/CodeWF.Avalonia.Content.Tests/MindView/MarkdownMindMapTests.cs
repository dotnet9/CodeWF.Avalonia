using CodeWF.Avalonia.MindView.Markdown;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.MindView;

public class MarkdownMindMapTests
{
    [Fact]
    public void InsertingEarlierContentKeepsExistingNodeIdentity()
    {
        var before = MarkdownMindMapDocument.Parse("# 标题\n\n## 章节\n\n正文");
        var after = MarkdownMindMapDocument.Parse("# 标题\n\n新增段落\n\n## 章节\n\n正文");
        Assert.Equal(before.Children[0].Id, after.Children[1].Id);
        Assert.NotEqual(before.Children[0].SourceLine, after.Children[1].SourceLine);
    }

    [Fact]
    public void AstPreservesHeadingAndListHierarchyAndSourceRanges()
    {
        const string source = "# 计划\r\n\r\n正文 **重点**。\r\n\r\n### 跳级\r\n- 第一项\r\n  - 子项\r\n\r\n```md\r\n# 不是标题\r\n```\r\n\r\n## 第二节\r\n";
        var root = MarkdownMindMapDocument.Parse(source);
        var heading = root;
        Assert.Equal("计划", heading.Title);
        Assert.Equal(3, heading.Children.Count);
        Assert.Equal("正文 重点。", heading.Children[0].FullText);
        var skipped = heading.Children[1];
        Assert.Equal("跳级", skipped.Title);
        var item = Assert.Single(skipped.Children);
        Assert.Equal("第一项", item.Title);
        Assert.Equal("子项", Assert.Single(item.Children).Title);
        Assert.Equal("第二节", heading.Children[2].Title);
        Assert.StartsWith("### 跳级", source[skipped.SourceStart..skipped.SourceEnd]);
        Assert.Equal(5, skipped.SourceLine);
    }

    [Fact]
    public void SummaryCountsGraphemesWithoutChangingFullText()
    {
        const string text = "👨‍👩‍👧‍👦e\u0301中文";
        Assert.Equal("👨‍👩‍👧‍👦e\u0301…", MarkdownMindMapDocument.Summarize(text, 2));
        var root = MarkdownMindMapDocument.Parse(text, options: new() { SummaryLength = 2 });
        var node = Assert.Single(root.Children);
        Assert.Equal(text, node.FullText);
        Assert.Equal("👨‍👩‍👧‍👦e\u0301…", node.Title);
        Assert.Equal(text.Length, node.SourceEnd);
    }

    [Fact]
    public void CodeAndTableAreOptInAndParagraphsCanBeHidden()
    {
        const string source = "paragraph\n\n```cs\n# code\n```\n\n| A | B |\n|---|---|\n| 1 | 2 |\n";
        Assert.Single(MarkdownMindMapDocument.Parse(source).Children);
        var root = MarkdownMindMapDocument.Parse(source, options: new() { ShowCode = true, ShowTables = true, ShowParagraphs = false });
        Assert.Equal(new[] { MarkdownMindMapNodeKind.Code, MarkdownMindMapNodeKind.Table }, root.Children.Select(n => n.Kind));
    }

    [Fact]
    public void RootPolicyIgnoresFrontMatterAndKeepsMultipleTitles()
    {
        var root = MarkdownMindMapDocument.Parse("---\ntitle: metadata\n---\n\n# 正文\n\n内容");
        Assert.Equal("正文", root.Title);
        Assert.Equal("内容", Assert.Single(root.Children).FullText);
        var multiple = MarkdownMindMapDocument.Parse("# 一\n\n# 二", "文件");
        Assert.Equal("文件", multiple.Title);
        Assert.Equal(2, multiple.Children.Count);
        Assert.Empty(MarkdownMindMapDocument.Parse("- first\n- second", options: new() { ShowLists = false }).Children);
    }
}

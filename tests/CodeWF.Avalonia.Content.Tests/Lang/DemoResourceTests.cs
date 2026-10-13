using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using System.Xml.Linq;

namespace CodeWF.Avalonia.Content.Tests.Lang;

public sealed class DemoResourceTests
{
    [Fact]
    public void LocalizedFormatsRetainArgumentsInBothLanguages()
    {
        var chinese = Read(Path.Combine(DemoDirectory, "I18n", "CodeWF.Avalonia.Demo.zh-CN.json"));
        var english = Read(Path.Combine(DemoDirectory, "I18n", "CodeWF.Avalonia.Demo.en-US.json"));
        foreach (var file in Directory.EnumerateFiles(DemoDirectory, "*.cs", SearchOption.AllDirectories))
        {
            var root = CSharpSyntaxTree.ParseText(File.ReadAllText(file)).GetRoot();
            foreach (var call in root.DescendantNodes().OfType<InvocationExpressionSyntax>().Where(call => call.Expression.ToString() == "string.Format"))
            {
                var arguments = call.ArgumentList.Arguments;
                for (var index = 0; index < arguments.Count; index++)
                {
                    if (arguments[index].Expression is not InvocationExpressionSyntax lookup || !lookup.Expression.ToString().EndsWith("GetResource")) continue;
                    if (lookup.ArgumentList.Arguments.FirstOrDefault()?.Expression is not LiteralExpressionSyntax key || !chinese.ContainsKey(key.Token.ValueText)) continue;
                    var original = chinese[key.Token.ValueText];
                    var translated = english[key.Token.ValueText];
                    Assert.True(CompositeFormat.Parse(original).MinimumArgumentCount <= arguments.Count - index - 1, key.Token.ValueText);
                    Assert.True(CompositeFormat.Parse(translated).MinimumArgumentCount <= arguments.Count - index - 1, key.Token.ValueText);
                    static string[] Indices(string format) => Regex.Matches(format, @"(?<!\{)\{(\d+)(?:,[^}:]+)?(?::[^}]+)?\}(?!\})")
                        .Select(match => match.Groups[1].Value).Distinct().Order().ToArray();
                    Assert.Equal(Indices(original), Indices(translated));
                }
            }
        }
    }

    [Fact]
    public void DemoSourcesKeepChineseTextInLocalizationResources()
    {
        foreach (var file in Directory.EnumerateFiles(DemoDirectory, "*.cs", SearchOption.AllDirectories))
        {
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file));
            foreach (var token in tree.GetRoot().DescendantTokens().Where(token => token.IsKind(SyntaxKind.StringLiteralToken)
                         || token.IsKind(SyntaxKind.InterpolatedStringTextToken)
                         || token.IsKind(SyntaxKind.SingleLineRawStringLiteralToken)
                         || token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken)))
                Assert.False(Regex.IsMatch(token.ValueText, "[\\u4e00-\\u9fff]"), Path.GetFileName(file) + ": " + token.GetLocation().GetLineSpan());
        }
        foreach (var file in Directory.EnumerateFiles(DemoDirectory, "*.axaml", SearchOption.AllDirectories))
            foreach (var attribute in XDocument.Load(file).Descendants().Attributes())
                Assert.False(Regex.IsMatch(attribute.Value, "[\\u4e00-\\u9fff]"), Path.GetFileName(file) + ": " + attribute.Name);
    }

    private static string DemoDirectory
    {
        get
        {
            for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
                if (File.Exists(Path.Combine(directory.FullName, "CodeWF.Avalonia.slnx")))
                    return Path.Combine(directory.FullName, "demo", "CodeWF.Avalonia.Demo");
            throw new DirectoryNotFoundException("Repository root not found");
        }
    }

    [Fact]
    public void ChineseAndEnglishHaveTheSameKeysAndAllReferencedResources()
    {
        var folder = DemoDirectory;
        var chinese = Read(Path.Combine(folder, "I18n", "CodeWF.Avalonia.Demo.zh-CN.json"));
        var english = Read(Path.Combine(folder, "I18n", "CodeWF.Avalonia.Demo.en-US.json"));
        Assert.Equal(chinese.Keys.Order().ToArray(), english.Keys.Order().ToArray());
        foreach (var resource in english.Where(resource => resource.Key != "Demo.Text.MainViewModel.Content001"))
            Assert.False(Regex.IsMatch(resource.Value, "[\\u4e00-\\u9fff]"), "Untranslated English resource: " + resource.Key);
        foreach (var file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
                     .Where(file => file.EndsWith(".cs") || file.EndsWith(".axaml")))
        {
            var content = File.ReadAllText(file);
            foreach (Match match in Regex.Matches(content, "GetResource\\(\\s*\"(Demo\\.[^\"]+)\"|:I18n\\s+(Demo\\.[^,}\\s]+)|\"@(Demo\\.[^\"]+)\""))
            {
                var key = match.Groups.Cast<Group>().Skip(1).First(group => group.Success).Value;
                Assert.True(chinese.ContainsKey(key), Path.GetFileName(file) + ": missing " + key);
            }
        }
    }

    private static Dictionary<string, string> Read(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var resources = new Dictionary<string, string>();
        void Walk(JsonElement element, string prefix)
        {
            if (element.ValueKind == JsonValueKind.Object)
                foreach (var property in element.EnumerateObject())
                    Walk(property.Value, prefix.Length == 0 ? property.Name : prefix + "." + property.Name);
            else if (element.ValueKind == JsonValueKind.String && prefix.StartsWith("Demo."))
                resources.Add(prefix, element.GetString()!);
        }
        Walk(document.RootElement, "");
        return resources;
    }
}

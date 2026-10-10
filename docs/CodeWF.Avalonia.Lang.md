# CodeWF.Avalonia.Lang / CodeWF.Avalonia.Lang.Generator

插件化 Avalonia 国际化方案：核心提供 I18n 标记扩展与文化管理，内置 Json / Xml / Resx 三种资源加载器（原独立插件包已并入），源生成器从语言资源生成强类型 Key。

## 安装

```bash
dotnet add package CodeWF.Avalonia.Lang
# 可选：强类型 Key 生成
dotnet add package CodeWF.Avalonia.Lang.Generator
```

## 初始化

```csharp
I18nManager.Init(
  new JsonLangPlugin(),                     // 内置加载器：Json / XmlLangPlugin / ResxLangPlugin
  "Assets/I18n",                            // 资源目录
  new CultureInfo("zh-CN"));
I18nManager.Instance.Culture = new CultureInfo("en-US");   // 运行时切换
```

## XAML 用法

```xml
xmlns:codewf="https://codewf.com"

<TextBlock Text="{codewf:I18n {x:Static lang:LanguageKeys.Title}}" />
<TextBlock Text="{codewf:I18nBinding Hello, Name={Binding UserName}}" />
```

- `I18nExtension`：单键翻译，文化切换自动刷新。
- `I18nBinding`：带参数的模板翻译。
- `I18nConverter` / `I18nKeyConverter`：转换器形态。
- 文化回退：`CultureFallback` 支持父文化链（zh-CN → zh → 默认资源）。

## 强类型 Key 生成

`CodeWF.Avalonia.Lang.Generator`（netstandard2.0，DevelopmentDependency）在编译期扫描语言资源，生成强类型 Key 常量类，避免手写魔法字符串；诊断 ID 前缀 `CodeWFLang`。

## 进阶：从 XML 资源文件迁移

存量项目常见做法是以 XML 语言文件为中心，用 source generator 生成强类型 Key，再逐步把界面替换为 I18n 标记扩展。要点：

1. 语言文件命名遵循 `<语言代码>.xml`（或 .json/.resx），键名用点分层级（`Main.Title`）。
2. 生成器输出的常量类可直接作为 `I18n` 扩展的 Key 参数，编译期即校验拼写。
3. 设计时预览可固定 `I18nManager.Instance.Culture`，XAML 预览器即可渲染对应语言。
4. 浏览器（WASM）场景注意资源加载为异步流，`browser-support` 相关限制与桌面一致。

## 注意事项

- XAML 命名空间统一挂 `https://codewf.com`。
- 三种加载器都在核心包内；`ILangPlugin` 契约保留，可自定义任意存储格式。

# CodeWF.Avalonia.Lang

Avalonia 国际化绑定：I18n 标记扩展与转换器，直接使用 CodeWF.Toolkit.Lang 唯一管理器、资源加载器与文化回退。Avalonia localization bindings for the shared CodeWF.Toolkit.Lang engine.

[![NuGet](https://img.shields.io/nuget/v/CodeWF.Avalonia.Lang?label=nuget)](https://www.nuget.org/packages/CodeWF.Avalonia.Lang)
[![License](https://img.shields.io/badge/license-MIT-blue)](https://github.com/dotnet9/CodeWF.Avalonia/blob/main/LICENSE)

## 安装

```bash
dotnet add package CodeWF.Avalonia.Lang
```

> nuget.org 上并存现行 `12.1.6.x` 与已废弃的历史 `12.2.x` 两条版本线（后者版本号更大），浮动解析会命中旧线；请逐包精确钉版到 `12.1.6.*`。

`CodeWF.Toolkit.Lang` 经传递依赖自动安装。本包不需要主题入口；唯一的管理器、JSON/XML/RESX 加载器与语言元数据均由 Toolkit 提供，运行时不创建另一套实例。

## 初始化（应用只做一次）

```csharp
using CodeWF.Toolkit.Lang;
using CodeWF.Toolkit.Lang.Json;

I18nManager.Instance.Register(
    new JsonLangPlugin { ResourceFolder = Path.Combine(AppContext.BaseDirectory, "I18n") },
    CultureInfo.GetCultureInfo("zh-CN"));
I18nManager.Instance.Culture = CultureInfo.GetCultureInfo("en-US");
```

## XAML 绑定

```xml
xmlns:codewf="https://codewf.com"

<TextBlock Text="{codewf:I18n {x:Static lang:ResourceKeys.Title}}" />
```

`I18nExtension` / `I18nBinding` 直接订阅管理器的 `Culture` 与 `ResourceVersion`：语言切换、运行时追加资源后现有控件自动刷新；支持固定语言、动态键与格式参数，查找按 请求语言 → 父语言 → 默认语言 回退。

## 资源覆盖

- NuGet 库的 JSON 以 `EmbeddedResource` 嵌入程序集，由 `JsonLocalizer` 读取默认文案，宿主无需复制库资源。
- 同键优先级：业务输出资源 > 外层库内嵌资源 > 内层依赖库内嵌资源；缺失键逐级回退，空字符串是有效覆盖。

## 强类型资源键

推荐 T4（`Language.tt`）生成并提交代码，普通构建无需执行模板；需要编译期生成时配套使用 [CodeWF.Avalonia.Lang.Generator](https://www.nuget.org/packages/CodeWF.Avalonia.Lang.Generator)，把语言资源作为 `AdditionalFiles` 传入。

> 从 `12.1.4.19` 起，管理器/插件/语言元数据类型统一位于 `CodeWF.Toolkit.Lang`（含 `Json` / `Xml` / `Resx` 子命名空间），旧命名空间中的类型已移除；所有 CodeWF.Avalonia 运行时包应一起升级。

## 文档

- 完整说明文档（含 Lang.Generator）：<https://github.com/dotnet9/CodeWF.Avalonia/blob/main/docs/CodeWF.Avalonia.Lang%20说明文档.md>
- 仓库与全部包一览：<https://github.com/dotnet9/CodeWF.Avalonia>

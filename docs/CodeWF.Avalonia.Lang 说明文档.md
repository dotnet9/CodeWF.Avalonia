# CodeWF.Avalonia.Lang / CodeWF.Avalonia.Lang.Generator

Avalonia 国际化绑定与转换器。唯一的语言管理器、JSON / XML / RESX 加载器和语言元数据均由 `CodeWF.Toolkit.Lang` 提供；运行时不创建另一套实例或转发事件。

## 安装与初始化

```sh
dotnet add package CodeWF.Avalonia.Lang
```

Toolkit.Lang 作为传递依赖自动安装。应用只初始化一次：

```csharp
using CodeWF.Toolkit.Lang;
using CodeWF.Toolkit.Lang.Json;
using System.Globalization;

I18nManager.Instance.Register(
    new JsonLangPlugin { ResourceFolder = Path.Combine(AppContext.BaseDirectory, "I18n") },
    CultureInfo.GetCultureInfo("zh-CN"));
I18nManager.Instance.Culture = CultureInfo.GetCultureInfo("en-US");
```

应用界面绑定、普通业务类、Markdown、日志、属性面板和更新服务都读取这个实例；业务不需要桥接两套管理器。未注册宿主资源时，也可直接设置全局 `Culture` 切换库内嵌文案。

## 资源与覆盖

- NuGet 库的 JSON 使用 `EmbeddedResource` 嵌入程序集，由 `JsonLocalizer` 读取默认文案。
- Demo 和应用的 JSON 复制到输出、发布目录的 `I18n/`；保留 `language`、`description`、`cultureName` 元数据。
- 同键优先级：**业务输出资源 > 外层库内嵌资源 > 内层依赖库内嵌资源**。缺失键逐级回退，空字符串是有效覆盖。
- 显式合并库资源时，`AddResource(innerAssembly, outerAssembly)` 从内层到外层注册；后注册库覆盖前注册库，业务输出资源始终优先。追加资源与重新加载遵循相同规则。
- 库使用 `new JsonLocalizer(typeof(MyLibrary).Assembly)` 即可跟随全局语言、允许业务同键覆盖并回退到默认英文；无需宿主复制库 JSON、提供 resolver 或逐库注册。

## T4 强类型键

库与业务均推荐使用 `Language.tt` 从各自 JSON 生成稳定语义键，提交生成代码，普通构建无需执行 T4。本仓运行 `pwsh scripts/sync_localization_keys.ps1` 更新，`-Check` 校验。生成代码只含资源键，不包含翻译；修改翻译仍在 JSON 中进行。

需要编译期生成时，可选择 `CodeWF.Avalonia.Lang.Generator`，并把资源文件作为 `AdditionalFiles` 传入；不同时手写维护一套映射。

## XAML 绑定

```xml
xmlns:codewf="https://codewf.com"

<TextBlock Text="{codewf:I18n {x:Static lang:ResourceKeys.Title}}" />
```

`I18nExtension` 和 `I18nBinding` 直接绑定 Toolkit 管理器的 `Culture` 与 `ResourceVersion`，语言切换、运行时追加资源后刷新现有控件。支持固定语言、动态键及格式参数；查找按请求语言、父语言、默认语言回退。

## 12.1.4.19 迁移

本次不保留旧接口。将管理器、插件与元数据的 `using CodeWF.Avalonia.Lang` 改为 `using CodeWF.Toolkit.Lang`，资源插件命名空间同步改为 Toolkit 的 `Json` / `Xml` / `Resx`。XAML 的 `https://codewf.com`、绑定和转换器命名空间保持原用法。所有 CodeWF.Avalonia 运行时包应一起升级，避免旧程序集调用已移除的类型。

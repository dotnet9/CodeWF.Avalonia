# CodeWF.Avalonia.Lang.Generator

Avalonia 多语言方案的编译期源生成器：从语言资源（JSON / XML / RESX）生成强类型资源 Key 常量。Compile-time source generator producing strongly-typed language keys.

[![NuGet](https://img.shields.io/nuget/v/CodeWF.Avalonia.Lang.Generator?label=nuget)](https://www.nuget.org/packages/CodeWF.Avalonia.Lang.Generator)
[![License](https://img.shields.io/badge/license-MIT-blue)](https://github.com/dotnet9/CodeWF.Avalonia/blob/main/LICENSE)

## 安装

生成器是编译期工具（`DevelopmentDependency`、netstandard2.0），引用时收进 `PrivateAssets`：

```xml
<PackageReference Include="CodeWF.Avalonia.Lang.Generator" Version="12.1.6.5" PrivateAssets="all" />
```

> nuget.org 上并存现行 `12.1.6.x` 与已废弃的历史 `12.2.x` 两条版本线（后者版本号更大），浮动解析会命中旧线；请逐包精确钉版到 `12.1.6.*`。

生成器在编译器内运行，消费工程发布 NativeAOT/裁剪不受影响。运行时配合 [CodeWF.Avalonia.Lang](https://www.nuget.org/packages/CodeWF.Avalonia.Lang) 与 `CodeWF.Toolkit.Lang` 的 `I18nManager` 使用。

## 用法

把语言资源文件加入 `AdditionalFiles`，生成器按扩展名自动识别 JSON / XML / RESX：

```xml
<ItemGroup>
  <AdditionalFiles Include="I18n\zh-CN.json" />
  <AdditionalFiles Include="I18n\en-US.json" />
</ItemGroup>
```

资源键需至少三段点分结构（如 `Log.View.Copy`），生成器按键的层级输出强类型常量类，键名拼写错误在编译期即被发现；多语言文件按键取并集，翻译仍只在资源文件中维护。

T4（`Language.tt`）是等价的编译后方案：生成代码提交入库、普通构建无需执行模板。两种方式二选一，不要同时手写维护一套映射。

## 文档

- 完整说明文档：<https://github.com/dotnet9/CodeWF.Avalonia/blob/main/docs/CodeWF.Avalonia.Lang%20说明文档.md>
- 仓库与全部包一览：<https://github.com/dotnet9/CodeWF.Avalonia>

// 从《配色与排版令牌规则.md》附录 A（9 套 × 8 色）生成 9×2 静态令牌 axaml、_index、
// StudioThemeVariants.g.cs（静态变体）与 StudioPalettes.g.cs（数据）。
// 派生公式与 Vex 原型 palette-rules.js 同源（规则 §3）。用法：node scripts/generate/generate-studio-cs.cjs
const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '../..');

const doc = fs.readFileSync(path.join(root, 'docs/配色与排版令牌规则.md'), 'utf8');
const appendix = doc.slice(doc.indexOf('## 附录 A'));
const data = JSON.parse(appendix.match(/```json\s*([\s\S]*?)```/)[1]);
const template = fs.readFileSync(path.join(__dirname, 'tokens-template.axaml'), 'utf8');

// —— §3 派生公式（与 palette-rules.js 同源） ——
const px = h => h.replace('#', '').match(/../g).map(x => parseInt(x, 16));
const toHex = v => '#' + v.map(x => Math.round(Math.min(255, Math.max(0, x))).toString(16).padStart(2, '0')).join('');
const mix = (a, b, w) => toHex(px(a).map((v, i) => v * (1 - w) + px(b)[i] * w));
const lum = h => px(h).map(v => { v /= 255; return v <= 0.04045 ? v / 12.92 : Math.pow((v + 0.055) / 1.055, 2.4); })
  .reduce((s, v, i) => s + v * [0.2126, 0.7152, 0.0722][i], 0);
const contrast = (a, b) => (Math.max(lum(a), lum(b)) + 0.05) / (Math.min(lum(a), lum(b)) + 0.05);
const over = (base, fg, alpha) => mix(base, fg, alpha);

const STATUS = {
  light: { danger: '#DC2626', success: '#15803D', warning: '#B45309', info: '#4338CA' },
  dark: { danger: '#F87171', success: '#4ADE80', warning: '#FBBF24', info: '#A5B4FC' },
};

function derive(base, dark) {
  const paper = base.paper, surface = base.surface, ink = base.ink, accent = base.accent;
  const onPrimary = contrast('#FFFFFF', accent) > contrast('#000000', accent) ? '#FFFFFF' : '#000000';
  // 状态软底（规则 §3 组件可读性约束）：浅色态向 paper 回退、深色态向状态色加深，保证状态文字 ≥4.5。
  const softFor = (hex, alpha) => {
    let a = alpha, bg = over(paper, hex, a);
    while (contrast(hex, bg) < 4.5 && a > (dark ? 0.40 : 0.03)) { a += dark ? 0.01 : -0.01; bg = over(paper, hex, a); }
    return bg;
  };
  const soft = hex => softFor(hex, dark ? 0.18 : 0.12);
  const softPressed = hex => softFor(hex, (dark ? 0.18 : 0.12) + 0.04);
  const st = STATUS[dark ? 'dark' : 'light'];
  return {
    notificationMuted: over(paper, ink, 0.85),
    StudioBackgroundColor: mix(paper, surface, 0.30),
    StudioSurfaceColor: surface,
    StudioPaperColor: paper,
    StudioRaisedColor: over(paper, ink, dark ? 0.06 : 0.04),
    StudioBorderColor: over(paper, ink, dark ? 0.16 : 0.12),
    StudioControlBorderColor: over(paper, ink, dark ? 0.26 : 0.20),
    StudioHoverColor: over(surface, ink, dark ? 0.07 : 0.05),
    StudioPressedColor: over(paper, accent, dark ? 0.18 : 0.12),
    StudioSelectedColor: over(paper, accent, dark ? 0.18 : 0.12),
    StudioFocusRingColor: accent,
    StudioDisabledColor: over(paper, ink, 0.40),
    StudioTextColor: ink,
    StatusBadgeLeftTextForeground: dark ? '#000000' : '#FFFFFF',
    // muted（规则 §3 派生表约束）：ink@64% 起步，对 bg/surface/raised 任一工作面不足 4.5 时按 1% 步进加深。
    StudioMutedColor: (() => {
      const bg = mix(paper, surface, 0.30), raised = over(paper, ink, dark ? 0.06 : 0.04);
      const worst = m => Math.min(contrast(m, bg), contrast(m, surface), contrast(m, raised));
      let muted = over(paper, ink, 0.64);
      for (let a = 0.65; a <= 0.90 && worst(muted) < 4.5; a += 0.01) muted = over(paper, ink, a);
      return muted;
    })(),
    StudioAccentColor: accent,
    StudioSecondaryColor: accent,
    StudioPrimaryColor: accent,
    StudioGradientStartColor: accent,
    StudioTextGradientEndColor: dark ? mix(accent, paper, 0.20) : mix(accent, '#FFFFFF', 0.25),
    StudioOnPrimary: onPrimary,
    // 行动渐变端点（规则 §3 行动渐变端点约束）：规则值 mix(accent→paper,20%)，与 OnPrimary 对比不足 4.5 时向 accent 收敛。
    StudioGradientEndColor: (() => {
      let w = 0.20, end = mix(accent, paper, w);
      while (contrast(onPrimary, end) < 4.5 && w > 0.005) { w -= 0.005; end = mix(accent, paper, w); }
      return end;
    })(),
    StudioPrimaryHover: (() => {
      const target = dark ? '#FFFFFF' : paper;
      let w = 0.08, hover = mix(accent, target, w);
      while (contrast(onPrimary, hover) < 4.5 && w > 0.005) { w -= 0.005; hover = mix(accent, target, w); }
      return hover;
    })(),
    StudioPrimaryPressed: dark ? mix(accent, '#000000', 0.12) : mix(accent, ink, 0.10),
    SemiColorSuccess: st.success,
    SemiColorWarning: st.warning,
    SemiColorDanger: st.danger,
    SemiColorDangerActive: softPressed(st.danger),
    SemiColorSuccessLight: soft(st.success),
    SemiColorWarningLight: soft(st.warning),
    SemiColorDangerLight: soft(st.danger),
    SemiColorDangerLightActive: softPressed(st.danger),
  };
}

// —— 模板值替换：仅替换「变动键」字面量；别名/结构原样保留（机制已被 CI 与 Demo 验证） ——
function renderFor(base, dark) {
  const derived = derive(base, dark);
  const derivedSourceKeys = new Set(Object.keys(data.derivedComponentKeys || {}));
  let out = template;
  // 组件覆盖键（附录 A componentOverrides/derivedComponentKeys）：别名 → 字面量画刷
  for (const [targetKey, derivedKey] of Object.entries(data.componentOverrides || {})) {
    const value = derived[derivedKey];
    if (!value) throw new Error('派生键缺失: ' + derivedKey);
    const aliasRe = new RegExp('<StaticResource x:Key="' + targetKey + '" ResourceKey="[^"]+"\\s*/?>');
    if (aliasRe.test(out)) { out = out.replace(aliasRe, '<SolidColorBrush x:Key="' + targetKey + '">' + value + '</SolidColorBrush>'); continue; }
    const literalRe = new RegExp('(<SolidColorBrush x:Key="' + targetKey + '">)[^<]*(</SolidColorBrush>)');
    if (literalRe.test(out)) { out = out.replace(literalRe, '$1' + value + '$2'); continue; }
    throw new Error('组件覆盖键在模板中未找到: ' + targetKey);
  }
  for (const [key, value] of Object.entries(derived)) {
    if (derivedSourceKeys.has(key)) continue;
    const colorRe = new RegExp('(<Color x:Key="' + key + '">)[^<]+(</Color>)');
    if (colorRe.test(out)) { out = out.replace(colorRe, '$1' + value + '$2'); continue; }
    const brushRe = new RegExp('(<SolidColorBrush x:Key="' + key + '" Color=")[^<]+(")');
    if (brushRe.test(out)) { out = out.replace(brushRe, '$1' + value + '$2'); continue; }
    const innerRe = new RegExp('(<SolidColorBrush x:Key="' + key + '">)[\\s\\S]*?(</SolidColorBrush>)');
    if (innerRe.test(out)) { out = out.replace(innerRe, '$1' + value + '$2'); continue; }
    throw new Error('模板缺少变动键: ' + key);
  }
  return out;
}

const outTokens = path.join(root, 'src/Controls/CodeWF.Avalonia.Controls/Themes/Studio/Tokens');
fs.mkdirSync(outTokens, { recursive: true });
const variantNames = { graphite: 'Graphite', indigo: 'Indigo', teal: 'Teal', pine: 'Pine', sky: 'Sky', amber: 'Amber', crimson: 'Crimson', violet: 'Violet', terracotta: 'Terracotta' };
const includes = [], variantProps = [], swatches = [];
for (const [key, p] of Object.entries(data.palettes)) {
  const name = variantNames[key];
  if (!name) throw new Error('未知配色 Key: ' + key);
  swatches.push('<SolidColorBrush x:Key="StudioSwatch' + name + '">' + p.light.accent + '</SolidColorBrush>');
  for (const dark of [false, true]) {
    const suffix = dark ? 'Dark' : 'Light';
    fs.writeFileSync(path.join(outTokens, name + suffix + '.axaml'), renderFor(dark ? p.dark : p.light, dark));
    includes.push('<ResourceInclude x:Key="{x:Static themes:StudioTheme.' + name + suffix + '}" Source="' + name + suffix + '.axaml"/>');
    variantProps.push('    public static ThemeVariant ' + name + suffix + ' { get; } = new(nameof(' + name + suffix + '), ThemeVariant.' + suffix + ');');
  }
}

// —— _index.axaml ——
const index = '<ResourceDictionary xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:themes="using:CodeWF.Avalonia.Controls.Themes">\n'
  + '<ResourceDictionary.ThemeDictionaries>\n'
  + '<ResourceInclude x:Key="Default" Source="IndigoLight.axaml"/>\n'
  + '<ResourceInclude x:Key="Light" Source="IndigoLight.axaml"/>\n'
  + '<ResourceInclude x:Key="Dark" Source="IndigoDark.axaml"/>\n'
  + includes.join('\n') + '\n'
  + '</ResourceDictionary.ThemeDictionaries>\n'
  + swatches.join('\n') + '\n'
  + '</ResourceDictionary>\n';
fs.writeFileSync(path.join(outTokens, '_index.axaml'), index);

// —— StudioThemeVariants.g.cs ——
const g = [];
g.push('// <auto-generated>由 scripts/generate/generate-studio-cs.cjs 生成（ruleVersion ' + data.ruleVersion + '）。勿手改。</auto-generated>');
g.push('using Avalonia.Styling;');
g.push('namespace CodeWF.Avalonia.Controls.Themes;');
g.push('public sealed partial class StudioTheme : Styles');
g.push('{');
g.push(variantProps.join('\n'));
g.push('');
g.push('    /// <summary>内置配色键 → 静态变体；用户配色（未注册键）运行时创建。</summary>');
g.push('    private static readonly System.Collections.Generic.Dictionary<(string Key, bool Dark), ThemeVariant> BuiltinVariants = new()');
g.push('    {');
for (const key of Object.keys(data.palettes)) {
  g.push('        { ("' + key + '", false), StudioTheme.' + variantNames[key] + 'Light },');
  g.push('        { ("' + key + '", true), StudioTheme.' + variantNames[key] + 'Dark },');
}
g.push('    };');
g.push('');

g.push('}');
fs.writeFileSync(path.join(root, 'src/Controls/CodeWF.Avalonia.Controls/Themes/StudioThemeVariants.g.cs'), g.join('\n') + '\n');

// —— StudioPalettes.g.cs ——
const pal = [];
for (const [key, p] of Object.entries(data.palettes)) {
  pal.push('    new StudioPaletteDefinition("' + key + '", "' + p.label + '", "' + p.en + '", "' + p.note + '", ' + (p.recommended ? 'true' : 'false') + ',\n      new StudioBaseColors("' + p.light.paper + '", "' + p.light.surface + '", "' + p.light.ink + '", "' + p.light.accent + '"),\n      new StudioBaseColors("' + p.dark.paper + '", "' + p.dark.surface + '", "' + p.dark.ink + '", "' + p.dark.accent + '")),');
}
const P = [];
P.push('// <auto-generated>由 scripts/generate/generate-studio-cs.cjs 生成（ruleVersion ' + data.ruleVersion + '）。勿手改。</auto-generated>');
P.push('namespace CodeWF.Avalonia.Controls.Themes;');
P.push('/// <summary>内置九套高对比配色（编写层 4 色 × 双明暗）。</summary>');
P.push('public sealed record StudioPaletteDefinition(string Key, string Name, string EnglishName, string Note, bool Recommended, StudioBaseColors Light, StudioBaseColors Dark);');
P.push('public sealed record StudioBaseColors(string Paper, string Surface, string Ink, string Accent);');
P.push('public static partial class StudioPalettes');
P.push('{');
P.push('  public static string DefaultKey { get; } = ' + JSON.stringify(data.default) + ';');
P.push('  public static System.Collections.Generic.IReadOnlyList<StudioPaletteDefinition> All { get; } = new StudioPaletteDefinition[]');
P.push('  {');
P.push(pal.join('\n'));
P.push('  };');
P.push('}');
fs.writeFileSync(path.join(root, 'src/Controls/CodeWF.Avalonia.Controls/Themes/Studio/StudioPalettes.g.cs'), P.join('\n') + '\n');
console.log('已生成 9×2 axaml + _index + StudioThemeVariants.g.cs + StudioPalettes.g.cs（ruleVersion ' + data.ruleVersion + '）');

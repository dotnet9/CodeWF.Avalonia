using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>
/// PropertyGrid 演示：构建四级属性树（分组/可展开子树/动态摘要/动态可见性），记录值提交日志。
/// </summary>
public class PropertyPanelDemoViewModel : ReactiveObject
{
    private PropertyItem? _retryStrategy;
    private PropertyItem? _backoff;
    private PropertyItem? _backoffFactor;

    public ObservableCollection<PropertyItem> Items { get; } = new();

    public ObservableCollection<string> ChangeLog { get; } = new();

    public ObservableCollection<string> BrowseLog { get; } = new();

    public ICommand ValueCommittedCommand { get; }

    public ICommand BrowseRequestedCommand { get; }

    public PropertyPanelDemoViewModel()
    {
        ValueCommittedCommand = ReactiveCommand.Create<PropertyValueChange>(change =>
        {
            ChangeLog.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {change.Item.Name}：{Describe(change.OldValue)} → {Describe(change.NewValue)}");
        });

        BrowseRequestedCommand = ReactiveCommand.Create<PropertyItem>(item =>
        {
            BrowseLog.Insert(0, $"[{DateTime.Now:HH:mm:ss}] 浏览：{item.Name}");
        });

        Build();
    }

    private static string Describe(object? value) => value switch
    {
        null => "（空）",
        bool b => b ? "true" : "false",
        _ => value.ToString() ?? string.Empty,
    };

    private void Build()
    {
        var name = new PropertyItem { Id = "p01", Name = "名称", Kind = PropertyItemKind.Text, Value = "阀门-01", DefaultValue = "", Description = "节点的显示名称，同时显示在画布与搜索结果中。" };
        var visible = new PropertyItem { Id = "p02", Name = "可见", Kind = PropertyItemKind.Boolean, Value = true, DefaultValue = true, Description = "关闭后节点在画布中隐藏，但仍参与计算与导出。" };
        var fill = new PropertyItem { Id = "p03", Name = "填充颜色", Kind = PropertyItemKind.Color, Value = "#3B82F6", DefaultValue = "#3B82F6", Description = "节点主体的填充颜色，支持 #RRGGBB 格式。" };
        var opacity = new PropertyItem { Id = "p04", Name = "不透明度", Kind = PropertyItemKind.Percent, Value = 72d, DefaultValue = 100d, Description = "节点整体不透明度，拖动滑块实时预览，松开后提交。" };

        var fontFamily = new PropertyItem { Id = "p05a", Name = "字体族", Kind = PropertyItemKind.Choice, Options = ["微软雅黑", "宋体", "Consolas", "Arial"], Value = "微软雅黑", DefaultValue = "微软雅黑", Description = "渲染文本使用的字体族。" };
        var fontSize = new PropertyItem { Id = "p05b", Name = "字号", Kind = PropertyItemKind.Number, Value = 12m, DefaultValue = 12m, Minimum = 8, Maximum = 72, Increment = 1, Unit = "px", Description = "文本的字号大小。" };
        var fontWeight = new PropertyItem { Id = "p05c", Name = "粗细", Kind = PropertyItemKind.Choice, Options = ["细体", "常规", "粗体"], Value = "常规", DefaultValue = "常规", Description = "字体的粗细样式。" };
        var italic = new PropertyItem { Id = "p05d", Name = "斜体", Kind = PropertyItemKind.Boolean, Value = false, DefaultValue = false, Description = "是否以斜体渲染文本。" };
        var font = new PropertyItem
        {
            Id = "p05",
            Name = "字体",
            Description = "文本的全局字体设置。",
            Children = [fontFamily, fontSize, fontWeight, italic],
        };
        font.SummaryProvider = () => $"{fontFamily.Value}, {fontSize.Value}px, {fontWeight.Value}{(Equals(italic.Value, true) ? " 斜体" : string.Empty)}";

        var appearance = new PropertyItem { Id = "g1", Name = "外观", Kind = PropertyItemKind.Category, Description = "节点的外观属性。", Children = [name, visible, fill, opacity, font] };

        var posX = new PropertyItem { Id = "p06a", Name = "X", Kind = PropertyItemKind.Number, Value = 120m, DefaultValue = 0m, Minimum = -9999, Maximum = 9999, Unit = "px", Description = "节点在画布中的横坐标。" };
        var posY = new PropertyItem { Id = "p06b", Name = "Y", Kind = PropertyItemKind.Number, Value = 86m, DefaultValue = 0m, Minimum = -9999, Maximum = 9999, Unit = "px", Description = "节点在画布中的纵坐标。" };
        var position = new PropertyItem { Id = "p06", Name = "位置", Description = "节点在画布中的位置。", Children = [posX, posY] };
        position.SummaryProvider = () => $"{posX.Value}, {posY.Value}";

        var width = new PropertyItem { Id = "p07a", Name = "宽", Kind = PropertyItemKind.Number, Value = 160m, DefaultValue = 160m, Minimum = 40, Maximum = 960, Increment = 2, Unit = "px", Description = "节点宽度。" };
        var height = new PropertyItem { Id = "p07b", Name = "高", Kind = PropertyItemKind.Number, Value = 48m, DefaultValue = 48m, Minimum = 24, Maximum = 720, Increment = 2, Unit = "px", Description = "节点高度。" };
        var keepRatio = new PropertyItem { Id = "p07c", Name = "保持比例", Kind = PropertyItemKind.Boolean, Value = false, DefaultValue = false, Description = "调整时锁定宽高比。" };
        var size = new PropertyItem { Id = "p07", Name = "尺寸", Description = "节点尺寸与约束。", Children = [width, height, keepRatio] };
        size.SummaryProvider = () => $"{width.Value} × {height.Value}";

        var rotate = new PropertyItem { Id = "p08", Name = "旋转", Kind = PropertyItemKind.Number, Value = 0m, DefaultValue = 0m, Minimum = -360, Maximum = 360, Increment = 5, Unit = "°", Description = "绕旋转中心的旋转角度，逆时针为正。" };
        var centerX = new PropertyItem { Id = "p09a", Name = "中心 X", Kind = PropertyItemKind.Percent, Value = 50d, DefaultValue = 50d, Description = "旋转中心的横坐标（占宽度百分比）。" };
        var centerY = new PropertyItem { Id = "p09b", Name = "中心 Y", Kind = PropertyItemKind.Percent, Value = 50d, DefaultValue = 50d, Description = "旋转中心的纵坐标（占高度百分比）。" };
        var rotationCenter = new PropertyItem { Id = "p09", Name = "旋转中心", Description = "旋转锚点，取值为中心的百分比坐标。", Children = [centerX, centerY] };
        rotationCenter.SummaryProvider = () => $"{centerX.Value}%, {centerY.Value}%";

        var layout = new PropertyItem { Id = "g2", Name = "布局", Kind = PropertyItemKind.Category, Children = [position, size, rotate, rotationCenter] };

        var joinMode = new PropertyItem { Id = "p10", Name = "参与计算", Kind = PropertyItemKind.Choice, Options = ["参与计算", "仅预览", "不参与"], Value = "仅预览", DefaultValue = "参与计算", Description = "该节点是否参与仿真计算与结果导出。" };
        var trigger = new PropertyItem { Id = "p11", Name = "触发方式", Kind = PropertyItemKind.Radio, Options = ["手动", "自动", "定时"], Value = "自动", DefaultValue = "手动", Description = "节点开始执行的触发方式。" };
        var priority = new PropertyItem { Id = "p12", Name = "优先级", Kind = PropertyItemKind.Choice, Options = ["高", "中", "低"], Value = "中", DefaultValue = "中", Description = "同一调度周期内的执行先后顺序。" };

        var retryCount = new PropertyItem { Id = "p13a", Name = "最大重试次数", Kind = PropertyItemKind.Number, Value = 3m, DefaultValue = 3m, Minimum = 0, Maximum = 99, Unit = "次", Description = "失败后最大的自动重试次数。" };
        var retryInterval = new PropertyItem { Id = "p13b", Name = "重试间隔", Kind = PropertyItemKind.Number, Value = 500m, DefaultValue = 500m, Minimum = 0, Maximum = 60000, Increment = 100, Unit = "ms", Description = "两次重试之间的等待时间。" };
        _backoff = new PropertyItem { Id = "p13c", Name = "指数退避", Kind = PropertyItemKind.Boolean, Value = true, DefaultValue = false, Description = "重试间隔按系数逐次放大，避免雪崩。" };
        _backoffFactor = new PropertyItem
        {
            Id = "p13ca",
            Name = "退避系数",
            Kind = PropertyItemKind.Number,
            Value = 1.6m,
            DefaultValue = 1.5m,
            Minimum = 1,
            Maximum = 5,
            Increment = 0.1,
            Format = "0.0",
            Description = "每次重试间隔的放大倍数（仅指数退避时可用）。",
        };
        _backoffFactor.VisibleWhen = () => Equals(_backoff?.Value, true);
        _retryStrategy = new PropertyItem { Id = "p13", Name = "重试策略", Description = "执行失败后的自动重试策略。", Children = [retryCount, retryInterval, _backoff, _backoffFactor] };
        _retryStrategy.SummaryProvider = () => $"{retryCount.Value} 次 / {retryInterval.Value}ms";

        var note = new PropertyItem { Id = "p14", Name = "便签", Kind = PropertyItemKind.MultilineText, Value = "定期检查入口压力是否满足工况要求。", DefaultValue = "", Description = "仅用于标注说明，不参与计算，随模型一并保存。" };

        var behavior = new PropertyItem { Id = "g3", Name = "行为", Kind = PropertyItemKind.Category, Children = [joinMode, trigger, priority, _retryStrategy, note] };

        var fullPath = new PropertyItem { Id = "p15", Name = "完整路径", Kind = PropertyItemKind.ReadOnly, Value = "D:\\models\\plant-v2\\pump_station.sfc", Description = "模型文件的绝对路径（只读）。" };
        var encoding = new PropertyItem { Id = "p16", Name = "编码", Kind = PropertyItemKind.Choice, Options = ["Unicode (UTF-8)", "Unicode (UTF-16 LE)", "GB2312", "ASCII"], Value = "Unicode (UTF-8)", DefaultValue = "Unicode (UTF-8)", Description = "文档的字符编码。" };
        var outputDir = new PropertyItem { Id = "p17", Name = "输出目录", Kind = PropertyItemKind.Path, Value = "D:\\exports\\plant-v2", DefaultValue = "D:\\exports\\plant-v2", Description = "仿真结果的输出目录。" };
        var stylesheet = new PropertyItem { Id = "p18", Name = "样式表", Kind = PropertyItemKind.Button, Watermark = "浏览样式表文件…", Description = "节点在报告中使用的样式表文件，点击选择文件。" };

        PropertyItem MakePort(string id, string name, string portId, string portType, decimal diameter, string summaryType)
        {
            var portNumber = new PropertyItem { Id = id + "a", Name = "编号", Kind = PropertyItemKind.Text, Value = portId, DefaultValue = portId, Description = "端口的唯一编号。" };
            var type = new PropertyItem { Id = id + "b", Name = "类型", Kind = PropertyItemKind.Choice, Options = ["进水", "出水", "排污"], Value = portType, DefaultValue = portType, Description = "端口的介质方向。" };
            var dia = new PropertyItem { Id = id + "c", Name = "直径", Kind = PropertyItemKind.Number, Value = diameter, DefaultValue = diameter, Minimum = 50, Maximum = 2000, Increment = 10, Unit = "mm", Description = "端口公称直径。" };
            var port = new PropertyItem { Id = id, Name = name, Description = $"{name} 的配置。", Children = [portNumber, type, dia] };
            port.SummaryProvider = () => $"DN{dia.Value} · {type.Value}";
            return port;
        }

        var ports = new PropertyItem
        {
            Id = "p19",
            Name = "端口列表",
            Kind = PropertyItemKind.ReadOnly,
            Description = "节点的端口集合，展开查看各端口配置。",
            Children = [MakePort("p19a", "进口 #1", "IN-01", "进水", 200m, "进水"), MakePort("p19b", "出口 #1", "OUT-01", "出水", 150m, "出水"), MakePort("p19c", "排污口", "DW-01", "排污", 80m, "排污")],
        };
        ports.SummaryProvider = () => $"{ports.Children?.Count ?? 0} 项";
        ports.Summary = ports.SummaryProvider();

        var file = new PropertyItem { Id = "g4", Name = "文件", Kind = PropertyItemKind.Category, Children = [fullPath, encoding, outputDir, stylesheet, ports] };

        foreach (var item in new[] { appearance, layout, behavior, file })
        {
            Items.Add(item);
        }

        // 初始化摘要
        foreach (var root in Items)
        {
            foreach (var node in root.SelfAndDescendants())
            {
                node.RefreshSummary();
            }

            root.RecalculateModifiedDeep();
        }
    }
}

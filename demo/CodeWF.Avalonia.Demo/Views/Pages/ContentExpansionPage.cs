using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.Controls.Models;
using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.Lang.MarkupExtensions;
using Ui = CodeWF.Avalonia.Controls.Presentation.StudioUi;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public sealed class ContentExpansionPage : DemoPageBase
{
    public ContentExpansionViewModel ViewModel { get; } = new();

    public ContentExpansionPage()
    {
        DataContext = ViewModel;
        var toggle = new Button { Classes = { "demo-action" }, Command = ViewModel.ToggleCommand };
        toggle.Bind(ContentControl.ContentProperty, new I18nExtension("Demo.Catalog.ExpansionToggle").CreateBinding());
        var animate = new CheckBox();
        animate.Bind(ContentControl.ContentProperty, new I18nExtension("Demo.Catalog.ExpansionAnimate").CreateBinding());
        animate.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(ViewModel.Animate)) { Mode = BindingMode.TwoWay });
        var duration = new Slider { Minimum = 0, Maximum = 1000, Width = 200 };
        duration.Bind(Slider.ValueProperty, new Binding(nameof(ViewModel.Milliseconds)) { Mode = BindingMode.TwoWay });
        var size = new Slider { Minimum = 100, Maximum = 300, Width = 200 };
        size.Bind(Slider.ValueProperty, new Binding(nameof(ViewModel.ExpandedSize)) { Mode = BindingMode.TwoWay });
        Content = Ui.Page(
            Ui.Card("@Demo.Catalog.Input", Ui.Hint("@Demo.Catalog.ExpansionHint"), Ui.RowLoose(16, toggle, animate),
                Ui.Field("Duration (ms)", duration, 130), Ui.Field("ExpandedSize", size, 130)),
            Ui.Card("@Demo.Catalog.ExpansionHorizontal", new Border { Height = 140, HorizontalAlignment = HorizontalAlignment.Left, Child = CreateAnimator(ExpansionOrientation.Horizontal) }),
            Ui.Card("@Demo.Catalog.ExpansionVertical", new Border { Height = 300, Child = CreateAnimator(ExpansionOrientation.Vertical) }));
    }

    private ContentExpansionAnimator CreateAnimator(ExpansionOrientation orientation)
    {
        var animator = new ContentExpansionAnimator
        {
            Orientation = orientation,
            VerticalAlignment = VerticalAlignment.Top,
            Child = Ui.Card(null, Ui.Sub("@Demo.Catalog.ExpansionContent")),
        };
        if (orientation == ExpansionOrientation.Horizontal) animator.Height = 140;
        animator.Bind(ContentExpansionAnimator.IsExpandedProperty, new Binding(nameof(ViewModel.IsExpanded)));
        animator.Bind(ContentExpansionAnimator.AnimateProperty, new Binding(nameof(ViewModel.Animate)));
        animator.Bind(ContentExpansionAnimator.DurationProperty, new Binding(nameof(ViewModel.Duration)));
        animator.Bind(ContentExpansionAnimator.ExpandedSizeProperty, new Binding(nameof(ViewModel.ExpandedSize)));
        return animator;
    }

    public override string TitleKey => "Demo.Item.ContentExpansionAnimator";
    public override string DescriptionKey => "Demo.Api.ContentExpansionAnimator.Desc";
    public override string XamlCode => "<codewf:ContentExpansionAnimator IsExpanded=\"{Binding IsExpanded}\" Orientation=\"Horizontal\"\n  Duration=\"{Binding Duration}\" Animate=\"{Binding Animate}\" ExpandedSize=\"{Binding ExpandedSize}\">\n  <ContentControl Content=\"{Binding PanelContent}\" />\n</codewf:ContentExpansionAnimator>";
    public override string CSharpCode => "animator.IsExpanded = !animator.IsExpanded;\n// Reversing during an active transition continues from the current progress.";
}

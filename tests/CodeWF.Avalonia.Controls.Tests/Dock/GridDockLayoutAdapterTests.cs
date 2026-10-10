using CodeWF.Avalonia.Dock.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using ReactiveUI.Builder;
using System;
using System.Collections.Generic;
using Xunit;

namespace CodeWF.Avalonia.Controls.Tests.Dock;

public sealed class GridDockLayoutAdapterTests
{
    static GridDockLayoutAdapterTests()
    {
        RxAppBuilder.CreateReactiveUIBuilder()
            .WithCoreServices()
            .BuildApp();
    }

    [Fact]
    public void RegionDefinitionRejectsNegativeContentColumn()
    {
        Assert.Throws<ArgumentException>(() =>
            new GridDockRegionDefinition([-1], [1]));
    }

    [Fact]
    public void RegionDefinitionAllowsContentAndCollapseColumnsToOverlap()
    {
        var definition = new GridDockRegionDefinition([0], [0, 1]);

        Assert.Equal(new[] { 0 }, definition.ContentColumns);
        Assert.Equal(new[] { 0, 1 }, definition.CollapseColumns);
    }

    [Fact]
    public void AdapterRejectsOverlappingCollapseColumns()
    {
        var regions = new[]
        {
            new GridDockRegionDefinition([0], [1]),
            new GridDockRegionDefinition([2], [1])
        };

        Assert.Throws<ArgumentException>(() =>
            new GridDockColumnLayoutAdapter(3, regions));
    }

    [Fact]
    public void AdapterReportsVisibleContentForTheConfiguredColumn()
    {
        var tool = new Tool
        {
            Id = "tool",
            Column = 0
        };
        var dock = new GridDock
        {
            Id = "grid",
            VisibleDockables = new List<IDockable> { tool }
        };
        var adapter = new GridDockColumnLayoutAdapter(
            "grid",
            3,
            new GridDockRegionDefinition([0], [1]));

        var state = adapter.GetLayoutState(dock);

        Assert.NotNull(state);
        Assert.True(state.Regions[0].HasVisibleContent);
    }

    [Fact]
    public void AdapterIgnoresDockWithAnotherTargetId()
    {
        var adapter = new GridDockColumnLayoutAdapter(
            "target",
            2,
            new GridDockRegionDefinition([0], [1]));

        var state = adapter.GetLayoutState(new GridDock { Id = "other" });

        Assert.Null(state);
    }
}

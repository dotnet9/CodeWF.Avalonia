using CodeWF.Avalonia.DataGrid;
using Xunit;

namespace CodeWF.Avalonia.Controls.Tests;

public sealed class DataGridNaturalSortComparerTests
{
    [Fact]
    public void CompareText_OrdersEmbeddedNumbersNaturally()
    {
        Assert.True(DataGridNaturalSortComparer.CompareText("Task 2", "Task 10") < 0);
        Assert.True(DataGridNaturalSortComparer.CompareText("Task 10", "Task 2") > 0);
    }

    [Fact]
    public void Compare_UsesNestedPropertyPath()
    {
        var comparer = new DataGridNaturalSortComparer("Details.Name");
        var first = new Row(new Details("Item 2"));
        var second = new Row(new Details("Item 10"));

        Assert.True(comparer.Compare(first, second) < 0);
    }

    [Fact]
    public void Compare_OrdersNullValuesBeforeNonNullValues()
    {
        var comparer = new DataGridNaturalSortComparer("Details.Name");
        var empty = new Row(null);
        var value = new Row(new Details("Item 1"));

        Assert.True(comparer.Compare(empty, value) < 0);
        Assert.Equal(0, comparer.Compare(empty, empty));
    }

    [Fact]
    public void Compare_MissingPropertyPathDoesNotThrow()
    {
        var comparer = new DataGridNaturalSortComparer("Missing.Name");
        var result = comparer.Compare(new Row(new Details("Item 1")), new Row(new Details("Item 2")));

        Assert.Equal(0, result);
    }

    private sealed class Row
    {
        public Row(Details? details)
        {
            Details = details;
        }

        public Details? Details { get; }
    }

    private sealed class Details
    {
        public Details(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}

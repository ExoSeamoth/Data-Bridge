using DataBridge.Utils.Entries;
using FluentAssertions;

namespace DataBridge.Tests;

public sealed class ObservableStackTests
{
    [Fact]
    public void Push_ShouldUpdateCurrentItem()
    {
        ObservableStack<string> stack = new();

        stack.Push("abc");

        stack.CurrentItem.Should().Be("abc");
    }

    [Fact]
    public void Pop_ShouldUpdateCurrentItem()
    {
        ObservableStack<string> stack = new();
        stack.Push("First");
        stack.Push("Second");

        stack.CurrentItem.Should().Be("Second");

        stack.Pop();

        stack.CurrentItem.Should().Be("First");
    }
}
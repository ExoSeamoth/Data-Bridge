using System.Globalization;
using DataBridgeRework.Utils.Converters;
using FluentAssertions;

namespace DataBridgeRework.Tests;

public sealed class FullPathToFolderNameConverterTests
{
    [Theory]
    [InlineData(@"C:\Users\Docs\", "Docs")]
    [InlineData("/", "/")]
    [InlineData("/home/exo", "exo")]
    public void Convert_ShouldReturnLastFolder(string input, string expected)
    {
        var converter = new FullPathToFolderNameConverter();
        var result = converter.Convert(input, typeof(string), null, CultureInfo.InvariantCulture);

        result.Should().Be(expected);
    }
}
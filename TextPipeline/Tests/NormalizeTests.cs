using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;

namespace Tests;

public class NormalizeTests
{
    [Fact]
    public void Normalize_NormalText_ReturnsLowercaseText()
    {
        var result = TextOperations.Normalize("Hello World");

        Assert.Equal("hello world", result);
    }

    [Fact]
    public void Normalize_LeadingAndTrailingSpaces_RemovesSpaces()
    {
        var result = TextOperations.Normalize("   Hello World   ");

        Assert.Equal("hello world", result);
    }

    [Fact]
    public void Normalize_MultipleSpaces_CollapsesToOneSpace()
    {
        var result = TextOperations.Normalize("Hello    World");

        Assert.Equal("hello world", result);
    }

    [Fact]
    public void Normalize_WhitespaceCharacters_CollapsesToOneSpace()
    {
        var result = TextOperations.Normalize("Hello\t\tWorld\nTest");

        Assert.Equal("hello world test", result);
    }

    [Fact]
    public void Normalize_EmptyString_ReturnsEmptyString()
    {
        var result = TextOperations.Normalize("");

        Assert.Equal("", result);
    }

    [Fact]
    public void Normalize_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => TextOperations.Normalize(null!));
    }
}

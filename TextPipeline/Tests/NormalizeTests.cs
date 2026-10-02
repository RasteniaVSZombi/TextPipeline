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
        // Проверяем перевод обычного текста в нижний регистр.
        var result = TextOperations.Normalize("Hello World");

        Assert.Equal("hello world", result);
    }

    [Fact]
    public void Normalize_LeadingAndTrailingSpaces_RemovesSpaces()
    {
        // Проверяем удаление пробелов в начале и конце строки.
        var result = TextOperations.Normalize("   Hello World   ");

        Assert.Equal("hello world", result);
    }

    [Fact]
    public void Normalize_MultipleSpaces_CollapsesToOneSpace()
    {
        // Проверяем замену нескольких пробелов одним.
        var result = TextOperations.Normalize("Hello    World");

        Assert.Equal("hello world", result);
    }

    [Fact]
    public void Normalize_WhitespaceCharacters_CollapsesToOneSpace()
    {
        // Проверяем обработку табуляции и переноса строки.
        var result = TextOperations.Normalize("Hello\t\tWorld\nTest");

        Assert.Equal("hello world test", result);
    }

    [Fact]
    public void Normalize_EmptyString_ReturnsEmptyString()
    {
        // Проверяем корректную обработку пустой строки.
        var result = TextOperations.Normalize("");

        Assert.Equal("", result);
    }

    [Fact]
    public void Normalize_NullInput_ThrowsArgumentNullException()
    {
        // Проверяем ошибку при передаче null.
        Assert.Throws<ArgumentNullException>(() => TextOperations.Normalize(null!));
    }
}

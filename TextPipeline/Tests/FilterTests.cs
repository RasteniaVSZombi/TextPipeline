using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;

namespace Tests;

public class FilterTests
{
    [Fact]
    public void FilterByCharType_LettersOnly_ReturnsOnlyLetters()
    {
        var result = TextOperations.FilterByCharType(
            "abc123!@#",
            CharacterType.Letters);

        Assert.Equal("abc", result);
    }

    [Fact]
    public void FilterByCharType_DigitsOnly_ReturnsOnlyDigits()
    {
        var result = TextOperations.FilterByCharType(
            "abc123!@#",
            CharacterType.Digits);

        Assert.Equal("123", result);
    }

    [Fact]
    public void FilterByCharType_OtherOnly_ReturnsOnlyOtherCharacters()
    {
        var result = TextOperations.FilterByCharType(
            "abc 123!@#",
            CharacterType.Other);

        Assert.Equal(" !@#", result);
    }

    [Fact]
    public void FilterByCharType_LettersAndDigits_ReturnsLettersAndDigits()
    {
        var result = TextOperations.FilterByCharType(
            "abc123!@#",
            CharacterType.Letters | CharacterType.Digits);

        Assert.Equal("abc123", result);
    }

    [Fact]
    public void FilterByCharType_EmptyString_ReturnsEmptyString()
    {
        var result = TextOperations.FilterByCharType(
            "",
            CharacterType.Letters);

        Assert.Equal("", result);
    }

    [Fact]
    public void FilterByCharType_NoCharacterType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            TextOperations.FilterByCharType(
                "abc123",
                CharacterType.None));
    }
}

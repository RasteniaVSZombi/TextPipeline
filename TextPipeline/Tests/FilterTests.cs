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
        // Проверяем выбор только букв из исходной строки.
        var result = TextOperations.FilterByCharType(
            "abc123!@#",
            CharacterType.Letters);

        Assert.Equal("abc", result);
    }

    [Fact]
    public void FilterByCharType_DigitsOnly_ReturnsOnlyDigits()
    {
        // Проверяем выбор только цифр из исходной строки.
        var result = TextOperations.FilterByCharType(
            "abc123!@#",
            CharacterType.Digits);

        Assert.Equal("123", result);
    }

    [Fact]
    public void FilterByCharType_OtherOnly_ReturnsOnlyOtherCharacters()
    {
        // Проверяем выбор символов, которые не являются буквами или цифрами.
        var result = TextOperations.FilterByCharType(
            "abc 123!@#",
            CharacterType.Other);

        Assert.Equal(" !@#", result);
    }

    [Fact]
    public void FilterByCharType_LettersAndDigits_ReturnsLettersAndDigits()
    {
        // Проверяем одновременный выбор букв и цифр.
        var result = TextOperations.FilterByCharType(
            "abc123!@#",
            CharacterType.Letters | CharacterType.Digits);

        Assert.Equal("abc123", result);
    }

    [Fact]
    public void FilterByCharType_EmptyString_ReturnsEmptyString()
    {
        // Проверяем корректную обработку пустой строки.
        var result = TextOperations.FilterByCharType(
            "",
            CharacterType.Letters);

        Assert.Equal("", result);
    }

    [Fact]
    public void FilterByCharType_NoCharacterType_ThrowsArgumentException()
    {
        // Проверяем ошибку при отсутствии выбранного типа символов.
        Assert.Throws<ArgumentException>(() =>
            TextOperations.FilterByCharType(
                "abc123",
                CharacterType.None));
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;

namespace Tests;

public class MaskTests
{
    [Fact]
    public void ApplyMask_StandardPhoneMask_ReturnsFormattedString()
    {
        // Проверяем форматирование номера телефона по маске.
        var result = TextOperations.ApplyMask(
            "1234567890",
            "+7 (###) ###-##-##");

        Assert.Equal("+7 (123) 456-78-90", result);
    }

    [Fact]
    public void ApplyMask_OnePlaceholder_ReturnsOneInputCharacter()
    {
        // Проверяем замену одного символа # символом из входной строки.
        var result = TextOperations.ApplyMask(
            "123",
            "X#Y");

        Assert.Equal("X1Y", result);
    }

    [Fact]
    public void ApplyMask_ExactNumberOfCharacters_ReturnsCorrectResult()
    {
        // Проверяем маску, количество символов которой точно совпадает с вводом.
        var result = TextOperations.ApplyMask(
            "123",
            "###");

        Assert.Equal("123", result);
    }

    [Fact]
    public void ApplyMask_ExtraInputCharacters_IgnoresExtraCharacters()
    {
        // Проверяем, что лишние символы входной строки не используются.
        var result = TextOperations.ApplyMask(
            "123456",
            "##-##");

        Assert.Equal("12-34", result);
    }

    [Fact]
    public void ApplyMask_MaskWithoutPlaceholder_ThrowsArgumentException()
    {
        // Проверяем ошибку при отсутствии символов # в маске.
        Assert.Throws<ArgumentException>(() =>
            TextOperations.ApplyMask(
                "123",
                "ABC"));
    }

    [Fact]
    public void ApplyMask_InsufficientInputCharacters_ThrowsArgumentException()
    {
        // Проверяем ошибку, если входных символов недостаточно для маски.
        Assert.Throws<ArgumentException>(() =>
            TextOperations.ApplyMask(
                "12",
                "###"));
    }
}

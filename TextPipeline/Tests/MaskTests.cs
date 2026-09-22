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
        var result = TextOperations.ApplyMask(
            "1234567890",
            "+7 (###) ###-##-##");

        Assert.Equal("+7 (123) 456-78-90", result);
    }

    [Fact]
    public void ApplyMask_OnePlaceholder_ReturnsOneInputCharacter()
    {
        var result = TextOperations.ApplyMask(
            "123",
            "X#Y");

        Assert.Equal("X1Y", result);
    }

    [Fact]
    public void ApplyMask_ExactNumberOfCharacters_ReturnsCorrectResult()
    {
        var result = TextOperations.ApplyMask(
            "123",
            "###");

        Assert.Equal("123", result);
    }

    [Fact]
    public void ApplyMask_ExtraInputCharacters_IgnoresExtraCharacters()
    {
        var result = TextOperations.ApplyMask(
            "123456",
            "##-##");

        Assert.Equal("12-34", result);
    }

    [Fact]
    public void ApplyMask_MaskWithoutPlaceholder_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            TextOperations.ApplyMask(
                "123",
                "ABC"));
    }

    [Fact]
    public void ApplyMask_InsufficientInputCharacters_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            TextOperations.ApplyMask(
                "12",
                "###"));
    }
}

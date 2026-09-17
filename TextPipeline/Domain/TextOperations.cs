using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;

namespace Domain;

[Flags]
public enum CharacterType
{
    None = 0,
    Letters = 1,
    Digits = 2,
    Other = 4
}

public static class TextOperations
{
    /// <summary>
    /// Нормализует строку:
    /// - убирает пробелы в начале и конце;
    /// - заменяет последовательности whitespace одним пробелом;
    /// - переводит строку в нижний регистр.
    /// </summary>
    public static string Normalize(string input)
    {
        Guard.RequiresNotNull(input, nameof(input));

        string result = Regex.Replace(input.Trim(), @"\s+", " ").ToLowerInvariant();

        // Post: результат не содержит пробелов по краям
        // и не содержит последовательностей из нескольких whitespace.
        Debug.Assert(result == result.Trim());
        Debug.Assert(!Regex.IsMatch(result, @"\s{2,}"));

        return result;
    }

    /// <summary>
    /// Оставляет в строке только символы выбранного типа.
    /// Можно выбрать один или несколько типов.
    /// </summary>
    public static string FilterByCharType(string input, CharacterType characterTypes)
    {
        Guard.RequiresNotNull(input, nameof(input));

        Guard.Requires(characterTypes != CharacterType.None, "Необходимо выбрать хотя бы один тип символов.");

        string result = string.Empty;

        foreach (char character in input)
        {
            bool isLetter = char.IsLetter(character) && characterTypes.HasFlag(CharacterType.Letters);

            bool isDigit = char.IsDigit(character) && characterTypes.HasFlag(CharacterType.Digits);

            bool isOther = !char.IsLetterOrDigit(character) && characterTypes.HasFlag(CharacterType.Other);

            if (isLetter || isDigit || isOther)
            {
                result += character;
            }
        }

        // Post: каждый символ результата относится
        // к одному из выбранных типов.
        foreach (char character in result)
        {
            bool valid = (char.IsLetter(character) && characterTypes.HasFlag(CharacterType.Letters)) || (char.IsDigit(character) && characterTypes.HasFlag(CharacterType.Digits)) || (!char.IsLetterOrDigit(character) && characterTypes.HasFlag(CharacterType.Other));
            Debug.Assert(valid);
        }

        return result;
    }

    /// <summary>
    /// Подставляет символы входной строки по порядку
    /// вместо каждого символа '#' в маске.
    /// </summary>
    public static string ApplyMask(string input, string mask)
    {
        Guard.RequiresNotNull(input, nameof(input));
        Guard.RequiresNotNull(mask, nameof(mask));

        Guard.Requires(mask.Contains('#'), "Маска должна содержать хотя бы один символ '#'.");

        int requiredCharacters = mask.Count(c => c == '#');

        Guard.Requires(input.Length >= requiredCharacters, "Во входной строке недостаточно символов для применения маски.");

        string result = string.Empty;
        int inputIndex = 0;

        foreach (char character in mask)
        {
            if (character == '#')
            {
                result += input[inputIndex];
                inputIndex++;
            }
            else
            {
                result += character;
            }
        }

        // Post: количество символов '#' в маске
        // соответствует количеству использованных символов входа.
        Debug.Assert(inputIndex == requiredCharacters);
        return result;
    }
}
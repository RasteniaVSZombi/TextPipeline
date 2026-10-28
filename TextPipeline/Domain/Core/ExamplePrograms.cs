using System.Collections.Generic;
using WpEngine.Models;

namespace WpEngine.Core;

public static class ExamplePrograms
{
    public static IReadOnlyList<WpExample> All { get; } =
        new List<WpExample>
        {
            new(
                "Ручной ввод",
                """

                """,
                "",
                ""
            ),

            new(
                "Максимум двух чисел",
                """
                if a >= b then
                    max := a
                else
                    max := b
                end
                """,
                "(max >= a) && (max >= b) && " +
                "((max == a) || (max == b))",
                "После выполнения переменная max содержит " +
                "максимальное из чисел a и b."
            ),

            new(
                "Квадратное уравнение",
                """
                D := b * b - 4 * a * c
                if D < 0 then
                    rootsCount := 0
                    x1 := 0
                    x2 := 0
                else
                    if D == 0 then
                        rootsCount := 1
                        x1 := -b / (2 * a)
                        x2 := x1
                    else
                        rootsCount := 2
                        x1 := (-b - sqrt(D)) / (2 * a)
                        x2 := (-b + sqrt(D)) / (2 * a)
                    end
                end
                """,
                "rootsCount > 0",
                "После выполнения программы уравнение " +
                "ax² + bx + c = 0 имеет хотя бы один " +
                "действительный корень. Предполагается, " +
                "что a не равно нулю."
            ),

            new(
                "Последовательность присваиваний",
                """
                x := a + 2
                y := x * 3
                z := y - 4
                """,
                "z > 10",
                "После выполнения последовательности значение z " +
                "должно быть больше 10."
            ),

            new(
                "Поиск максимальной длины слов",
                """
                if wordLength1 >= wordLength2 then
                    maxLength := wordLength1
                else
                    maxLength := wordLength2
                end
                """,
                "(maxLength >= wordLength1) && (maxLength >= wordLength2) && " +
                "((maxLength == wordLength1) || (maxLength == wordLength2))",
                "После выполнения переменная maxLength содержит " +
                "максимальную длину из двух проанализированных слов текста."
            ),

            new(
                "Анализ форматирования текста",
                """
                D := lineLength * lineLength - 4 * paragraph * minWidth
                if D < 0 then
                    formatStatus := 0
                    lineStart := 0
                    lineEnd := 0
                else
                    if D == 0 then
                        formatStatus := 1
                        lineStart := -lineLength / (2 * paragraph)
                        lineEnd := lineStart
                    else
                        formatStatus := 2
                        lineStart := (-lineLength - sqrt(D)) / (2 * paragraph)
                        lineEnd := (-lineLength + sqrt(D)) / (2 * paragraph)
                    end
                end
                """,
                "formatStatus > 0",
                "После выполнения программа определяет корректность " +
                "форматирования текстового блока. Предполагается, " +
                "что ширина абзаца не равна нулю."
            ),

            new(
                "Преобразование текстового формата",
                """
                processedLength := originalLength + 2
                formattedLength := processedLength * 3
                finalLength := formattedLength - 4
                """,
                "finalLength > 10",
                "После последовательной обработки текста " +
                "итоговая длина форматированного текста " +
                "должна превышать 10 символов."
            )

        };
}
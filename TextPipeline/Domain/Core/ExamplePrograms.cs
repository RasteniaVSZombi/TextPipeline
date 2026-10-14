
using System.Collections.Generic;
using WpEngine.Models;

namespace WpEngine.Core;

public static class ExamplePrograms
{
    public static IReadOnlyList<WpExample> All { get; } =
        new List<WpExample>
        {
            new(
                "Максимум двух чисел",
                """
                if a >= b then
                    max := a
                else
                    max := b
                end
                """,
                "(max >= a) && (max >= b) && ((max == a) || (max == b))"
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
                "(rootsCount >= 0) && (rootsCount <= 2)"
            ),

            new(
                "Последовательность присваиваний",
                """
                x := a + 2
                y := x * 3
                z := y - 4
                """,
                "z > 10"
            )
        };
}

using System;

namespace Domain;

public static class Guard
{
    //проверяет любое условие которое должно быть выполнено перед методом
    public static void Requires(bool condition, string message)
    {
        if (!condition)
        {
            throw new ArgumentException(message);
        }
    }

    public static void RequiresNotNull(object? value, string paramName)
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }
    }
}
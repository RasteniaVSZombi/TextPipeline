namespace WpEngine.Loops;

public static class LoopPresetProvider
{
    private static readonly IReadOnlyList<LoopPreset> Presets =
        new List<LoopPreset>
        {
            new()
            {
                Name = "Сумма",

                Strategy = LoopStrategies.PrefixSum,

                Array = new[] { 2, 4, 6, 8, 10 },

                ExpectedResult = 30
            },

            new()
            {
                Name = "Счётчик > T",

                Strategy = LoopStrategies.CountGreaterThanT,

                Array = new[] { 2, 9, 4, 12, 7, 3 },

                Threshold = 5,

                ExpectedResult = 3
            },

            new()
            {
                Name = "Максимум",

                Strategy = LoopStrategies.PrefixMax,

                Array = new[] { -8, -3, -15, -1, -7 },

                ExpectedResult = -1
            }
        };

    public static IReadOnlyList<LoopPreset> GetAll()
    {
        return Presets;
    }

    public static LoopPreset GetByName(string name)
    {
        return Presets.FirstOrDefault(p =>
            string.Equals(
                p.Name,
                name,
                StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException(
                $"Пресет '{name}' не найден.");
    }
}
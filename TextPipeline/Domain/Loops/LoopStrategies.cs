namespace WpEngine.Loops;

public static class LoopStrategies
{
    public static LoopStrategy PrefixSum { get; } = new()
    {
        Mode = LoopMode.PrefixSum,
        Name = "Сумма",

        Pre = "n >= 0 && j == 0 && res == 0",

        Invariant = "res == prefixSum && 0 <= j && j <= n",

        Post = "res == totalSum",

        Variant = "n - j",

        Guard = "j < n",

        Body = """
            res := res + x
            j := j + 1
            """,

        PreExplanation =
            "До начала цикла индекс равен нулю, " +
            "накопитель равен нулю, длина массива неотрицательна.",

        InvariantExplanation =
            "res равен сумме первых j элементов массива. " +
            "Индекс находится в диапазоне от 0 до n.",

        PostExplanation =
            "После обработки всех элементов накопитель равен сумме массива.",

        VariantExplanation =
            "n - j — количество необработанных элементов. " +
            "Оно уменьшается на единицу.",

        BodyExplanation =
            "К накопленной сумме прибавляется текущий элемент x, " +
            "затем индекс увеличивается."
    };

    public static LoopStrategy CountGreaterThanT { get; } = new()
    {
        Mode = LoopMode.CountGreaterThanThreshold,
        Name = "Счётчик > T",

        Pre = "n >= 0 && j == 0 && res == 0",

        Invariant = "res == prefixCount && 0 <= j && j <= n",

        Post = "res == totalCount",

        Variant = "n - j",

        Guard = "j < n",

        Body = """
            if x > T then
            res := res + 1
            else
            res := res
            end
            j := j + 1
            """,

        PreExplanation =
            "Индекс и счётчик начинаются с нуля.",

        InvariantExplanation =
            "res равен количеству элементов среди первых j элементов, " +
            "которые строго больше T.",

        PostExplanation =
            "После обработки массива res равен количеству элементов, " +
            "строго больших T.",

        VariantExplanation =
            "n - j — количество необработанных элементов. " +
            "Оно уменьшается на каждой итерации.",

        BodyExplanation =
            "Если текущий элемент больше T, счётчик увеличивается. " +
            "После этого индекс увеличивается."
    };

    public static LoopStrategy PrefixMax { get; } = new()
    {
        Mode = LoopMode.PrefixMax,
        Name = "Максимум",

        Pre = "n >= 1 && j == 1 && res == first",

        Invariant = "res == prefixMax && 1 <= j && j <= n",

        Post = "res == totalMax",

        Variant = "n - j",

        Guard = "j < n",

        Body = """
            if x > res then
            res := x
            else
            res := res
            end
            j := j + 1
            """,

        PreExplanation =
            "Массив непустой. Первый элемент уже обработан: " +
            "j равно 1, res равен A[0].",

        InvariantExplanation =
            "res является максимальным элементом среди первых j элементов.",

        PostExplanation =
            "После обработки всех элементов res равен максимуму массива.",

        VariantExplanation =
            "n - j — количество необработанных элементов. " +
            "Оно уменьшается на каждой итерации.",

        BodyExplanation =
            "Если текущий элемент больше накопленного максимума, " +
            "он становится новым максимумом."
    };

    public static IReadOnlyList<LoopStrategy> All { get; } =
        new[]
        {
            PrefixSum,
            CountGreaterThanT,
            PrefixMax
        };

    public static LoopStrategy Get(LoopMode mode)
    {
        return All.Single(strategy => strategy.Mode == mode);
    }
}
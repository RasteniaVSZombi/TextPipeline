namespace WpEngine.Loops;

public sealed class LoopExecutionState
{
    private readonly int[] _array;

    public IReadOnlyList<int> Array => _array;

    public int J { get; private set; }
    public int Res { get; private set; }
    public int Threshold { get; }
    public bool IsCompleted { get; private set; }
    public LoopMode Mode { get; }

    public int Length => _array.Length;
    public int Remaining => Length - J;

    public LoopExecutionState(
        IEnumerable<int> array,
        LoopMode mode,
        int threshold = 0,
        int initialIndex = 0,
        int initialResult = 0)
    {
        ArgumentNullException.ThrowIfNull(array);

        _array = array.ToArray();

        if (initialIndex < 0 || initialIndex > _array.Length)
            throw new ArgumentOutOfRangeException(nameof(initialIndex));

        if (mode == LoopMode.PrefixMax && _array.Length == 0)
            throw new ArgumentException(
                "Для поиска максимума массив не должен быть пустым.");

        if (mode == LoopMode.PrefixMax &&
            (initialIndex != 1 || initialResult != _array[0]))
        {
            throw new ArgumentException(
                "Для PrefixMax начальное состояние должно иметь j = 1 и res = A[0].");
        }

        Mode = mode;
        Threshold = threshold;
        J = initialIndex;
        Res = initialResult;

        IsCompleted = J >= _array.Length;
    }

    public bool ExecuteOneStep()
    {
        if (IsCompleted)
            return false;

        int current = _array[J];

        switch (Mode)
        {
            case LoopMode.PrefixSum:
                Res = checked(Res + current);
                break;

            case LoopMode.CountGreaterThanThreshold:
                if (current > Threshold)
                    Res = checked(Res + 1);
                break;

            case LoopMode.PrefixMax:
                Res = Math.Max(Res, current);
                break;

            default:
                throw new InvalidOperationException(
                    $"Неизвестный режим: {Mode}.");
        }

        J++;
        IsCompleted = J >= _array.Length;

        return true;
    }

    public void ExecuteAll()
    {
        while (ExecuteOneStep())
        {
        }
    }
}
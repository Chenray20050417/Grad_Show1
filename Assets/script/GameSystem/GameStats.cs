public static class GameStats
{
    public static int BrokenStressBalls { get; private set; }
    public static int UsedSupplements { get; private set; }
    public static int HurtCount { get; private set; }

    public static void Reset()
    {
        BrokenStressBalls = 0;
        UsedSupplements = 0;
        HurtCount = 0;
    }

    public static void AddBrokenStressBall()
    {
        BrokenStressBalls++;
    }

    public static void AddUsedSupplement()
    {
        UsedSupplements++;
    }

    public static void AddHurt()
    {
        HurtCount++;
    }
}

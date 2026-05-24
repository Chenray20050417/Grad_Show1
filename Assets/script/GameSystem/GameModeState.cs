public static class GameModeState
{
    public static bool IsTrialMode { get; private set; }
    public static string ReturnSceneName { get; private set; } = "MainMenu";

    public static void StartStoryMode()
    {
        IsTrialMode = false;
        ReturnSceneName = "MainMenu";
        GameStats.Reset();
    }

    public static void StartTrialMode()
    {
        IsTrialMode = true;
        ReturnSceneName = "MainMenu";
        GameStats.Reset();
    }

    public static void ClearTrialMode()
    {
        IsTrialMode = false;
        ReturnSceneName = "MainMenu";
    }
}

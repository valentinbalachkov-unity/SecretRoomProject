using System.Collections.Generic;

public static class GameConfig
{
    public const int MAX_ROUND = 15;
    public const int MAX_CARDS_IN_HAND = 3;
    public const int MAX_PLAYERS_COUNT = 6;
    
    public const int TIMER_PHASE_1 = 60;
    public const int TIMER_PHASE_2 = 60;
    public const int TIMER_PHASE_3 = 15;
    public const int TIMER_PHASE_4 = 60;

    public static List<string> ExampleText = new()
    {
        "тестовый прекол 1", "тестовый прекол 2", "тестовый прекол 3"
    };
}
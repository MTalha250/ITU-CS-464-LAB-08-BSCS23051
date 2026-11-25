using UnityEngine;

// The player's progress for this session: coins, stars, lanterns.
public static class GameData
{
    public const int LevelCount = 12;
    public static int Coins = 1250;
    public static int Level = 7;
    public static readonly int[] Stars = { 3, 3, 2, 3, 1, 2, 0, 0, 0, 0, 0, 0 };
    public static readonly string[] LevelNames =
    {
        "First Light", "Reed Maze", "Stone Steps", "Willow Bend", "Fog Bank", "Old Mill",
        "Firefly Run", "Night Market", "Rapids", "Moon Pool", "Two Rivers", "The Sea",
    };

    public static readonly string[] Lanterns = { "Paper Moon", "Lotus", "Firefly", "Aurora" };
    public static readonly int[] Price = { 0, 0, 600, 1500 };
    public static readonly bool[] Owned = { true, true, false, false };
    public static readonly Color32[] LanternColors =
    {
        new Color32(255, 226, 168, 255), new Color32(255, 143, 177, 255),
        new Color32(198, 242, 90, 255), new Color32(150, 132, 255, 255),
    };
    public static readonly int[][] Stats =      // glow, speed, drift, out of 5
    {
        new[] { 3, 2, 3 }, new[] { 2, 3, 4 }, new[] { 4, 4, 2 }, new[] { 5, 3, 5 },
    };
    public static int Equipped;
    public static int Magnets = 1, ExtraLights;

    public static int LastSparks, LastStars, LastScore, LastCoins;
    public static bool LastWon;

    // the highest level you can play: one past the last level with any stars
    public static int Unlocked
    {
        get
        {
            int n = 1;
            while (n < LevelCount && Stars[n - 1] > 0) n++;
            return n;
        }
    }

    public static string LevelTitle(int level) => $"Level {level} · {LevelNames[level - 1]}";

    public static void Finish(int sparks, bool won)
    {
        LastWon = won;
        LastSparks = sparks;
        LastStars = !won ? 0 : sparks >= 15 ? 3 : sparks >= 9 ? 2 : sparks >= 4 ? 1 : 0;   // no stars if the light went out
        LastScore = sparks * 100 + LastStars * 250;
        LastCoins = sparks * 5;
        Coins += LastCoins;
        Stars[Level - 1] = Mathf.Max(Stars[Level - 1], LastStars);
    }
}

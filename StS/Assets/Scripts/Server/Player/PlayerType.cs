public enum PlayerState { Disconnected, Connection, Selection, Ready, WaitingForTheVote, Voted, InGame }

// Type Player
public class Player
{
    public string Pseudo { get; set; }
    public PlayerState PlayerState { get; set; }
    public CharacterBody Body { get; set; }

    public int Score;

    public int Victory;

    #region last Game Stats
    public int LastGameLanternSuccess;
    public int LastGamePowerUse;
    public int LastGameBestCombo;
    public int LastGameTotalLantern;
    public int LastGameScore;
    public int LastGameRank;
    #endregion
}

public class AI_Player
{
    #region Constants

    public static readonly int MIN_SUCCESS_RATE = 40;
    public static readonly int MAX_SUCCESS_RATE = 90;

    #endregion

    public string Name;

    public CharacterBody Body;

    public int Score;

    public int Difficulty;

    public int Victory;

    #region last Game Stats
    public int LastGameLanternSuccess;
    public int LastGamePowerUse;
    public int LastGameBestCombo;
    public int LastGameTotalLantern;
    public int LastGameScore;
    #endregion
}

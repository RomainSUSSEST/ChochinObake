public enum PlayerState { Disconnected, Connection, Selection, Ready, WaitingForTheVote, Voted, InGame }

// Type Player
public class Player
{
    public string Pseudo { get; set; }
    public PlayerState PlayerState { get; set; }
    public CharacterBody Body { get; set; }

    public int Score;
}

public class AI_Player
{
    public string Name;

    public CharacterBody Body;

    public int ID;

    public int Score;
}

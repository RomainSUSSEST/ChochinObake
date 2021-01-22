public enum PlayerState { Disconnected, Connection, Selection, Ready, WaitingForTheVote, Voted, InGame }

// Type Player
public class Player
{
    public string Pseudo { get; set; }
    public PlayerState PlayerState { get; set; }
    public CharacterHats Hat { get; set; }
    public CharacterBody Body { get; set; }
}

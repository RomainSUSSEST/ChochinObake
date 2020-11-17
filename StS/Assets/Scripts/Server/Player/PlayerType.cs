public enum PlayerState { Disconnected, Connection, Selection, Ready, MusicSelection, InGame }

// Type Player
public class Player
{
    public string Pseudo { get; set; }
    public PlayerState PlayerState { get; set; }
    public SlimeHats Hat { get; set; }
    public SlimeBody Body { get; set; }
}

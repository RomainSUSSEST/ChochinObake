using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DepartureCamera : ServerSimpleGameStateObserver
{
    public enum State
    {
        MainMenu,
        MusicSelection
    }

    #region Attributes

    private Animator anim;

    private State CurrentState;

    #endregion

    #region Life cycle

    protected override void Awake()
    {
        base.Awake();

        CurrentState = State.MainMenu;
        anim = GetComponent<Animator>();
    }

    #endregion

    #region GameEvent callbacks

    protected override void GameMainMenu(GameMainMenuEvent e)
    {
        base.GameMainMenu(e);

        if (CurrentState != State.MainMenu)
        {
            anim.SetTrigger("State_MainMenu");
            CurrentState = State.MainMenu;
        }
    }

    protected override void GameMusicSelectionMenu(GameMusicSelectionMenuEvent e)
    {
        base.GameMusicSelectionMenu(e);

        if (CurrentState != State.MusicSelection)
        {
            anim.SetTrigger("State_MusicSelection");
            CurrentState = State.MusicSelection;
        }
    }

    #endregion
}

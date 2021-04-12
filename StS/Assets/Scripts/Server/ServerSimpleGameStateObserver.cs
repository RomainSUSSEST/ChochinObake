using UnityEngine;
using SDD.Events;

public abstract class ServerSimpleGameStateObserver : MonoBehaviour, IEventHandler {

	// Event subscription
	public virtual void SubscribeEvents()
	{
		EventManager.Instance.AddListener<GameMainMenuEvent>(GameMainMenu);
		EventManager.Instance.AddListener<GameRoomMenuEvent>(GameRoomMenu);
		EventManager.Instance.AddListener<GameOptionsMenuEvent>(GameOptionsMenu);
		EventManager.Instance.AddListener<GameMusicSelectionMenuEvent>(GameMusicSelectionMenu);
		EventManager.Instance.AddListener<GameMusicResultMenuEvent>(GameMusicResultMenu);

		EventManager.Instance.AddListener<GamePlayEvent>(GamePlay);
		EventManager.Instance.AddListener<GameEndEvent>(GameEnd);
		EventManager.Instance.AddListener<GameResultEvent>(GameResult);

		EventManager.Instance.AddListener<GamePauseEvent>(GamePause);
		EventManager.Instance.AddListener<GameContinueEvent>(GameContinue);
	}

	public virtual void UnsubscribeEvents()
	{
		EventManager.Instance.RemoveListener<GameMainMenuEvent>(GameMainMenu);
		EventManager.Instance.RemoveListener<GameRoomMenuEvent>(GameRoomMenu);
		EventManager.Instance.RemoveListener<GameOptionsMenuEvent>(GameOptionsMenu);
		EventManager.Instance.RemoveListener<GameMusicSelectionMenuEvent>(GameMusicSelectionMenu);
		EventManager.Instance.RemoveListener<GameMusicResultMenuEvent>(GameMusicResultMenu);

		EventManager.Instance.RemoveListener<GamePlayEvent>(GamePlay);
		EventManager.Instance.RemoveListener<GameEndEvent>(GameEnd);
		EventManager.Instance.RemoveListener<GameResultEvent>(GameResult);

		EventManager.Instance.RemoveListener<GamePauseEvent>(GamePause);
		EventManager.Instance.RemoveListener<GameContinueEvent>(GameContinue);
	}


	// Life cycle

	protected virtual void Awake()
	{
		SubscribeEvents();
	}

	protected virtual void OnDestroy()
	{
		UnsubscribeEvents();
	}


	// Event call

	protected virtual void GameMainMenu(GameMainMenuEvent e)
	{
	}

	protected virtual void GameRoomMenu(GameRoomMenuEvent e)
	{
	}

	protected virtual void GameOptionsMenu(GameOptionsMenuEvent e)
	{
	}

	protected virtual void GameMusicSelectionMenu(GameMusicSelectionMenuEvent e)
	{
	}

	protected virtual void GameMusicResultMenu(GameMusicResultMenuEvent e)
	{
	}

	protected virtual void GamePlay(GamePlayEvent e)
	{
	}

	protected virtual void GameEnd(GameEndEvent e)
	{
	}

	protected virtual void GameResult(GameResultEvent e)
	{
	}

	protected virtual void GamePause(GamePauseEvent e)
	{
	}

	protected virtual void GameContinue(GameContinueEvent e)
	{
	}
}

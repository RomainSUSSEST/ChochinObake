using UnityEngine;
using SDD.Events;

public abstract class ServerSingletonGameStateObserver<T> :  Singleton<T>,IEventHandler where T:Component
{
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
		EventManager.Instance.AddListener <GameResultEvent>(GameResult);
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
	}


	// Life cycle

	protected override void Awake()
	{
		base.Awake();
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
}

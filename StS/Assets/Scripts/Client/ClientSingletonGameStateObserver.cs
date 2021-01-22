using SDD.Events;
using UnityEngine;

public abstract class ClientSingletonGameStateObserver<T> : Singleton<T>, IEventHandler where T:Component
{
    // Event subscription
    public virtual void SubscribeEvents()
    {
        EventManager.Instance.AddListener<MobileMainMenuEvent>(MobileMainMenu);
        EventManager.Instance.AddListener<MobileJoinRoomEvent>(MobileJoinRoom);
        EventManager.Instance.AddListener<MobileCharacterSelectionEvent>(MobileCharacterSelection);
        EventManager.Instance.AddListener<MobileMusicSelectionEvent>(MobileMusicSelection);
        EventManager.Instance.AddListener<MobileMusicResultEvent>(MobileMusicResult);
        EventManager.Instance.AddListener<MobileGamePlayEvent>(MobileGamePlay);
    }

    public virtual void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<MobileMainMenuEvent>(MobileMainMenu);
        EventManager.Instance.RemoveListener<MobileJoinRoomEvent>(MobileJoinRoom);
        EventManager.Instance.RemoveListener<MobileCharacterSelectionEvent>(MobileCharacterSelection);
        EventManager.Instance.RemoveListener<MobileMusicSelectionEvent>(MobileMusicSelection);
        EventManager.Instance.RemoveListener<MobileMusicResultEvent>(MobileMusicResult);
        EventManager.Instance.RemoveListener<MobileGamePlayEvent>(MobileGamePlay);
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

    protected virtual void MobileMainMenu(MobileMainMenuEvent e)
    {
    }

    protected virtual void MobileJoinRoom(MobileJoinRoomEvent e)
    {
    }

    protected virtual void MobileCharacterSelection(MobileCharacterSelectionEvent e)
    {
    }

    protected virtual void MobileMusicSelection(MobileMusicSelectionEvent e)
    {
    }

    protected virtual void MobileMusicResult(MobileMusicResultEvent e)
    {
    }

    protected virtual void MobileGamePlay(MobileGamePlayEvent e)
    {
    }
}

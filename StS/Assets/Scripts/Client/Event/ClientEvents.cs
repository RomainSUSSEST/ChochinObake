using SDD.Events;

#region GameManagerEvents
// Client State

public class MobileMainMenuEvent : Event
{
}

public class MobileJoinRoomEvent : Event
{
}

public class MobileCharacterSelectionEvent : Event
{
}

public class ReadyCharacterSelectionEvent : Event
{
}

public class MobileMusicSelectionEvent : Event
{
}

public class MobileChooseMusicEvent : Event
{
}

public class MobileGamePlayEvent : Event
{
}
#endregion

#region MenuManagerEvents
// Client
public class JoinButtonClickedEvent : Event
{
}

public class LeaveButtonClickedEvent : Event
{
}

public class PreviousCharacterSelectionButtonClickedEvent : Event
{
}

public class ReadyCharacterSelectionButtonClickedEvent : Event
{
}

public class PreviousMusicSelectionButtonClickedEvent : Event
{
}

public class RespawnButtonClickedEvent : Event
{
}
#endregion

#region Network Client
public class ServerConnectionEvent : Event
{
	public string Adress;
}

public class ServerClosedEvent : Event
{
}
#endregion
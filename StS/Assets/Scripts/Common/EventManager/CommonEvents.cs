using SDD.Events;
using System.Collections.Generic;

#region UI Events
public class ResizeUIEvent : Event
{
}
public class ResizeUIRequestEvent : Event
{
}
public class ResizeUICompleteEvent : Event
{
}
#endregion

#region Network type but local raise
public class ServerConnectionSuccessEvent : Event
{
	public ulong ClientID;
}
#endregion

#region Network type but networked raise
#region Menu

#region Character Selection
public class PlayerEnterInCharacterSelectionEvent : NetworkedEvent
{
	public PlayerEnterInCharacterSelectionEvent(ulong playerID) : base(playerID)
	{
	}
}

public class LobbyInformationEvent : NetworkedEvent
{
	// Attributs

	public List<SlimeBody.BodyType> InvalidBody;


	// Constructeur

	public LobbyInformationEvent(ulong PlayerID, List<SlimeBody.BodyType> invalidBody) : base (PlayerID, new Argument()
	{
		Arg = invalidBody,
		Type = invalidBody.GetType()
	})
	{
		InvalidBody = invalidBody;
	}
}
public class RequestPlayerReadyInCharacterSelectionEvent : NetworkedEvent
{
	// Attributs

	public bool IsReady;
	public SlimeHats.HatsType HatType;
	public SlimeBody.BodyType BodyType;
	public string Pseudo;


	// Constructeur

	/**
	 * @arg if (Isready)
	 *			Hat, Body, Arms -> A utilisé
	 *		else
	 *			Hat, Body, Arms -> A libérer
	 */
	public RequestPlayerReadyInCharacterSelectionEvent(ulong playerID, bool isReady
		, SlimeHats.HatsType hat, SlimeBody.BodyType body, string pseudo)
		: base(playerID,
			new Argument() { Arg = isReady, Type = typeof(bool) },
			new Argument() { Arg = hat, Type = hat.GetType() },
			new Argument() { Arg = body, Type = body.GetType() },
			new Argument() { Arg = pseudo, Type = typeof(string) })
	{
		IsReady = isReady;
		HatType = hat;
		BodyType = body;
		Pseudo = pseudo;
	}
}

public class RequestAcceptedPlayerReadyInCharacterSelectionEvent : NetworkedEvent
{
	public RequestAcceptedPlayerReadyInCharacterSelectionEvent(ulong playerID) : base(playerID)
	{
	}
}

public class InverseStateOfColorEvent : NetworkedEvent
{
	// Attributs

	public SlimeBody.BodyType BodyType;


	// Constructeur

	public InverseStateOfColorEvent(ulong playerID, SlimeBody.BodyType bodyType) : base(playerID,
		new Argument() { Arg = bodyType, Type = bodyType.GetType() })
	{
		BodyType = bodyType;
	}
}

public class InvalidPseudoEvent : NetworkedEvent
{
	public InvalidPseudoEvent(ulong playerID) : base(playerID)
	{
	}
}

public class InvalidColorEvent : NetworkedEvent
{
	public InvalidColorEvent(ulong PlayerID) : base(PlayerID)
	{
	}
}
#endregion

#region Music Selection

#region ModelMusicSelectionClient

public class AskForMusicListEvent : NetworkedEvent
{
	public AskForMusicListEvent(ulong ClientID) : base(ClientID)
	{
	}
}

public class VoteButtonHasBeenClickedEvent : NetworkedEvent
{
	// Attributs

	public string AudioTitle;


	// Constructeur

	public VoteButtonHasBeenClickedEvent(ulong ClientID, string audioTitle) :
		base(ClientID, new Argument()
		{
			Arg = audioTitle,
			Type = typeof(string)
		})
	{
		this.AudioTitle = audioTitle;
	}
}

#endregion

#region ModelMusicSelectionServer

public class AnswerForMusicListRequestEvent : NetworkedEvent
{
	// Attributs

	public string[] MusicList;


	// Constructeur

	public AnswerForMusicListRequestEvent(ulong clientID, string[] list) :
		base(clientID, new Argument()
		{
			Arg = list,
			Type = typeof(string[])
		})
	{
		MusicList = list;
	}
}

public class MusicVoteAcceptedEvent : NetworkedEvent
{
	// Constructeur

	public MusicVoteAcceptedEvent(ulong PlayerID) : base(PlayerID)
	{
	}
}

#endregion

#endregion

#endregion

#region MobileInputs
public class DoublePressEvent : NetworkedEvent
{
	public DoublePressEvent(ulong playerID) : base(playerID)
	{
	}
}

public class SwipeLeftEvent : NetworkedEvent
{
	public SwipeLeftEvent(ulong playerID) : base(playerID)
	{
	}
}

public class SwipeRightEvent : NetworkedEvent
{
	public SwipeRightEvent(ulong playerID) : base(playerID)
	{
	}
}

public class SwipeUpEvent : NetworkedEvent
{
	public SwipeUpEvent(ulong playerID) : base(playerID)
	{
	}
}

public class SwipeDownEvent : NetworkedEvent
{
	public SwipeDownEvent(ulong playerID) : base(playerID)
	{
	}
}

// Movement

/**
* Classe représentant les mouvements
*/
public abstract class MovementEvent : NetworkedEvent
{
	// Attributs

	public float intensity { get; }


	// Constructeur

	public MovementEvent(ulong playerID, float intensity) : base(playerID,
		new Argument()
		{
			Arg = intensity,
			Type = typeof(float)
		})
	{
		this.intensity = intensity;
	}
}

// Direction

public class TiltLeftRightEvent : MovementEvent
{
	public TiltLeftRightEvent(ulong playerID, float intensity) : base(playerID, intensity)
	{
	}
}
public class TiltTopBottomEvent : MovementEvent
{
	public TiltTopBottomEvent(ulong playerID, float intensity) : base(playerID, intensity)
	{
	}
}
#endregion

#region ServerState
public class GameStartedEvent : NetworkedEvent
{
}

public class ServerEnterInGameMusicSelectionEvent : NetworkedEvent
{
}

public class RespawnEvent : NetworkedEvent
{
	public ulong PlayerID;
}
#endregion
#endregion
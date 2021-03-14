using SDD.Events;
using System;
using System.Collections.Generic;

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

	public List<CharacterBody.BodyType> InvalidBody;


	// Constructeur

	public LobbyInformationEvent(ulong PlayerID, List<CharacterBody.BodyType> invalidBody) : base (PlayerID, new Argument()
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
	public CharacterBody.BodyType BodyType;
	public string Pseudo;


	// Constructeur

	/**
	 * @arg if (Isready)
	 *			Hat, Body, Arms -> A utilisé
	 *		else
	 *			Hat, Body, Arms -> A libérer
	 */
	public RequestPlayerReadyInCharacterSelectionEvent(ulong playerID, bool isReady,
		CharacterBody.BodyType body, string pseudo)
		: base(playerID,
			new Argument() { Arg = isReady, Type = typeof(bool) },
			new Argument() { Arg = body, Type = body.GetType() },
			new Argument() { Arg = pseudo, Type = typeof(string) })
	{
		IsReady = isReady;
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

	public CharacterBody.BodyType BodyType;


	// Constructeur

	public InverseStateOfColorEvent(ulong playerID, CharacterBody.BodyType bodyType) : base(playerID,
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

#region ModelMusicResultClient

public class EasyDifficultySelectedEvent : NetworkedEvent
{
	public EasyDifficultySelectedEvent(ulong ClientID) : base(ClientID)
	{
	}
}

public class MediumDifficultySelectedEvent : NetworkedEvent
{
	public MediumDifficultySelectedEvent(ulong ClientID) : base(ClientID)
	{
	}
}

public class HardDifficultySelectedEvent : NetworkedEvent
{
	public HardDifficultySelectedEvent(ulong ClientID) : base(ClientID)
	{
	}
}

#endregion

#region ModelMusicResultServer

public class DifficultyVoteAcceptedEvent : NetworkedEvent
{
	public DifficultyVoteAcceptedEvent(ulong PlayerID) : base(PlayerID)
	{
	}
}

#endregion

#endregion

#endregion

#region ServerState
public class GameStartedEvent : NetworkedEvent
{
}

public class ServerEnterInGameMusicSelectionEvent : NetworkedEvent
{
}

public class ServerEnterInGameMusicResultEvent : NetworkedEvent
{
}

public class ServerEnterInLobbyEvent : NetworkedEvent
{
}

#endregion

#region InRound

public class UpdateSuccessiveSuccessEvent : NetworkedEvent
{
	public BonusStreak Value;
	public enum BonusStreak {
		InvertKanji,
		UncolorKanji,
		FlashKanji,
		InvertInput,
		DisableOtherPlayers,
		ResetAllCombo,
		Shield,
		Default
	}

	public UpdateSuccessiveSuccessEvent(ulong PlayerID, BonusStreak bonus) :
		base(PlayerID, new Argument() { Arg = bonus, Type = typeof(BonusStreak) })
	{
		Value = bonus;
	}
}

#region Malus

public class InvertInputEvent : NetworkedEvent
{
	public float Delai;
	public InvertInputEvent(ulong PlayerID, float delai) : base (PlayerID, new Argument { Arg = delai, Type = typeof(float) })
	{
		Delai = delai;
	}
}

#endregion

#region MobileInputs
public class FireEvent : NetworkedEvent
{
	public FireEvent(ulong playerID) : base(playerID)
	{
	}
}

public class EarthEvent : NetworkedEvent
{
	public EarthEvent(ulong playerID) : base(playerID)
	{
	}
}

public class WaterEvent : NetworkedEvent
{
	public WaterEvent(ulong playerID) : base(playerID)
	{
	}
}

public class PowerEvent : NetworkedEvent
{
	public PowerEvent(ulong playerID) : base(playerID)
	{
	}
}

public class InputListenRequestEvent : NetworkedEvent
{
	[Serializable]
	public enum Input { TILT_LEFT, TILT_RIGHT, TILT_FRONT, TILT_BACK }

	#region Attributes

	public Input Type;
	public float During;
	public float RefreshDelai;

	#endregion

	public InputListenRequestEvent(ulong PlayerID, Input input, float during, float refreshDelai) : base(PlayerID,
		new Argument() { Arg = input, Type = input.GetType() },
		new Argument() { Arg = during, Type = typeof(float) },
		new Argument() { Arg = refreshDelai, Type = typeof(float) })
	{

		Type = input;
		During = during;
		RefreshDelai = refreshDelai;
	}
}

public class InputListenAnswerEvent : NetworkedEvent
{
	public bool Value;

	public InputListenAnswerEvent(ulong PlayerID, bool value) :
		base(PlayerID, new Argument() { Arg = value, Type = typeof(bool) })
	{
		Value = value;
	}
}

#endregion

#endregion
#endregion
using System.Collections.Generic;
using UnityEngine;

#region GameManager Events
// Server state
public class GameMainMenuEvent : SDD.Events.Event
{
}
public class GameRoomMenuEvent : SDD.Events.Event
{
}
public class GameOptionsMenuEvent : SDD.Events.Event
{
}
public class GameCreditsMenuEvent : SDD.Events.Event
{
}
public class GameMusicSelectionMenuEvent : SDD.Events.Event
{
	public IReadOnlyDictionary<ulong, Player> players;
}
public class GameMusicResultMenuEvent : SDD.Events.Event
{
}
public class GamePlayEvent : SDD.Events.Event
{
	// Attributs

	private Dictionary<ulong, Player> Players;
	private AudioClip Clip;


	// Constructeur

	public GamePlayEvent(Dictionary<ulong, Player> list, AudioClip clip)
	{
		if (list == null || clip == null)
		{
			throw new System.Exception();
		}
		else
		{
			Players = new Dictionary<ulong, Player>();

			Dictionary<ulong, Player>.KeyCollection keys = list.Keys;

			foreach (ulong id in keys)
			{
				Players.Add(id, list[id]);
			}

			Clip = clip;
		}
	}


	// Requetes

	public Dictionary<ulong, Player> GetPlayers()
	{
		Dictionary<ulong, Player> tampon = new Dictionary<ulong, Player>();

		Dictionary<ulong, Player>.KeyCollection keys = Players.Keys;

		foreach (ulong id in keys)
		{
			tampon.Add(id, Players[id]);
		}

		return tampon;
	}

	public AudioClip GetMusic()
	{
		return Clip;
	}
}
public class GamePauseEvent : SDD.Events.Event
{
}
public class GameResumeEvent : SDD.Events.Event
{
}
public class GameEndEvent : SDD.Events.Event
{
}
public class GameResultEvent : SDD.Events.Event
{
}
#endregion

#region MenuManager Events
// Server
public class EscapeButtonClickedEvent : SDD.Events.Event
{
}

public class PlayButtonClickedEvent : SDD.Events.Event
{
}

public class OptionsButtonClickedEvent : SDD.Events.Event
{
}

public class CreditsButtonClickedEvent : SDD.Events.Event
{
}

public class QuitButtonClickedEvent : SDD.Events.Event
{
}

public class RoomLeaveButtonClickedEvent : SDD.Events.Event
{
}

public class RoomNextButtonClickedEvent : SDD.Events.Event
{
	public Dictionary<ulong, Player> PlayerList;
}

public class MusicSelectionLeaveButtonClickedEvent : SDD.Events.Event
{
}

public class MusicResultNextButtonClickedEvent : SDD.Events.Event
{
}
#endregion

#region Level Manager

public class GameReadyEvent : SDD.Events.Event
{
}
#region AlgoProcedural Events
public class NextWaveEvent : SDD.Events.Event
{
}

public class CanNotAddObstacle : SDD.Events.Event
{
}

public class ObstacleEndMapEvent : SDD.Events.Event
{
}
#endregion
#endregion

#region Network Server
public class ServerDisconnectionSuccessEvent : SDD.Events.Event
{
	public ulong ClientID;
}
#endregion

#region AccountManager Events
	
public class ProgressBarPrepareSongHaveChangedEvent : SDD.Events.Event
{
	public string State;
	public double Value; // Compris entre 0 et 100%
}

public class ProgressBarPrepareSongErrorEvent : SDD.Events.Event
{
	public string msg;
}

public class PrepareSongEndEvent : SDD.Events.Event
{
}

public class DataSongDeletedEvent : SDD.Events.Event
{
}

#endregion
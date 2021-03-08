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
}
public class GameMusicResultMenuEvent : SDD.Events.Event
{
}
public class GamePlayEvent : SDD.Events.Event
{
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

public class MusicSelectionTimerEndEvent : SDD.Events.Event
{
	public string PathDirectoryMusicSelected;
}

public class MusicResultGameReadyEvent : SDD.Events.Event
{
	public AudioClip audio;
	public List<SpectralFluxInfo> map;
	public float difficulty; // En % de 0 à 1
}
#endregion

#region UI Panel

#region SongListModel

public class SongListModelHasBeenClosedEvent : SDD.Events.Event
{
}

#endregion

#endregion

#region Level Manager

#region AlgoProcedural Events

public class GroundEndMapEvent : SDD.Events.Event
{
}

public class BackgroundEndMapEvent : SDD.Events.Event
{
}

#endregion

#region World

public class RoundStartEvent : SDD.Events.Event
{
	public IReadOnlyCollection<CharacterPlayer> RoundPlayers;
}

public class MusicRoundEndEvent : SDD.Events.Event
{
}

#endregion

#region Player

public class PowerDeclenchementEvent : SDD.Events.Event
{
	public int CmptCombo;
	public CharacterServer CharacterServer;
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

// Add	
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

// Delete
public class DataSongDeletedEvent : SDD.Events.Event
{
}

// Loading
public class UpdateLoadingProgressionAudioClipEvent : SDD.Events.Event
{
	public float progression; // 0f - 1f
}

public class UpdateLoadingMapDataEvent : SDD.Events.Event
{
	public double progression; // 0f- 1f
}

#endregion
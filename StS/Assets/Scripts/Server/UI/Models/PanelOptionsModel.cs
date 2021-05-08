using System.Collections.Generic;
using UnityEngine;

public class PanelOptionsModel : MonoBehaviour
{
	#region Panels
	[Header("Panels")]
	[SerializeField] private GameObject m_PanelSounds;
	[SerializeField] private GameObject m_PanelCredits;
	[SerializeField] private GameObject m_PanelTutorials;

	[Header("SubPanels")]
	[SerializeField] private GameObject m_PanelInGame;
	[SerializeField] private GameObject m_PanelLobby;

	private List<GameObject> m_AllPanels;
	#endregion

	#region Monobehaviour lifecycle
	private void Awake()
	{
		RegisterPanels();
	}
	#endregion

	#region Panel Methods
	private void RegisterPanels()
	{
		m_AllPanels = new List<GameObject>();

		m_AllPanels.Add(m_PanelSounds);
		m_AllPanels.Add(m_PanelCredits);
		m_AllPanels.Add(m_PanelTutorials);
	}

	/// <summary>
	/// Ouvre le panel "panel" et ferme ceux qui ne sont pas la cible.
	/// </summary>
	/// <param name="panel"></param>
	private void OpenPanel(GameObject panel)
	{
		foreach (var item in m_AllPanels)
			if (item) item.SetActive(item == panel);
	}
    #endregion

    #region OnClickButtons

	public void OnClickSoundsButton()
	{
		OpenPanel(m_PanelSounds);
	}

	public void OnClickCreditsButton()
	{
		OpenPanel(m_PanelCredits);
	}

	public void OnClickTutorialButton()
	{
		OpenPanel(m_PanelTutorials);
	}

	public void OnClickToggleSubPanelLobbyButton()
	{
		m_PanelLobby.SetActive(false);
		m_PanelInGame.SetActive(true);
	}

	public void OnClickToggleSubPanelInGameButton()
	{
		m_PanelInGame.SetActive(false);
		m_PanelLobby.SetActive(true);
	}

	#endregion
}

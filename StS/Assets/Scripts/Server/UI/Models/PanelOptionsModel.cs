using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelOptionsModel : MonoBehaviour
{
	#region Panels
	[Header("Panels")]
	[SerializeField] private GameObject m_PanelGraphics;
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

		m_AllPanels.Add(m_PanelGraphics);
		m_AllPanels.Add(m_PanelSounds);
		m_AllPanels.Add(m_PanelCredits);
		m_AllPanels.Add(m_PanelTutorials);
	}

	private void OpenPanel(GameObject panel)
	{
		foreach (var item in m_AllPanels)
			if (item) item.SetActive(item == panel);
	}

	private void OpenAllPanel()
	{
		foreach (var item in m_AllPanels)
		{
			item.SetActive(true);
		}
	}

	private void CloseAllPanel()
	{
		OpenPanel(null);
	}
    #endregion

    #region

	public void OnClickGraphicsButton()
    {
		CloseAllPanel();
		OpenPanel(m_PanelGraphics);
	}

	public void OnClickSoundsButton()
	{
		CloseAllPanel();
		OpenPanel(m_PanelSounds);
	}

	public void OnClickCreditsButton()
	{
		CloseAllPanel();
		OpenPanel(m_PanelCredits);
	}

	public void OnClickTutorialButton()
	{
		CloseAllPanel();
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

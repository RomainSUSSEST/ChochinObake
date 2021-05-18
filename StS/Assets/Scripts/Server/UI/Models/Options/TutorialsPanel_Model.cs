using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialsPanel_Model : MonoBehaviour
{
    #region Attributes

    [SerializeField] private GameObject m_ContentInGame;
    [SerializeField] private GameObject m_ContentLobby;

    [Header("Tutorials")]

    [SerializeField] private List<Sprite> m_InGameTutorials;
    [SerializeField] private List<Sprite> m_InLobbyTutorials;

    [SerializeField] private Image m_InGameTutoImage;
    [SerializeField] private Image m_InLobbyTutoImage;

    private int currentIndex;

    #endregion

    #region Life Cycle

    private void OnEnable()
    {
        LoadLobbyPanel();
    }

    #endregion

    #region OnClick Button

    public void LobbyButtonHasBeenCliked()
    {
        LoadLobbyPanel();
    }

    public void InGameButtonHasBeenClicked()
    {
        LoadInGamePanel();
    }

    public void LobbyTutoIncrementButtonHasBeenClicked()
    {
        currentIndex = (currentIndex + 1) % m_InLobbyTutorials.Count;

        m_InLobbyTutoImage.sprite = m_InLobbyTutorials[currentIndex];
    }

    public void LobbyTutoDecrementButtonHasBeenClicked()
    {
        --currentIndex;
        if (currentIndex < 0)
            currentIndex = m_InLobbyTutorials.Count - 1;

        m_InLobbyTutoImage.sprite = m_InLobbyTutorials[currentIndex];
    }

    public void InGameTutoIncrementButtonHasBeenClicked()
    {
        currentIndex = (currentIndex + 1) % m_InGameTutorials.Count;

        m_InGameTutoImage.sprite = m_InGameTutorials[currentIndex];
    }

    public void InGameTutoDecrementButtonHasBeenClicked()
    {
        --currentIndex;
        if (currentIndex < 0)
            currentIndex = m_InGameTutorials.Count - 1;

        m_InGameTutoImage.sprite = m_InGameTutorials[currentIndex];
    }

    #endregion

    #region Tools

    private void LoadLobbyPanel()
    {
        m_ContentInGame.SetActive(false);
        m_ContentLobby.SetActive(true);

        currentIndex = 0;

        m_InLobbyTutoImage.sprite = m_InLobbyTutorials[currentIndex];
    }

    private void LoadInGamePanel()
    {
        m_ContentInGame.SetActive(true);
        m_ContentLobby.SetActive(false);

        currentIndex = 0;

        m_InGameTutoImage.sprite = m_InGameTutorials[currentIndex];
    }

    #endregion

}

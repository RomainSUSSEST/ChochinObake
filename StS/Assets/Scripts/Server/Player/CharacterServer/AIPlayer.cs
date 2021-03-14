using System.Collections;
using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    #region Constants

    private static readonly int INVERT_INPUT_MALUS = 35;
    private static readonly int FLASH_KANJI_MALUS = 30;
    private static readonly int UNCOLOR_KANJI_MALUS = 25;
    private static readonly int INVERT_KANJI_MALUS = 20;

    #endregion

    #region Attributes

    private CharacterServer AssociatedCharacter;
    private AI_Player AssociatedProfil;

    #region GamePlay

    private int CmptCombo;
    private int CurrentSuccessRate; // en %
    private int UncoloredKanji;

    #endregion

    #endregion

    #region Request

    public AI_Player GetAssociatedProfil()
    {
        return AssociatedProfil;
    }

    #endregion

    #region LifeCycle

    private void Start()
    {
        // Initialisation

        CurrentSuccessRate = (int) AssociatedProfil.Difficulty;
        Debug.Log(CurrentSuccessRate);
    }

    #endregion

    #region Methods

    public void SetAssociatedCharacterServer(CharacterServer c)
    {
        AssociatedCharacter = c;
    }

    public void SetAssociatedAIProfil(AI_Player profil)
    {
        AssociatedProfil = profil;
    }

    #region GamePlay

    public void SetCmptCombo(int combo)
    {
        CmptCombo = combo;
    }

    public void InvertInput(float delai)
    {
        StartCoroutine(_InvertInput(delai));
    }

    public void FlashKanji(float delai)
    {
        StartCoroutine(_FlashKanji(delai));
    }

    public void UncolorKanji(int nbr)
    {
        UncoloredKanji += nbr;
        CurrentSuccessRate -= UNCOLOR_KANJI_MALUS;
    }

    public void SuccessTime(Obstacle.Elements correctElement)
    {
        int randomValue = Random.Range(0, 100);
        if (randomValue <= CurrentSuccessRate)
        {
            if (correctElement == Obstacle.Elements.WATER)
            {
                AssociatedCharacter.AIWater();
            } else if (correctElement == Obstacle.Elements.FIRE)
            {
                AssociatedCharacter.AIFire();
            } else if (correctElement == Obstacle.Elements.EARTH)
            {
                AssociatedCharacter.AIEarth();
            }
        }

        // 1 chance sur 2 de faire un coups mauvais (pour apporter du réalisme)
        else if (Random.Range(0, 100) <= 50)
        {
            if (correctElement != Obstacle.Elements.FIRE)
            {
                AssociatedCharacter.AIFire();
            } else
            {
                AssociatedCharacter.AIWater();
            }
        }

        // traitement du malus uncolored kanji
        if (UncoloredKanji > 0)
        {
            if (--UncoloredKanji == 0)
            {
                CurrentSuccessRate += UNCOLOR_KANJI_MALUS;
            }
        }
    }

    #endregion

    #endregion

    #region Tools

    #region Coroutine

    private IEnumerator _InvertInput(float delai)
    {
        CurrentSuccessRate -= INVERT_INPUT_MALUS;

        yield return new WaitForSeconds(delai);

        CurrentSuccessRate += INVERT_INPUT_MALUS;
    }

    private IEnumerator _FlashKanji(float delai)
    {
        CurrentSuccessRate -= FLASH_KANJI_MALUS;

        yield return new WaitForSeconds(delai);

        CurrentSuccessRate += FLASH_KANJI_MALUS;
    }

    #endregion

    #endregion
}

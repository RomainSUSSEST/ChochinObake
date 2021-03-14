using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    #region Attributes

    private CharacterServer AssociatedCharacter;
    private int AssociatedAIID;
    private System.Random random;

    private int SuccessRate; // Max 100 min 0

    #region GamePlay

    private int CmptCombo;

    #endregion

    #endregion

    #region Request

    public int GetAssociatedAIID()
    {
        return AssociatedAIID;
    }

    #endregion

    #region Methods

    public void SetAssociatedCharacterServer(CharacterServer c, int ID)
    {
        AssociatedCharacter = c;
        AssociatedAIID = ID;

        random = new System.Random(AssociatedAIID); // On donne comme seed, l'index de l'AI
    }

    #region GamePlay

    public void SetCmptCombo(int combo)
    {
        CmptCombo = combo;
    }

    public void InvertInput(float delai)
    {

    }

    public void FlashKanji(float delai)
    {

    }

    public void UncolorKanji(int nbr)
    {

    }

    public void SuccessTime()
    {
        
    }

    #endregion

    #endregion
}

using TMPro;
using UnityEngine;

public class PseudoInputField : MonoBehaviour
{
    // Attributs

    private Animator InvalidPseudoAnimator;
    private TMP_InputField InputField;


    // Life Cycle

    private void Start()
    {
        InvalidPseudoAnimator = GetComponent<Animator>();
        InputField = GetComponent<TMP_InputField>();
    }


    // Requete

    #region Animation Pseudo Input Field
    public void StartInvalidAnimation()
    {
        InvalidPseudoAnimator.SetBool("Start", true);
    }

    public void AnimationInvalidEnd()
    {
        InvalidPseudoAnimator.SetBool("Start", false);
    }
    #endregion

    #region InputField
    public string GetPseudo()
    {
        return InputField.text;
    }
    #endregion
}

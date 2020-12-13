using UnityEngine;

public class InputActionValidArea : MonoBehaviour
{
    // Enum
    public enum InputAction : int { SWIPE_TOP, SWIPE_RIGHT, SWIPE_BOTTOM, SWIPE_LEFT, DOUBLE_PRESS }
    private int InputActionCount = 5;


    #region Attributs

    [Header("Slime Body")]
    [SerializeField] private SlimeBody.BodyType AssociatedBody;

    [Header("Associated InputAction_Obstacle")]

    [SerializeField] private InputAction_Obstacle Top;
    [SerializeField] private InputAction_Obstacle Bottom;
    [SerializeField] private InputAction_Obstacle Right;
    [SerializeField] private InputAction_Obstacle Left;
    [SerializeField] private InputAction_Obstacle DoublePress;

    private InputAction_Obstacle[] InputActions;

    #endregion


    #region Life Cycle

    private void Awake()
    {
        // On initialise le tableau des inputs actions
        InputActions = new InputAction_Obstacle[InputActionCount];

        InputActions[(int) InputAction.DOUBLE_PRESS] = DoublePress;
        InputActions[(int) InputAction.SWIPE_BOTTOM] = Bottom;
        InputActions[(int) InputAction.SWIPE_LEFT] = Left;
        InputActions[(int) InputAction.SWIPE_RIGHT] = Right;
        InputActions[(int) InputAction.SWIPE_TOP] = Top;
    }

    #endregion


    #region Request

    public SlimeBody.BodyType GetAssociatedBody()
    {
        return AssociatedBody;
    }

    public InputAction_Obstacle GetInputAction(InputAction type)
    {
        return InputActions[(int) type];
    }

    #endregion
}

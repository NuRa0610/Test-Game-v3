using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    // Start is called before the first frame update
    public Action<Vector2> OnMoveInput;

    private void Update()
    {
        CheckMovementInput();
        CheckSprintInput();
        CheckJumpInput();
        CheckCrouchInput();
        CheckChangePOVInput();
        CheckClimbInput();
        CheckGlideInput();
        CheckCancelInput();
        CheckAttackInput();
        CheckMainMenuInput();
    }

    // Update is called once per frame
    private void CheckMovementInput()
    {
        // Check for movement input
        float verticalAxis = Input.GetAxis("Vertical");
        float horizontalAxis = Input.GetAxis("Horizontal");

        Vector2 inputAxis = new Vector2(horizontalAxis, verticalAxis);

        if (OnMoveInput != null)
        {
            OnMoveInput(inputAxis);
        }
        Debug.Log("Vertical Axis: " + (verticalAxis));
        Debug.Log("Horizontal Axis: " + (horizontalAxis));
    }

    private void CheckSprintInput()
    {
        bool isHoldSprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (isHoldSprint)
        {
            Debug.Log("Sprinting");
        }

        else
        {
            Debug.Log("Not Sprinting");
        }
    }

    private void CheckJumpInput()
    {
        bool isPressJump = Input.GetKeyDown(KeyCode.Space);

        if (isPressJump)
        {
            Debug.Log("Jump");
        }
    }

    private void CheckCrouchInput()
    {
        bool isHoldCrouch = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (isHoldCrouch)
        {
            Debug.Log("Crouching");
        }
        else
        {
            Debug.Log("Not Crouching");
        }
    }

    private void CheckChangePOVInput()
    {
        bool isPressChangePOV = Input.GetKeyDown(KeyCode.Q);

        if (isPressChangePOV)
        {
            Debug.Log("Change POV");
        }
    }

    private void CheckClimbInput()
    {
        bool isPressClimb = Input.GetKeyDown(KeyCode.E);

        if (isPressClimb)
        {
            Debug.Log("Climb");
        }
    }

    private void CheckGlideInput()
    {
        bool isPressGlide = Input.GetKeyDown(KeyCode.G);

        if (isPressGlide)
        {
            Debug.Log("Gliding");
        }
        /* else
        {
            Debug.Log("Not Gliding");
        } */
    }

    private void CheckCancelInput()
    {
        bool isPressCancel = Input.GetKeyDown(KeyCode.C);

        if (isPressCancel)
        {
            Debug.Log("Cancel Climb/Glide");
        }
    }

    private void CheckAttackInput()
    {
        bool isPressAttack = Input.GetKeyDown(KeyCode.Mouse0); //Input.GetMouseButtonDown(0);

        if (isPressAttack)
        {
            Debug.Log("Attack");
        }
    }

    private void CheckMainMenuInput()
    {
        bool isPressMainMenu = Input.GetKeyDown(KeyCode.Escape);

        if (isPressMainMenu)
        {
            Debug.Log("Open Main Menu");
        }
    }

}

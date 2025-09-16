using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    // Start is called before the first frame update
    public Action<Vector2> OnMoveInput;
    public Action<bool> OnSprintInput;
    public Action OnJumpInput;
    public Action OnClimbInput;
    public Action OnCancelClimb;
    public Action OnChangePOVInput;
    public Action OnCrouchInput;
    public Action OnGlideInput;
    public Action OnCancelGlide;
    public Action OnAttackInput;

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
        //Debug.Log("Vertical Axis: " + (verticalAxis));
        //Debug.Log("Horizontal Axis: " + (horizontalAxis));
    }

    private void CheckSprintInput()
    {
        bool isHoldSprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (isHoldSprint)
        {
            //Debug.Log("Sprinting");
            if (OnSprintInput != null)
            {
                Debug.Log("Sprinting");
                OnSprintInput(true);
            }
        }

        else
        {
            //Debug.Log("Not Sprinting");
            if (OnSprintInput != null)
            {
                OnSprintInput(false);
            }
        }
    }

    private void CheckJumpInput()
    {
        bool isPressJump = Input.GetKeyDown(KeyCode.Space);

        if (isPressJump)
        {
            //Debug.Log("Jump");
            if (OnJumpInput != null)
            {   
                Debug.Log("Jump");
                OnJumpInput();
            }
        }
    }

    private void CheckCrouchInput()
    {
        bool isPressCrouch = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);

        if (isPressCrouch)
        {
            //Debug.Log("Crouching");
            OnCrouchInput();
        }


    }

    private void CheckChangePOVInput()
    {
        bool isPressChangePOV = Input.GetKeyDown(KeyCode.Q);

        if (isPressChangePOV)
        {
            //Debug.Log("Change POV");
            if (OnChangePOVInput != null)
            {
                Debug.Log("Change POV");
                OnChangePOVInput();
            }
        }
    }

    private void CheckClimbInput()
    {
        bool isPressClimb = Input.GetKeyDown(KeyCode.E);

        if (isPressClimb)
        {
            //Debug.Log("Climb");
            OnClimbInput();
        }
    }

    private void CheckGlideInput()
    {
        bool isPressGlide = Input.GetKeyDown(KeyCode.G);

        if (isPressGlide)
        {
            if (OnGlideInput != null)
            {
                Debug.Log("Glide");
                OnGlideInput();
            }
        }        
    }

    private void CheckCancelInput()
    {
        bool isPressCancel = Input.GetKeyDown(KeyCode.C);

        if (isPressCancel)
        {
            if (OnCancelClimb != null)
            {
                Debug.Log("Cancel");
                OnCancelClimb();
            }
            if (OnCancelGlide != null)
            {
                Debug.Log("Cancel Glide");
                OnCancelGlide();
            }
        }
    }

    private void CheckAttackInput()
    {
        bool isPressAttack = Input.GetKeyDown(KeyCode.Mouse0); //Input.GetMouseButtonDown(0);

        if (isPressAttack)
        {
            //Debug.Log("Attack");
            OnAttackInput();
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

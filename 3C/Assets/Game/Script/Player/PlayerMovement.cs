using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private InputManager _input;

    [SerializeField]
    private float _rotationSmoothTime = 0.1f;
    private float _rotationSmoothVelocity;

    [SerializeField] 
    private float _walkSpeed;

    [SerializeField]
    private float _sprintSpeed;

    [SerializeField]
    private float _walkSprintTransition;
    private float _speed;

    [SerializeField]
    private float _jumpForce;

    [SerializeField]
    private Transform _groundCheckDistance;

    [SerializeField]
    private float _groundRadius;

    [SerializeField]
    private LayerMask _groundLayer;
    private bool _isGrounded;

    [SerializeField]
    private Vector3 _upperStepOffset;

    [SerializeField]
    private float _stepCheckDistance;

    [SerializeField]
    private float _stepForce;

    [SerializeField]
    private Transform _climbDetector;

    [SerializeField]
    private float _climbCheckDistance;

    [SerializeField]
    private LayerMask _climbableLayer;

    [SerializeField]
    private Vector3 _climbOffset;

    [SerializeField]
    private float _climbSpeed;

    [SerializeField]
    private float _crouchSpeed;

    [SerializeField]
    private Transform _cameraTransform;

    [SerializeField]
    private CameraManager _cameraManager;

    private PlayerStance _playerStance;

    private Animator _animator;

    private Rigidbody _rigidbody;

    private CapsuleCollider _collider;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();
        _speed = _walkSpeed;
        _input.OnJumpInput += Jump;
        _playerStance = PlayerStance.Stand;
        _animator = GetComponent<Animator>();
        HideAndLockCursor();
    }

    private void HideAndLockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Start()
    {
        _input.OnMoveInput += Move; // subscribe to the event
        _input.OnSprintInput += Sprint;
        //_input.OnJumpInput += Jump;
        _input.OnClimbInput += StartClimb;
        _input.OnCancelClimb += CancelClimb;
        _input.OnCrouchInput += Crouch;
        _cameraManager.OnChangePerspective += ChangePerspective;

    }

    private void Update()
    {
        CheckIsGrounded();
        CheckStepClimb();
    }

    private void OnDestroy()
    {
        _input.OnMoveInput -= Move; // unsubscribe from the event
        _input.OnSprintInput -= Sprint;
        _input.OnJumpInput -= Jump;
        _input.OnClimbInput -= StartClimb;
        _input.OnCancelClimb -= CancelClimb;
        _input.OnCrouchInput -= Crouch;
        _cameraManager.OnChangePerspective -= ChangePerspective;
    }

    private void Move(Vector2 axisDirection)
    {
        Vector3 moveDirection = Vector3.zero;
        bool isPlayerStanding = _playerStance == PlayerStance.Stand;
        bool isPlayerClimbing = _playerStance == PlayerStance.Climb;
        bool isPlayerCrouching = _playerStance == PlayerStance.Crouch;

        if (isPlayerStanding || isPlayerCrouching)
        {
            Vector3 velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
            _animator.SetFloat("Velocity", velocity.magnitude * axisDirection.magnitude);
            _animator.SetFloat("VelocityY", velocity.magnitude * axisDirection.y);
            _animator.SetFloat("VelocityX", velocity.magnitude * axisDirection.x);
            switch (_cameraManager.CameraState)
                {
                    case CameraState.ThirdPerson:
                        if (axisDirection.magnitude >= 0.1)
                        {
                            float targetAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
                            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _rotationSmoothVelocity, _rotationSmoothTime);
                            //smoothen the rotation speed and angle
                            transform.rotation = Quaternion.Euler(0f, angle, 0f);
                            moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                            //Vector3 moveDirection = new Vector3(axisDirection.x, 0, axisDirection.y);
                            //Debug.Log(moveDirection);
                            _rigidbody.AddForce(moveDirection * _speed * Time.deltaTime);
                            //delta time -> based on frame rate
                        }
                        break;
                    case CameraState.FirstPerson:
                        transform.rotation = Quaternion.Euler(0f, _cameraTransform.eulerAngles.y, 0f);
                        Vector3 horizontal = axisDirection.x * transform.right;
                        Vector3 vertical = axisDirection.y * transform.forward;
                        moveDirection = (horizontal + vertical).normalized;
                        _rigidbody.AddForce(moveDirection * _speed * Time.deltaTime);
                        break;
                    default:
                        break;
                }
        } 
        else if (isPlayerClimbing)
        {
            Vector3 horizontal = axisDirection.x * transform.right;
            Vector3 vertical = axisDirection.y * transform.up;
            moveDirection = (horizontal + vertical).normalized;
            _rigidbody.AddForce(moveDirection * _climbSpeed * Time.deltaTime);
        }

        /* else if (isPlayerCrouching)
        {
            Vector3 horizontal = axisDirection.x * transform.right;
            Vector3 vertical = axisDirection.y * transform.forward;
            moveDirection = (horizontal + vertical).normalized;
            _rigidbody.AddForce(moveDirection * _crouchSpeed * Time.deltaTime); 

            Vector3 velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
            _animator.SetFloat("Velocity", velocity.magnitude * axisDirection.magnitude);
        } */

    }

    public void Sprint(bool isSprinting)
    {
        if (isSprinting)
        {
            if (_speed < _sprintSpeed)
            {
                _speed = _speed + _walkSprintTransition * Time.deltaTime;
            }
        }
        else
        {
            if (_speed > _walkSpeed)
            {
                _speed = _speed - _walkSprintTransition * Time.deltaTime;
            }
        }
    }

    public void Jump()
    {
        if (_isGrounded)
        {
            Vector3 jumpDirection = Vector3.up;
            _rigidbody.AddForce(jumpDirection * _jumpForce, ForceMode.Impulse); 
            _animator.SetTrigger("Jump");
            //remove * Time.deltaTime (10.000) cause make the jump inconsistent
        }
    }

    private void CheckIsGrounded()
    {
        _isGrounded = Physics.CheckSphere(_groundCheckDistance.position, _groundRadius, _groundLayer);
        _animator.SetBool("IsGrounded", _isGrounded);
    }

    private void CheckStepClimb()
    {
        bool isHitLower = Physics.Raycast(_groundCheckDistance.position, transform.forward, _stepCheckDistance);
        bool isHitUpper = Physics.Raycast(_groundCheckDistance.position + _upperStepOffset, transform.forward, _stepCheckDistance);

        if (isHitLower && !isHitUpper)
        {
            _rigidbody.AddForce(0, _stepForce, 0);
        }
    }

    private void StartClimb()
    {
        bool isInFrontOfClimbable = Physics.Raycast(_climbDetector.position, transform.forward, out RaycastHit hit, _climbCheckDistance, _climbableLayer);
        bool isNotClimbing = _playerStance != PlayerStance.Climb;

        if (isInFrontOfClimbable && isNotClimbing && _isGrounded)
        {
            Vector3 offset = (transform.forward * _climbOffset.z) + (Vector3.up * _climbOffset.y);
            transform.position = hit.point - offset;
            _playerStance = PlayerStance.Climb;
            _rigidbody.useGravity = false;
            _cameraManager.SetFPSClampedCamera(true, transform.rotation.eulerAngles);
            _cameraManager.SetTPSFieldOfView(70);

        }
    }

    public void CancelClimb()
    {
        //bool isClimbing = _playerStance == PlayerStance.Climb;

        if (_playerStance == PlayerStance.Climb)
        {
            _playerStance = PlayerStance.Stand;
            _rigidbody.useGravity = true;
            transform.position -= transform.forward * 1f;
            _cameraManager.SetFPSClampedCamera(false, transform.rotation.eulerAngles);
            _cameraManager.SetTPSFieldOfView(40);
        }
    }

    private void Crouch()
    {
        if (_playerStance == PlayerStance.Stand)
        {
            _playerStance = PlayerStance.Crouch;
            _animator.SetBool("IsCrouch", true);
            _speed = _crouchSpeed;
            _collider.height = 1.3f;
            _collider.center = Vector3.up * 0.65f;
        }
        else if (_playerStance == PlayerStance.Crouch)
        {
            _playerStance = PlayerStance.Stand;
            _animator.SetBool("IsCrouch", false);
            _speed = _walkSpeed;
            _collider.height = 2f;
            _collider.center = Vector3.up * 1f;
        }
    }

    private void ChangePerspective()
    {
        _animator.SetTrigger("ChangePerspective");
    }
}

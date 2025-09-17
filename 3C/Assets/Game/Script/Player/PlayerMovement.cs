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
    private float _glideSpeed;

    [SerializeField]
    private Vector3 _glideRotationSpeed;

    [SerializeField]
    private float _minGlideRotationX;

    [SerializeField]
    private float _maxGlideRotationX;

    [SerializeField]
    private float _airDrag;

    [SerializeField]
    private Transform _cameraTransform;

    [SerializeField]
    private CameraManager _cameraManager;

    [SerializeField]
    private float _resetComboInterval;
    private Coroutine _resetCombo;

    [SerializeField]
    private Transform _hitDetector;

    [SerializeField]
    private float _hitDetectorRadius;

    [SerializeField]
    private LayerMask _hitLayer;

    [SerializeField]
    private PlayerAudioManager _playerAudioManager;

    private PlayerStance _playerStance;

    private Animator _animator;

    private Rigidbody _rigidbody;

    private CapsuleCollider _collider;

    private bool _isPunching;
    private int _combo = 0;

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
        _input.OnGlideInput += StartGlide;
        _input.OnCancelGlide += CancelGlide;
        _input.OnAttackInput += Punch;
        _cameraManager.OnChangePerspective += ChangePerspective;

    }

    private void Update()
    {
        CheckIsGrounded();
        CheckStepClimb();
        Glide();
    }

    private void OnDestroy()
    {
        _input.OnMoveInput -= Move; // unsubscribe from the event
        _input.OnSprintInput -= Sprint;
        _input.OnJumpInput -= Jump;
        _input.OnClimbInput -= StartClimb;
        _input.OnCancelClimb -= CancelClimb;
        _input.OnCrouchInput -= Crouch;
        _input.OnGlideInput -= StartGlide;
        _input.OnCancelGlide -= CancelGlide;
        _input.OnAttackInput -= Punch;
        _cameraManager.OnChangePerspective -= ChangePerspective;
    }

    private void Move(Vector2 axisDirection)
    {
        Vector3 moveDirection = Vector3.zero;
        bool isPlayerStanding = _playerStance == PlayerStance.Stand;
        bool isPlayerClimbing = _playerStance == PlayerStance.Climb;
        bool isPlayerCrouching = _playerStance == PlayerStance.Crouch;
        bool isPlayerGliding = _playerStance == PlayerStance.Glide;

        if ((isPlayerStanding || isPlayerCrouching) && !_isPunching)
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

            Vector3 velocity = new Vector3(_rigidbody.velocity.x, _rigidbody.velocity.y, 0);
            _animator.SetFloat("ClimbVelocityY", velocity.magnitude * axisDirection.y);
            _animator.SetFloat("ClimbVelocityX", velocity.magnitude * axisDirection.x);
        }
        else if (isPlayerGliding)
        {
            Vector3 rotationDegree = transform.rotation.eulerAngles;
            rotationDegree.x += _glideRotationSpeed.x * axisDirection.y * Time.deltaTime;
            rotationDegree.x += Mathf.Clamp(rotationDegree.x, _minGlideRotationX, _maxGlideRotationX);
            rotationDegree.y += _glideRotationSpeed.y * axisDirection.x * Time.deltaTime;
            rotationDegree.z += _glideRotationSpeed.z * axisDirection.x * Time.deltaTime;
            transform.rotation = Quaternion.Euler(rotationDegree);
        }        
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
        if (_isGrounded)
        {
            CancelGlide();
        }
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
            _animator.SetBool("IsClimbing", true);
            _rigidbody.useGravity = false;
            _cameraManager.SetFPSClampedCamera(true, transform.rotation.eulerAngles);
            _cameraManager.SetTPSFieldOfView(70);
            _collider.center = Vector3.up * 1.3f;

        }
    }

    public void CancelClimb()
    {
        //bool isClimbing = _playerStance == PlayerStance.Climb;

        if (_playerStance == PlayerStance.Climb)
        {
            _playerStance = PlayerStance.Stand;
            _animator.SetBool("IsClimbing", false);
            _rigidbody.useGravity = true;
            transform.position -= transform.forward * 1f;
            _cameraManager.SetFPSClampedCamera(false, transform.rotation.eulerAngles);
            _cameraManager.SetTPSFieldOfView(40);
            _collider.center = Vector3.up * 0.9f;
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

    private void StartGlide()
    {
        if(_playerStance != PlayerStance.Glide && !_isGrounded)
        {
            _playerStance = PlayerStance.Glide;
            _animator.SetBool("IsGliding", true);
            _cameraManager.SetFPSClampedCamera(true, transform.rotation.eulerAngles);
            _playerAudioManager.PlayGlideSfx();
        }
    }

    private void CancelGlide()
    {
        if(_playerStance == PlayerStance.Glide)
        {
            _playerStance = PlayerStance.Stand;
            _animator.SetBool("IsGliding", false);
            _cameraManager.SetFPSClampedCamera(false, transform.rotation.eulerAngles);
            _playerAudioManager.StopGlideSfx();
        }
    }

    private void Glide()
    {
        if(_playerStance == PlayerStance.Glide)
        {
            Vector3 playerRotation = transform.rotation.eulerAngles;
            float lift = playerRotation.x;
            Vector3 upForce = transform.up * (lift + _airDrag);
            Vector3 forwardForce = transform.forward * _glideSpeed;
            Vector3 totalForce = upForce + forwardForce;
            _rigidbody.AddForce(totalForce * Time.deltaTime);
        }
    }

    private void Punch()
    {
        if (!_isPunching && _playerStance == PlayerStance.Stand)
        {
            _isPunching = true;
            if (_combo < 3)
            {
                _combo = _combo + 1;
            }
            else
            {
                _combo = 1;
            }
            _animator.SetInteger("Combo", _combo);
            _animator.SetTrigger("Punch");
        }
    }

    private void EndPunch()
    {
        _isPunching = false;
        if (_resetCombo != null)
        {
            StopCoroutine(_resetCombo);
        }
        _resetCombo = StartCoroutine(ResetCombo());
    }

    private IEnumerator ResetCombo()
    {
        yield return new WaitForSeconds(_resetComboInterval);
        _combo = 0;
    }

    private void Hit()
    {
        Collider[] hitObjects = Physics.OverlapSphere(_hitDetector.position, _hitDetectorRadius, _hitLayer);
        for (int i = 0; i < hitObjects.Length; i++)
        {
            if (hitObjects[i].gameObject != null)
            {
                Destroy(hitObjects[i].gameObject);
                //Debug.Log("Hit " + hitObjects[i].name);
            }
        }
    }

    private void ChangePerspective()
    {
        _animator.SetTrigger("ChangePerspective");
    }
}

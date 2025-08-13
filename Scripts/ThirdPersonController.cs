using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace GameScript.Scripts
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")] [Tooltip("Move speed of the character in m/s")]
        public float moveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float sprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")] [Range(0.0f, 0.3f)]
        public float rotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float speedChangeRate = 10.0f;

        public AudioClip landingAudioClip;
        public AudioClip[] footstepAudioClips;
        [Range(0, 1)] public float footstepAudioVolume = 0.5f;

        [Space(10)] [Tooltip("The height the player can jump")]
        public float jumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float jumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float fallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool grounded = true;

        [Tooltip("Useful for rough ground")] public float groundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float groundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask groundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject cinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float topClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float bottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float cameraAngleOverride;

        [Tooltip("For locking the camera position on all axis")]
        public bool lockCameraPosition;

        private const float JumpGraceTime = 0.2f; // ジャンプ中にgrounded無視する時間
        private const float JumpDelay = 0.133f; // 8フレーム ≒ 約0.13秒

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        private bool _jumpTriggered;
        private float _jumpDelayTimer;
        private float _jumpGraceTimer;

        private float _verticalVelocity;
        private const float TerminalVelocity = 53.0f;
        

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDCombat;
        private int _animIDAttack;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private PlayerInputsManager _inputManager;
        private GameObject _mainCamera;

        private const float Threshold = 0.01f;

        private bool _hasAnimator;
        private bool _isCombat;
        private bool _isSprinting;

        private MovementHandler _movementHandler;
        private JumpHandler _jumpHandler;
        private GuardHandler _guardHandler;
        private AttackHandler _attackHandler;
        private DodgeHandler _dodgeHandler;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = cinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _inputManager = GetComponent<PlayerInputsManager>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            _movementHandler = new MovementHandler(_controller, _inputManager, transform, _mainCamera,
                rotationSmoothTime, speedChangeRate, moveSpeed, sprintSpeed);
            _jumpHandler = new JumpHandler(_inputManager, fallTimeout, JumpDelay, jumpHeight, gravity, JumpGraceTime,
                jumpTimeout, TerminalVelocity);
            _guardHandler = new GuardHandler(_inputManager);
            _attackHandler = new AttackHandler(_inputManager);
            _dodgeHandler = new DodgeHandler(_inputManager);
            
            AssignAnimationIDs();
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);
            var verticalVelocity = _jumpHandler.UpdateJump(grounded,_animator,_hasAnimator);
            GroundedCheck();
            var horizontalMove = _movementHandler.UpdateMovement(_animator, _hasAnimator);
            _controller.Move(horizontalMove.normalized * (_movementHandler.Speed * Time.deltaTime) +
                             new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);
            _guardHandler.UpdateGuardState(_animator, _hasAnimator);
            _attackHandler.UpdateAttackState(_animator, _hasAnimator);
            _dodgeHandler.UpdateDodgeState(_animator, _hasAnimator);
            Combat();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _movementHandler.AnimIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _jumpHandler.AnimIDJump = Animator.StringToHash("Jump");
            _jumpHandler.AnimIDFreeFall = Animator.StringToHash("FreeFall");
            _movementHandler.AnimIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDCombat = Animator.StringToHash("Combat");
            _guardHandler.AnimIDGuard = Animator.StringToHash("Guard");
            _attackHandler.AnimIDAttack = Animator.StringToHash("Attack");
            _attackHandler.AnimIDCombat = Animator.StringToHash("Combat");
            _animIDAttack = Animator.StringToHash("Attack");
            _dodgeHandler.AnimIDDodge = Animator.StringToHash("Dodge");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset,
                transform.position.z);
            grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_inputManager.look.sqrMagnitude >= Threshold && !lockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _inputManager.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _inputManager.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);

            // Cinemachine will follow this target
            cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + cameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }
        
        private void Combat()
        {
            if (!_hasAnimator)
            {
                return;
            }

            if (_inputManager.combat)
            {
                if (!_animator.GetBool(_animIDCombat))
                {
                    _animator.SetBool(_animIDCombat, true);
                    _animator.ResetTrigger(_animIDAttack);
                    _inputManager.attack = false;
                    _inputManager.noncombat = false;
                }
            }

            if (_inputManager.noncombat)
            {
                if (_animator.GetBool(_animIDCombat))
                {
                    _animator.SetBool(_animIDCombat, false);
                    _inputManager.combat = false;
                }
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = grounded ? transparentGreen : transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z),
                groundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (footstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, footstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(footstepAudioClips[index], transform.TransformPoint(_controller.center),
                        footstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(landingAudioClip, transform.TransformPoint(_controller.center),
                    footstepAudioVolume);
            }
        }
    }
}
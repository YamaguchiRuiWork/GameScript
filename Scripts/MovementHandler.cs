using UnityEngine;

namespace GameScript.Scripts
{
    public class MovementHandler
    {
        private readonly CharacterController _controller;
        private readonly PlayerInputsManager _inputsManagerManager;
        private readonly Transform _transform;
        private readonly GameObject _mainCamera;

        public float Speed { get; private set; }
        private float _animationBlend;
        private readonly float _rotationSmoothTime ;
        private readonly float _speedChangeRate;
        private readonly float _moveSpeed;
        private readonly float _sprintSpeed;
        public int AnimIDSpeed { get; set; }
        public int AnimIDMotionSpeed { get; set; }

        private float _targetRotation;
        private float _rotationVelocity;

        private bool _isSprinting;

        public MovementHandler(CharacterController controller, PlayerInputsManager inputsManager, Transform transform,
            GameObject mainCamera, float rotationSmoothTime, float speedChangeRate, float moveSpeed, float sprintSpeed)
        {
            _controller = controller;
            _inputsManagerManager = inputsManager;
            _transform = transform;
            _mainCamera = mainCamera;
            _rotationSmoothTime = rotationSmoothTime;
            _speedChangeRate = speedChangeRate;
            _moveSpeed = moveSpeed;
            _sprintSpeed = sprintSpeed;
        }

        public Vector3 UpdateMovement(Animator animator, bool hasAnimator)
        {
            if (_inputsManagerManager.sprint)
            {
                _isSprinting = !_isSprinting;
                _inputsManagerManager.sprint = false;
            }

            var targetSpeed = _isSprinting ? _sprintSpeed : _moveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

           
            // if there is no input, set the target speed to 0
            if (_inputsManagerManager.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _inputsManagerManager.analogMovement ? _inputsManagerManager.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                Speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * _speedChangeRate);

                // round speed to 3 decimal places
                Speed = Mathf.Round(Speed * 1000f) / 1000f;
            }
            else
            {
                Speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * _speedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_inputsManagerManager.move.x, 0.0f, _inputsManagerManager.move.y)
                .normalized;
            
            // if there is a move input rotate player when the player is moving
            if (_inputsManagerManager.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(_transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    _rotationSmoothTime);

                // rotate to face input direction relative to camera position
                _transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            var targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            // _controller.Move(targetDirection.normalized * (Speed * Time.deltaTime) +
            //                  new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (hasAnimator)
            {
                animator.SetFloat(AnimIDSpeed, _animationBlend);
                animator.SetFloat(AnimIDMotionSpeed, inputMagnitude);
            }

            return targetDirection;
        }
    }
}
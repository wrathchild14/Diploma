﻿using UnityEngine;

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")] [Tooltip("Move speed of the character in m/s")]
        public float moveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float sprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")] [Range(0.0f, 0.3f)]
        public float rotationSmoothTime = 0.12f;

        public float speedChangeRate = 10.0f;

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

        private float _animationBlend;
        private Animator _animator;
        private int _animIDFreeFall;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDMotionSpeed;

        // animation IDs
        private int _animIDSpeed;
        private CharacterController _controller;
        private float _fallTimeoutDelta;

        private bool _hasAnimator;
        private bool _jumpBool = false;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private GameObject _mainCamera;

        private float _rotationVelocity;

        // [Header("Cinemachine")]
        // [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        // public GameObject CinemachineCameraTarget;
        //
        // [Tooltip("How far in degrees can you move the camera up")]
        // public float TopClamp = 70.0f;
        //
        // [Tooltip("How far in degrees can you move the camera down")]
        // public float BottomClamp = -30.0f;
        //
        // [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        // public float CameraAngleOverride = 0.0f;
        //
        // [Tooltip("For locking the camera position on all axis")]
        // public bool LockCameraPosition = false;
        //
        // // cinemachine
        // private float _cinemachineTargetYaw;
        // private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _targetRotation;
        private const float TerminalVelocity = 53.0f;
        private float _verticalVelocity;

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
            // _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = jumpTimeout;
            _fallTimeoutDelta = fallTimeout;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        private void OnDrawGizmosSelected()
        {
            var transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            var transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = grounded ? transparentGreen : transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            var position = transform.position;
            Gizmos.DrawSphere(
                new Vector3(position.x, position.y - groundedOffset, position.z),
                groundedRadius);
        }

        // private void LateUpdate()
        // {
        //     CameraRotation();
        // }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            var position = transform.position;
            var spherePosition = new Vector3(position.x, position.y - groundedOffset,
                position.z);
            grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator) _animator.SetBool(_animIDGrounded, grounded);
        }

        // private void CameraRotation()
        // {
        // if there is an input and camera position is not fixed
        // if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        // {
        //     //Don't multiply mouse input by Time.deltaTime;
        //     float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
        //
        //     _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
        //     _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
        // }

        // clamp our rotations so our values are limited 360 degrees
        // _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        // _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        // CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
        //     _cinemachineTargetYaw, 0.0f);
        // }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            var targetSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;

            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (input == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            var velocity = _controller.velocity;
            var currentHorizontalSpeed = new Vector3(velocity.x, 0.0f, velocity.z).magnitude;

            const float speedOffset = 0.1f;
            var inputMagnitude = input.magnitude;
            // float inputMagnitude = input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * speedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            var inputDirection = new Vector3(input.x, 0.0f, input.y).normalized;
            // Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (input != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                var rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    rotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            var targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = fallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f) _verticalVelocity = -2f;

                // Jump
                _jumpBool = Input.GetKey(KeyCode.Space);
                if (_jumpBool && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                    // update animator if using character
                    if (_hasAnimator) _animator.SetBool(_animIDJump, true);
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f) _jumpTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = jumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator) _animator.SetBool(_animIDFreeFall, true);
                }

                // if we are not grounded, do not jump
                _jumpBool = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < TerminalVelocity) _verticalVelocity += gravity * Time.deltaTime;
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
        
        private void OnFootstep(AnimationEvent animationEvent)
        {
        }

        private void OnLand(AnimationEvent animationEvent)
        {
        }
    }
}
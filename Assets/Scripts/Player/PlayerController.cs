using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace AstroSurvive.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Speeds")]
        [Tooltip("Standard walking speed in m/s")]
        public float walkSpeed = 4.5f;

        [Tooltip("Running/Sprinting speed in m/s")]
        public float runSpeed = 8.5f;

        [Tooltip("Crouching movement speed in m/s")]
        public float crouchSpeed = 2.4f;

        [Tooltip("Acceleration factor when changing movement direction/speed")]
        public float acceleration = 12f;

        [Header("Mars Physics")]
        [Tooltip("Physical Mars surface gravity (m/s^2)")]
        public float gravity = -3.72f;

        [Tooltip("Jump apex height on Mars")]
        public float jumpHeight = 1.35f;

        [Tooltip("Snappy fall multiplier to eliminate floaty moon jumps")]
        public float fallMultiplier = 2.4f;

        [Header("Crouch Configuration")]
        public float standingHeight = 2.0f;
        public float crouchingHeight = 1.1f;
        public float standingCamY = 1.65f;
        public float crouchingCamY = 0.9f;
        public float crouchTransitionSpeed = 9f;

        [Header("Mouse Look")]
        public float mouseSensitivity = 1.8f;
        public float minPitch = -85f;
        public float maxPitch = 85f;

        [Header("Head Bobbing")]
        public bool enableHeadBob = true;
        public float bobFrequency = 2.2f;
        public float bobAmount = 0.045f;

        [Header("Survival / Telemetry")]
        public float maxOxygen = 100f;
        public float oxygenDepletionRate = 0.45f; // per second in harsh Mars atmosphere
        public float currentOxygen { get; private set; }

        public float maxStamina = 100f;
        public float staminaDrainRate = 18f;
        public float staminaRecoveryRate = 14f;
        public float currentStamina { get; private set; }

        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController characterController;
        private Vector3 moveVelocity;
        private float verticalVelocity;
        private float pitch;
        private float currentSpeed;
        private bool isCrouching;
        private bool isRunning;
        private bool isGrounded;
        private float bobTimer;
        private Vector3 initialCamPos;

        public static bool InputLocked = false;

        public bool IsCrouching => isCrouching;
        public bool IsRunning => isRunning;
        public bool IsGrounded => isGrounded;
        public float CurrentSpeed => currentSpeed;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            
            if (cameraTransform == null)
            {
                var cam = GetComponentInChildren<Camera>();
                if (cam != null)
                {
                    cameraTransform = cam.transform;
                }
            }

            if (cameraTransform != null)
            {
                initialCamPos = cameraTransform.localPosition;
            }

            currentOxygen = maxOxygen;
            currentStamina = maxStamina;
            currentSpeed = walkSpeed;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public bool IsDead => currentOxygen <= 0f;

        private void Update()
        {
            // Oxygen Death: When O2 is 0%, freeze player entirely to simulate Game Over
            if (currentOxygen <= 0f)
            {
                currentSpeed = 0f;
                moveVelocity = Vector3.zero;
                isRunning = false;
                if (!characterController.isGrounded)
                {
                    verticalVelocity += gravity * fallMultiplier * Time.deltaTime;
                    characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
                }
                return;
            }

            HandleCursorToggle();
            HandleMouseLook();
            HandleSurvivalStats();
            HandleCrouch();
            HandleMovement();
            HandleHeadBob();
        }

        private void HandleCursorToggle()
        {
            if (InputLocked) return;
            bool escapePressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
                escapePressed = Keyboard.current.escapeKey.wasPressedThisFrame;
#else
            escapePressed = Input.GetKeyDown(KeyCode.Escape);
#endif
            if (escapePressed)
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }

        private void HandleMouseLook()
        {
            if (currentOxygen <= 0f || InputLocked || Cursor.lockState != CursorLockMode.Locked) return;

            Vector2 mouseDelta = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                mouseDelta = Mouse.current.delta.ReadValue() * 0.1f * mouseSensitivity;
            }
#else
            mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseSensitivity;
#endif

            // Horizontal rotation (turns player body)
            transform.Rotate(Vector3.up * mouseDelta.x);

            // Vertical rotation (pitches camera)
            if (cameraTransform != null)
            {
                pitch -= mouseDelta.y;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
                cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
            }
        }

        private void HandleSurvivalStats()
        {
            // Oxygen consumption
            if (currentOxygen > 0f)
            {
                currentOxygen = Mathf.Max(0f, currentOxygen - (oxygenDepletionRate * Time.deltaTime));
            }

            // Stamina mechanics
            if (isRunning && currentSpeed > walkSpeed + 0.5f)
            {
                currentStamina = Mathf.Max(0f, currentStamina - (staminaDrainRate * Time.deltaTime));
                if (currentStamina <= 0f)
                {
                    // Explicitly force sprint false and return to walk speed instantly
                    isRunning = false;
                    currentSpeed = walkSpeed;
                }
            }
            else
            {
                currentStamina = Mathf.Min(maxStamina, currentStamina + (staminaRecoveryRate * Time.deltaTime));
            }
        }

        private void HandleCrouch()
        {
            bool crouchInput = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                crouchInput = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.cKey.isPressed;
            }
#else
            crouchInput = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
#endif

            // If wanting to stand up, verify there is no obstacle directly above
            if (!crouchInput && isCrouching)
            {
                Vector3 sphereCastOrigin = transform.position + Vector3.up * crouchingHeight;
                float sphereCastDist = standingHeight - crouchingHeight;
                if (Physics.SphereCast(sphereCastOrigin, characterController.radius * 0.9f, Vector3.up, out _, sphereCastDist))
                {
                    crouchInput = true; // Obstructed overhead, forced to stay crouched
                }
            }

            isCrouching = crouchInput;

            // Smoothly adjust character controller height and center
            float targetHeight = isCrouching ? crouchingHeight : standingHeight;
            characterController.height = Mathf.Lerp(characterController.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
            characterController.center = new Vector3(0f, characterController.height * 0.5f, 0f);

            // Smoothly adjust camera Y position
            if (cameraTransform != null)
            {
                float targetCamY = isCrouching ? crouchingCamY : standingCamY;
                Vector3 currentCamPos = cameraTransform.localPosition;
                currentCamPos.y = Mathf.Lerp(currentCamPos.y, targetCamY, Time.deltaTime * crouchTransitionSpeed);
                cameraTransform.localPosition = currentCamPos;
            }
        }

        private void HandleMovement()
        {
            isGrounded = characterController.isGrounded;
            if (isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f; // Slight downward force to maintain ground contact
            }

            // Read directional input
            Vector2 inputDir = Vector2.zero;
            bool sprintKey = false;
            bool jumpKey = false;

            if (InputLocked)
            {
                currentSpeed = Mathf.Lerp(currentSpeed, 0f, Time.deltaTime * acceleration);
                moveVelocity = Vector3.Lerp(moveVelocity, Vector3.zero, Time.deltaTime * acceleration);
                verticalVelocity += gravity * Time.deltaTime;
                characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
                return;
            }

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) inputDir.y += 1f;
                if (Keyboard.current.sKey.isPressed) inputDir.y -= 1f;
                if (Keyboard.current.dKey.isPressed) inputDir.x += 1f;
                if (Keyboard.current.aKey.isPressed) inputDir.x -= 1f;

                sprintKey = Keyboard.current.leftShiftKey.isPressed;
                jumpKey = Keyboard.current.spaceKey.wasPressedThisFrame;
            }
#else
            inputDir.x = Input.GetAxisRaw("Horizontal");
            inputDir.y = Input.GetAxisRaw("Vertical");
            sprintKey = Input.GetKey(KeyCode.LeftShift);
            jumpKey = Input.GetKeyDown(KeyCode.Space);
#endif

            // Guard: only normalize if there is actual input (prevents zero-vector IsNormalized assertion)
            if (inputDir.sqrMagnitude > 0.01f)
                inputDir.Normalize();

            // Determine target speed based on state
            float targetSpeed = walkSpeed;
            if (isCrouching)
            {
                targetSpeed = crouchSpeed;
                isRunning = false;
            }
            else if (currentStamina <= 0f)
            {
                // Force walk speed instantly when stamina is 0
                targetSpeed = walkSpeed;
                isRunning = false;
                currentSpeed = Mathf.Min(currentSpeed, walkSpeed);
            }
            else if (sprintKey && inputDir.y > 0.1f && currentStamina > 8f)
            {
                targetSpeed = runSpeed;
                isRunning = true;
            }
            else
            {
                isRunning = false;
            }

            if (inputDir.magnitude < 0.1f)
            {
                targetSpeed = 0f;
                isRunning = false;
            }

            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * acceleration);

            // Direction relative to player rotation (guard zero input to avoid zero-vector normalization)
            Vector3 targetMoveDir = Vector3.zero;
            if (inputDir.sqrMagnitude > 0.001f)
                targetMoveDir = (transform.right * inputDir.x + transform.forward * inputDir.y).normalized;
            moveVelocity = Vector3.Lerp(moveVelocity, targetMoveDir * currentSpeed, Time.deltaTime * acceleration);

            // Snappy Mars Jumping
            if (jumpKey && isGrounded && !isCrouching && currentOxygen > 0f)
            {
                // v = sqrt(2 * g * h)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Snappy Mars gravity: normal on ascent, accelerated on descent to avoid floaty moon jump
            if (verticalVelocity < 0f && !isGrounded)
            {
                verticalVelocity += gravity * fallMultiplier * Time.deltaTime;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            // Combine horizontal and vertical motion
            Vector3 finalMotion = moveVelocity + Vector3.up * verticalVelocity;
            characterController.Move(finalMotion * Time.deltaTime);
        }

        private void HandleHeadBob()
        {
            if (!enableHeadBob || cameraTransform == null) return;

            Vector3 camLocalPos = cameraTransform.localPosition;
            float currentBaseY = isCrouching ? crouchingCamY : standingCamY;

            if (isGrounded && moveVelocity.magnitude > 0.2f)
            {
                float speedMultiplier = isRunning ? 1.4f : (isCrouching ? 0.7f : 1f);
                bobTimer += Time.deltaTime * bobFrequency * speedMultiplier;
                float bobOffset = Mathf.Sin(bobTimer) * bobAmount * (isRunning ? 1.3f : 1f);
                camLocalPos.y = Mathf.Lerp(camLocalPos.y, currentBaseY + bobOffset, Time.deltaTime * 10f);
            }
            else
            {
                bobTimer = 0f;
                camLocalPos.y = Mathf.Lerp(camLocalPos.y, currentBaseY, Time.deltaTime * 8f);
            }

            cameraTransform.localPosition = camLocalPos;
        }

        public void RefillOxygen(float amount)
        {
            currentOxygen = Mathf.Min(maxOxygen, currentOxygen + amount);
        }

        public void ModifyOxygen(float amount)
        {
            currentOxygen = Mathf.Clamp(currentOxygen + amount, 0f, maxOxygen);
        }

        public void RestoreStamina(float amount)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
        }
    }
}

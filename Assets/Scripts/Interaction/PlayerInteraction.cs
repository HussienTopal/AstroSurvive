using UnityEngine;
using AstroSurvive.Player;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace AstroSurvive.Interaction
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Raycast Config")]
        [Tooltip("Max distance the player can interact with objects in meters")]
        public float interactionDistance = 3.5f;

        [Tooltip("Layers to check for interactables")]
        public LayerMask interactionLayers = ~0;

        [Header("References")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private PlayerController playerController;

        private IInteractable currentTarget;

        public IInteractable CurrentTarget => currentTarget;
        public string CurrentPrompt => currentTarget?.GetInteractionPrompt() ?? string.Empty;

        private void Awake()
        {
            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }

            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
                if (playerCamera == null) playerCamera = Camera.main;
            }
        }

        private void Update()
        {
            CheckForInteractable();
            HandleInteractionInput();
        }

        private void CheckForInteractable()
        {
            if (playerCamera == null) return;

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayers, QueryTriggerInteraction.Collide))
            {
                var interactable = hit.collider.GetComponentInParent<IInteractable>();
                currentTarget = interactable;
            }
            else
            {
                currentTarget = null;
            }
        }

        private void HandleInteractionInput()
        {
            if (currentTarget == null) return;

            bool interactPressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                interactPressed = Keyboard.current.eKey.wasPressedThisFrame;
            }
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                interactPressed = true;
            }
#else
            interactPressed = Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0);
#endif

            if (interactPressed)
            {
                currentTarget.Interact(playerController);
            }
        }
    }
}

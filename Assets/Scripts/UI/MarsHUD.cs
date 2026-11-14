using UnityEngine;
using UnityEngine.UI;
using AstroSurvive.Player;
using AstroSurvive.Interaction;
using AstroSurvive.Core;

namespace AstroSurvive.UI
{
    public class MarsHUD : MonoBehaviour
    {
        public static MarsHUD Instance { get; private set; }

        [Header("Canvas UI References (Visually Editable in Hierarchy)")]
        [Tooltip("Standard UI Text displaying current Sol, e.g. 'SOL 1 / 5'")]
        public Text solNumberText;

        [Tooltip("Standard UI Text displaying Outpost Power, e.g. 'POWER: 100%'")]
        public Text powerText;

        [Tooltip("Standard UI Text displaying Radiation Shield, e.g. 'SHIELD: 100%'")]
        public Text shieldText;

        [Tooltip("Standard UI Text displaying Suit Oxygen, e.g. 'SUIT O2: 98%'")]
        public Text oxygenText;

        [Tooltip("Standard UI Text displaying Suit Stamina, e.g. 'STAMINA: 100%'")]
        public Text staminaText;

        [Tooltip("Standard UI Text displaying Suit Movement Mode, e.g. '[WALKING]'")]
        public Text suitModeText;

        [Tooltip("Standard UI Text displaying NASA space weather warning and educational note")]
        public Text nasaWarningText;

        [Tooltip("Standard UI Text displaying interaction prompts like '[E] Pull Lever'")]
        public Text interactionPromptText;

        [Tooltip("Standard UI Text for general notifications")]
        public Text notificationText;

        [Header("Optional UI Sliders (Visually Editable in Hierarchy)")]
        public Slider powerSlider;
        public Slider shieldSlider;
        public Slider oxygenSlider;
        public Slider staminaSlider;

        [Header("Core References")]
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerInteraction interaction;
        [SerializeField] private SolGameManager solGameManager;

        private static string activeNotification = "";
        private static float notificationTimer = 0f;

        private static string activeNasaWarning = "";
        private static float nasaWarningTimer = 0f;

        public static void ShowNotification(string message, float duration = 4.5f)
        {
            activeNotification = message;
            notificationTimer = duration;
        }

        public static void ShowNasaWarning(string message, float duration = 5.0f)
        {
            activeNasaWarning = message;
            nasaWarningTimer = duration;
        }

        private void Awake()
        {
            Instance = this;
            if (player == null) player = FindFirstObjectByType<PlayerController>();
            if (interaction == null) interaction = FindFirstObjectByType<PlayerInteraction>();
            if (solGameManager == null) solGameManager = FindFirstObjectByType<SolGameManager>();
        }

        private void Update()
        {
            UpdateTimers();
            UpdateTelemetryUI();
            UpdateInteractionUI();
            UpdateNasaWarningUI();
            UpdateNotificationUI();
        }

        private void UpdateTimers()
        {
            if (notificationTimer > 0f)
            {
                notificationTimer -= Time.deltaTime;
                if (notificationTimer <= 0f) activeNotification = "";
            }

            if (nasaWarningTimer > 0f)
            {
                nasaWarningTimer -= Time.deltaTime;
                if (nasaWarningTimer <= 0f) activeNasaWarning = "";
            }
        }

        private void UpdateTelemetryUI()
        {
            if (solGameManager == null) solGameManager = SolGameManager.Instance;

            // Sol Number
            if (solNumberText != null && solGameManager != null)
            {
                solNumberText.text = $"SOL {solGameManager.CurrentSol} / {SolGameManager.MaxSols}";
            }

            // Outpost Stats
            if (solGameManager != null)
            {
                if (powerText != null) powerText.text = $"POWER: {solGameManager.GlobalPower:0}%";
                if (powerSlider != null) powerSlider.value = solGameManager.GlobalPower / 100f;

                if (shieldText != null) shieldText.text = $"SHIELD: {solGameManager.RadiationShield:0}%";
                if (shieldSlider != null) shieldSlider.value = solGameManager.RadiationShield / 100f;
            }

            // Player Life Support
            if (player != null)
            {
                if (oxygenText != null)
                {
                    if (player.currentOxygen <= 0f)
                        oxygenText.text = "SUIT O2: 0% [CRITICAL FAILURE]";
                    else
                        oxygenText.text = $"SUIT O2: {player.currentOxygen:0}%";
                }
                if (oxygenSlider != null) oxygenSlider.value = player.currentOxygen / player.maxOxygen;

                if (staminaText != null) staminaText.text = $"STAMINA: {player.currentStamina:0}%";
                if (staminaSlider != null) staminaSlider.value = player.currentStamina / player.maxStamina;

                if (suitModeText != null)
                {
                    if (player.currentOxygen <= 0f) suitModeText.text = "MODE: [DECEASED]";
                    else if (!player.IsGrounded) suitModeText.text = "MODE: [LOW-G AIRBORNE]";
                    else if (player.IsCrouching) suitModeText.text = "MODE: [CROUCHED]";
                    else if (player.IsRunning) suitModeText.text = "MODE: [SPRINTING]";
                    else suitModeText.text = "MODE: [WALKING]";
                }
            }
        }

        private void UpdateInteractionUI()
        {
            if (interactionPromptText == null) return;

            if (interaction != null && interaction.CurrentTarget != null)
            {
                string prompt = interaction.CurrentPrompt;
                interactionPromptText.text = prompt;
                if (!interactionPromptText.gameObject.activeSelf && !string.IsNullOrEmpty(prompt))
                {
                    interactionPromptText.gameObject.SetActive(true);
                }
            }
            else
            {
                if (interactionPromptText.gameObject.activeSelf)
                {
                    interactionPromptText.text = "";
                    interactionPromptText.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateNasaWarningUI()
        {
            if (nasaWarningText == null) return;

            if (!string.IsNullOrEmpty(activeNasaWarning) && nasaWarningTimer > 0f)
            {
                nasaWarningText.text = activeNasaWarning;
                if (!nasaWarningText.gameObject.activeSelf) nasaWarningText.gameObject.SetActive(true);
            }
            else
            {
                if (nasaWarningText.gameObject.activeSelf)
                {
                    nasaWarningText.text = "";
                    nasaWarningText.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateNotificationUI()
        {
            if (notificationText == null) return;

            if (!string.IsNullOrEmpty(activeNotification) && notificationTimer > 0f)
            {
                notificationText.text = activeNotification;
                if (!notificationText.gameObject.activeSelf) notificationText.gameObject.SetActive(true);
            }
            else
            {
                if (notificationText.gameObject.activeSelf)
                {
                    notificationText.text = "";
                    notificationText.gameObject.SetActive(false);
                }
            }
        }
    }
}

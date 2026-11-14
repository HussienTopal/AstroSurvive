using System;
using UnityEngine;
using AstroSurvive.Player;
using AstroSurvive.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace AstroSurvive.Core
{
    public class SolGameManager : MonoBehaviour
    {
        public static SolGameManager Instance { get; private set; }

        [Header("5-Sol Mission Stats")]
        [Tooltip("Current mission day on Mars (Sol 1 to 5)")]
        [SerializeField] private int currentSol = 1;
        public const int MaxSols = 5;

        [Tooltip("Global Outpost Power (0 - 100%)")]
        [Range(0f, 100f)]
        [SerializeField] private float globalPower = 100f;

        [Tooltip("Electromagnetic Radiation Deflection Shield (0 - 100%)")]
        [Range(0f, 100f)]
        [SerializeField] private float radiationShield = 100f;

        [Header("References")]
        [SerializeField] private PlayerController playerController;

        [Header("Status")]
        [SerializeField] private bool isGameOver = false;
        [SerializeField] private bool isMissionComplete = false;

        // Public properties
        public int CurrentSol => currentSol;
        public float GlobalPower => globalPower;
        public float RadiationShield => radiationShield;
        public bool IsGameOver => isGameOver;
        public bool IsMissionComplete => isMissionComplete;

        public PlayerController Player
        {
            get
            {
                if (playerController == null)
                    playerController = FindFirstObjectByType<PlayerController>();
                return playerController;
            }
        }

        // Events
        public event Action<int> OnSolChanged;
        public event Action<float, float> OnStatsChanged; // power, shield
        public event Action<string> OnGameOver;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (playerController == null)
            {
                playerController = FindFirstObjectByType<PlayerController>();
            }
        }

        private void Start()
        {
            // Initial notification
            MarsHUD.ShowNotification($"ARES OUTPOST: SOL {currentSol}/{MaxSols} STARTED. Power: {globalPower:0}% | Shield: {radiationShield:0}%", 5f);
            CheckFailConditions();
        }

        private void Update()
        {
            CheckFailConditions();

            // Debug hotkey for testing Sol advancement: F1 or T key
            HandleDebugInput();
        }

        private void HandleDebugInput()
        {
            if (isGameOver || isMissionComplete) return;

            bool advancePressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                advancePressed = Keyboard.current.tKey.wasPressedThisFrame || Keyboard.current.f1Key.wasPressedThisFrame;
            }
#else
            advancePressed = Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.F1);
#endif
            if (advancePressed)
            {
                AdvanceSol();
            }
        }

        public void AdvanceSol()
        {
            if (isGameOver || isMissionComplete) return;

            if (currentSol >= MaxSols)
            {
                CompleteMission();
                return;
            }

            currentSol++;
            OnSolChanged?.Invoke(currentSol);

            Debug.Log($"[SolGameManager] Advancing to Sol {currentSol}/{MaxSols}...");

            // Trigger NASA Space Weather Event via NasaEventManager
            if (NasaEventManager.Instance != null)
            {
                NasaEventManager.Instance.TriggerEventForSol(currentSol);
            }
            else
            {
                MarsHUD.ShowNotification($"Advanced to Sol {currentSol}/{MaxSols}. All systems nominal.", 4f);
            }
        }

        public void ApplyTradeOff(float powerDelta, float shieldDelta, float oxygenDeltaPercent = 0f)
        {
            ModifyPower(powerDelta);
            ModifyShield(shieldDelta);

            if (Mathf.Abs(oxygenDeltaPercent) > 0.001f && Player != null)
            {
                float oxygenDelta = (oxygenDeltaPercent / 100f) * Player.maxOxygen;
                Player.ModifyOxygen(oxygenDelta);
            }

            OnStatsChanged?.Invoke(globalPower, radiationShield);
            CheckFailConditions();
        }

        public void ModifyPower(float delta)
        {
            globalPower = Mathf.Clamp(globalPower + delta, 0f, 100f);
            OnStatsChanged?.Invoke(globalPower, radiationShield);
            CheckFailConditions();
        }

        public void ModifyShield(float delta)
        {
            radiationShield = Mathf.Clamp(radiationShield + delta, 0f, 100f);
            OnStatsChanged?.Invoke(globalPower, radiationShield);
            CheckFailConditions();
        }

        private void CheckFailConditions()
        {
            if (isGameOver) return;

            if (globalPower <= 0f)
            {
                TriggerGameOver("CRITICAL POWER FAILURE: Habitat Life Support & Outpost Systems Offline.");
                return;
            }

            if (radiationShield <= 0f)
            {
                TriggerGameOver("RADIATION SHIELD COLLAPSE: Lethal Solar Particle Influx Penetrated Outpost.");
                return;
            }

            if (Player != null && Player.currentOxygen <= 0f)
            {
                TriggerGameOver("LIFE SUPPORT DEPLETED: Suit Oxygen Tanks Exhausted on Mars Surface.");
                return;
            }
        }

        private void TriggerGameOver(string reason)
        {
            isGameOver = true;
            Debug.LogError($"[SolGameManager] GAME OVER: {reason}");
            MarsHUD.ShowNotification($"[GAME OVER] {reason}", 10f);
            OnGameOver?.Invoke(reason);
        }

        private void CompleteMission()
        {
            isMissionComplete = true;
            string victoryMsg = $"MISSION SUCCESS! You survived all 5 Sols on Mars! Final Power: {globalPower:0}% | Shield: {radiationShield:0}%";
            Debug.Log($"[SolGameManager] {victoryMsg}");
            MarsHUD.ShowNotification(victoryMsg, 10f);
        }
    }
}

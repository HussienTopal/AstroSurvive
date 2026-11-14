using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MarsOutpostController : MonoBehaviour
{
    public static MarsOutpostController Instance { get; private set; }

    [Header("--- 5-Sol Survival Telemetry ---")]
    [Range(1, 5)] public int CurrentSol = 1;
    [Range(0, 100)] public float Power = 80f;
    [Range(0, 100)] public float Oxygen = 80f;
    [Range(0, 100)] public float ShieldIntegrity = 50f;

    [Header("--- Rates & Balance (per second) ---")]
    [Tooltip("Shield ON: Shield recovery rate per second")]
    public float shieldOnIntegrityGain = 5f;
    [Tooltip("Shield ON: Power consumption rate per second")]
    public float shieldOnPowerDrain = 4f;
    [Tooltip("Shield OFF: Solar panel power recovery per second (if intact)")]
    public float shieldOffPowerRecovery = 3f;
    [Tooltip("Shield OFF: Natural shield integrity decay per second")]
    public float shieldOffIntegrityDecay = 2f;
    [Tooltip("Passive suit/habitat oxygen consumption per second")]
    public float passiveOxygenDrain = 0.45f;

    [Header("--- Scene GameObjects ---")]
    [SerializeField] public GameObject sheildObject;
    [SerializeField] public GameObject solarPanelsObject;
    [SerializeField] public GameObject plantsHouseObject;
    [SerializeField] public FirstPersonController playerController;
    [SerializeField] public Camera playerCamera;

    [Header("--- Interaction Balances ---")]
    public float interactionDistance = 5.5f;
    public float solarRepairOxygenCost = 15f;
    public float harvestOxygenGain = 25f;
    public float harvestPowerCost = 20f;

    [Header("--- State Flags ---")]
    public bool isShieldActive = true;
    public bool areSolarPanelsDamaged = false;
    public bool isGameOver = false;
    public bool isGameWon = false;

    [Header("--- UI References (Visually Editable) ---")]
    public Canvas hudCanvas;
    public Text solText;
    public Text powerText;
    public Text oxygenText;
    public Text shieldText;
    public Text domeStatusText;

    public Image powerBar;
    public Image oxygenBar;
    public Image shieldBar;

    public Text controlsGuideText;
    public Text crosshairPromptText;
    public Text feedbackNotificationText;

    [Header("--- NASA Educational Science Banner ---")]
    public GameObject scienceBannerPanel;
    public Text scienceBannerTitle;
    public Text scienceBannerNote;

    [Header("--- Mission Outcome Panels ---")]
    public GameObject missionFailedPanel;
    public Text missionFailedReasonText;
    public GameObject missionAccomplishedPanel;
    public Text missionAccomplishedText;

    private float notificationTimer = 0f;
    private Coroutine bannerHideCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        AutoResolveSceneReferences();
    }

    private void Start()
    {
        // Ensure shield starts in correct state
        if (sheildObject != null)
        {
            isShieldActive = sheildObject.activeSelf;
        }

        // Add colliders to models if missing so raycast hits them reliably
        EnsureCollidersExist(solarPanelsObject);
        EnsureCollidersExist(plantsHouseObject);

        // Intact panels at start
        SetSolarPanelsDamaged(false);

        // Display Sol 1 NASA note
        ShowNasaScienceNote(CurrentSol);

        ShowNotification($"<color=#3BCEFF>EXPEDITION SOL 1 BEGUN!</color> Protect base with [1], survive to Sol 5.", 5f);
        UpdateHUDVisuals();
    }

    private void Update()
    {
        if (isGameOver || isGameWon)
        {
            if (CheckKeyRestart())
            {
                RestartMission();
            }
            return;
        }

        HandleInputHotkeys();
        HandleResourceSimulation();
        HandleAimRaycastInteraction();
        UpdateHUDVisuals();
        CheckWinLossConditions();

        // Notification countdown
        if (notificationTimer > 0f)
        {
            notificationTimer -= Time.deltaTime;
            if (notificationTimer <= 0f && feedbackNotificationText != null)
            {
                feedbackNotificationText.text = "";
            }
        }
    }

    public void AutoResolveSceneReferences()
    {
        if (sheildObject == null)
            sheildObject = GameObject.Find("Sheild") ?? GameObject.Find("Shield") ?? GameObject.Find("Copule");

        if (solarPanelsObject == null)
            solarPanelsObject = GameObject.Find("Solar Panels");

        if (plantsHouseObject == null)
            plantsHouseObject = GameObject.Find("Plants House") ?? GameObject.Find("Plants");

        if (playerController == null)
            playerController = FindFirstObjectByType<FirstPersonController>();

        if (playerCamera == null)
        {
            if (playerController != null && playerController.playerCamera != null)
                playerCamera = playerController.playerCamera;
            else
                playerCamera = Camera.main;
        }
    }

    private void EnsureCollidersExist(GameObject root)
    {
        if (root == null) return;
        if (root.GetComponentInChildren<Collider>() == null)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if (r.GetComponent<Collider>() == null)
                {
                    r.gameObject.AddComponent<BoxCollider>();
                }
            }
        }
    }

    #region Input Handling
    private void HandleInputHotkeys()
    {
        // Key [1] Toggle Shield
        if (CheckKey1())
        {
            ToggleShieldDome();
        }

        // Key [2] Advance Sol & Storm
        if (CheckKey2())
        {
            AdvanceSol();
        }
    }

    private bool CheckKey1()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame))
            return true;
#endif
        return Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1);
    }

    private bool CheckKey2()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame))
            return true;
#endif
        return Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2);
    }

    private bool CheckKeyE()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            return true;
#endif
        return Input.GetKeyDown(KeyCode.E);
    }

    private bool CheckKeyRestart()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            return true;
#endif
        return Input.GetKeyDown(KeyCode.R);
    }
    #endregion

    #region Resource Simulation
    private void HandleResourceSimulation()
    {
        // 1. Passive life support oxygen depletion
        Oxygen = Mathf.Max(0f, Oxygen - passiveOxygenDrain * Time.deltaTime);

        // 2. Shield dynamics
        if (isShieldActive)
        {
            // Shield ON: Charges shield, heavy power drain
            ShieldIntegrity = Mathf.Min(100f, ShieldIntegrity + shieldOnIntegrityGain * Time.deltaTime);
            Power = Mathf.Max(0f, Power - shieldOnPowerDrain * Time.deltaTime);
        }
        else
        {
            // Shield OFF: Shield integrity decays
            ShieldIntegrity = Mathf.Max(0f, ShieldIntegrity - shieldOffIntegrityDecay * Time.deltaTime);

            // Power generation from solar panels if intact
            if (!areSolarPanelsDamaged)
            {
                Power = Mathf.Min(100f, Power + shieldOffPowerRecovery * Time.deltaTime);
            }
        }
    }
    #endregion

    #region Shield Dome Toggle (Key 1)
    public void ToggleShieldDome()
    {
        isShieldActive = !isShieldActive;
        if (sheildObject != null)
        {
            sheildObject.SetActive(isShieldActive);
        }

        if (isShieldActive)
        {
            ShowNotification("<color=#00F0FF><b>[DOME SHIELD ACTIVATED]</b></color> +5% Shield/s | -4% Power/s. Base protected from storms!", 3.5f);
        }
        else
        {
            string pwrMsg = areSolarPanelsDamaged ? "Power regeneration OFFLINE (Damaged)." : "+3% Power/s regenerating.";
            ShowNotification($"<color=#FFD13B><b>[DOME SHIELD DEACTIVATED]</b></color> Shield dropping. {pwrMsg}", 3.5f);
        }
    }
    #endregion

    #region Advance Sol & NASA Space Weather Storm (Key 2)
    public void AdvanceSol()
    {
        if (isGameOver || isGameWon) return;

        if (CurrentSol >= 5)
        {
            // Completing Sol 5
            TriggerMissionAccomplished();
            return;
        }

        CurrentSol++;

        // Fetch NASA DONKI Science Event
        NasaDataManager.SpaceWeatherItem evt = null;
        if (NasaDataManager.Instance != null)
        {
            evt = NasaDataManager.Instance.GetEventForSol(CurrentSol);
        }

        if (evt == null)
        {
            evt = new NasaDataManager.SpaceWeatherItem
            {
                sol_day = CurrentSol,
                event_type = "Solar_Radiation_Surge",
                severity = "High",
                nasa_flr_id = $"2024-05-1{CurrentSol}T09:00:00Z",
                educational_note = "Solar energetic particles accelerate along interplanetary magnetic field lines directly toward Mars."
            };
        }

        // Show NASA Educational Note Banner
        ShowNasaScienceNote(CurrentSol, evt);

        // Execute Storm Physics
        TriggerSolarStorm(evt);
    }

    private void TriggerSolarStorm(NasaDataManager.SpaceWeatherItem evt)
    {
        if (isShieldActive)
        {
            // Shield absorbs storm
            float absorbedDamage = 20f;
            ShieldIntegrity = Mathf.Max(0f, ShieldIntegrity - absorbedDamage);

            ShowNotification($"<color=#00F0FF><b>[STORM DEFLECTED!]</b></color> Blue Shield Dome absorbed {evt.event_type.Replace('_', ' ')}! Solar panels protected. (-20% Shield)", 6f);
        }
        else
        {
            // Unshielded: Solar panels break, power and shield take heavy damage
            SetSolarPanelsDamaged(true);
            ShieldIntegrity = Mathf.Max(0f, ShieldIntegrity - 35f);
            Power = Mathf.Max(0f, Power - 10f);

            ShowNotification($"<color=#FF3344><b>[WARNING: SOLAR STORM HIT!]</b></color> Unshielded base struck! Solar Panels DAMAGED & Offline. Look at panels & press [E] to repair (-15% O2).", 6.5f);
        }
    }

    public void SetSolarPanelsDamaged(bool damaged)
    {
        areSolarPanelsDamaged = damaged;
        if (solarPanelsObject == null) return;

        Transform[] allChildren = solarPanelsObject.GetComponentsInChildren<Transform>(true);
        foreach (var child in allChildren)
        {
            if (child.name.Equals("LargeSolarPanelArrayDamaged", StringComparison.OrdinalIgnoreCase))
            {
                child.gameObject.SetActive(damaged);
            }
            else if (child.name.Equals("LargeSolarPanelArray", StringComparison.OrdinalIgnoreCase))
            {
                if (child != solarPanelsObject.transform)
                {
                    child.gameObject.SetActive(!damaged);
                }
            }
        }
    }
    #endregion

    #region Raycast Physical Interaction (Key E)
    private void HandleAimRaycastInteraction()
    {
        if (crosshairPromptText == null) return;

        bool lookingAtSolar = false;
        bool lookingAtPlants = false;

        // 1. Raycast from camera
        if (playerCamera != null)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, ~0, QueryTriggerInteraction.Collide))
            {
                if (solarPanelsObject != null && (hit.transform == solarPanelsObject.transform || hit.transform.IsChildOf(solarPanelsObject.transform)))
                {
                    lookingAtSolar = true;
                }
                else if (plantsHouseObject != null && (hit.transform == plantsHouseObject.transform || hit.transform.IsChildOf(plantsHouseObject.transform)))
                {
                    lookingAtPlants = true;
                }
            }
        }

        // 2. Proximity fallback (if player is right next to them)
        if (!lookingAtSolar && !lookingAtPlants && playerController != null)
        {
            Vector3 playerPos = playerController.transform.position;
            if (solarPanelsObject != null && Vector3.Distance(playerPos, solarPanelsObject.transform.position) <= 8f)
            {
                lookingAtSolar = true;
            }
            else if (plantsHouseObject != null && Vector3.Distance(playerPos, plantsHouseObject.transform.position) <= 20f)
            {
                lookingAtPlants = true;
            }
        }

        // Process Prompts & Key 'E'
        if (lookingAtSolar)
        {
            if (areSolarPanelsDamaged)
            {
                crosshairPromptText.text = $"<color=#FFD13B><b>[E] REPAIR SOLAR PANELS</b> (-{solarRepairOxygenCost}% O2)</color>";
                if (CheckKeyE())
                {
                    AttemptRepairSolarPanels();
                }
            }
            else
            {
                crosshairPromptText.text = "<color=#44FF77>Solar Panels: 100% Operational (+3% Power/s)</color>";
            }
        }
        else if (lookingAtPlants)
        {
            crosshairPromptText.text = $"<color=#3BCEFF><b>[E] HARVEST OXYGEN</b> (+{harvestOxygenGain}% O2, -{harvestPowerCost}% Power)</color>";
            if (CheckKeyE())
            {
                AttemptHarvestOxygen();
            }
        }
        else
        {
            crosshairPromptText.text = "";
        }
    }

    private void AttemptRepairSolarPanels()
    {
        if (Oxygen >= solarRepairOxygenCost)
        {
            Oxygen -= solarRepairOxygenCost;
            SetSolarPanelsDamaged(false);
            ShowNotification("<color=#44FF77><b>[REPAIR SUCCESSFUL]</b></color> Solar Panels operational! Power generation resumed.", 4f);
        }
        else
        {
            ShowNotification($"<color=#FF3344><b>[NOT ENOUGH OXYGEN]</b></color> Need at least {solarRepairOxygenCost}% O2 for EVA repairs!", 3.5f);
        }
    }

    private void AttemptHarvestOxygen()
    {
        if (Power >= harvestPowerCost)
        {
            Power -= harvestPowerCost;
            Oxygen = Mathf.Min(100f, Oxygen + harvestOxygenGain);
            ShowNotification($"<color=#3BCEFF><b>[HARVEST SUCCESSFUL]</b></color> +{harvestOxygenGain}% Oxygen generated from greenhouse hydroponics!", 4f);
        }
        else
        {
            ShowNotification($"<color=#FF3344><b>[LOW POWER]</b></color> Need at least {harvestPowerCost}% Power to cycle greenhouse life support!", 3.5f);
        }
    }
    #endregion

    #region NASA Science Banner
    public void ShowNasaScienceNote(int sol, NasaDataManager.SpaceWeatherItem evt = null)
    {
        if (evt == null && NasaDataManager.Instance != null)
        {
            evt = NasaDataManager.Instance.GetEventForSol(sol);
        }

        if (scienceBannerPanel != null)
        {
            scienceBannerPanel.SetActive(true);

            if (scienceBannerTitle != null)
            {
                string evtName = evt != null ? evt.event_type.Replace('_', ' ') : "Solar Weather Observation";
                string sev = evt != null ? evt.severity : "Normal";
                scienceBannerTitle.text = $"★ NASA DONKI SCIENCE LOG | SOL {sol} — {evtName} ({sev}) ★";
            }

            if (scienceBannerNote != null)
            {
                string note = evt != null ? evt.educational_note : "Cosmic radiation requires continuous telemetry monitoring.";
                scienceBannerNote.text = $"<i>\"{note}\"</i>";
            }
        }
    }
    #endregion

    #region Win & Loss Evaluation
    private void CheckWinLossConditions()
    {
        if (Power <= 0f)
        {
            TriggerMissionFailed("MISSION FAILED: Base Power depleted to 0%! All thermal and life support systems shut down.");
        }
        else if (Oxygen <= 0f)
        {
            TriggerMissionFailed("MISSION FAILED: Oxygen depleted to 0%! Asphyxiation occurred on Martian surface.");
        }
        else if (ShieldIntegrity <= 0f)
        {
            TriggerMissionFailed("MISSION FAILED: Radiation Shield Integrity hit 0%! Lethal cosmic rays penetrated habitat.");
        }
    }

    public void TriggerMissionFailed(string reason)
    {
        if (isGameOver || isGameWon) return;
        isGameOver = true;

        if (missionFailedPanel != null)
        {
            missionFailedPanel.SetActive(true);
            if (missionFailedReasonText != null)
                missionFailedReasonText.text = reason;
        }

        ShowNotification("<color=#FF2233><b>[CRITICAL FAILURE]</b></color> Press [R] to Restart Expedition.", 999f);
    }

    public void TriggerMissionAccomplished()
    {
        if (isGameOver || isGameWon) return;
        isGameWon = true;

        if (missionAccomplishedPanel != null)
        {
            missionAccomplishedPanel.SetActive(true);
            if (missionAccomplishedText != null)
            {
                missionAccomplishedText.text = $"Congratulations, Commander!\n\nYou successfully protected the Mars Outpost and survived all 5 Sols through severe space weather and solar storms!\n\nFinal Stats — Power: {Mathf.RoundToInt(Power)}% | Oxygen: {Mathf.RoundToInt(Oxygen)}% | Shield: {Mathf.RoundToInt(ShieldIntegrity)}%";
            }
        }

        ShowNotification("<color=#44FF77><b>[EXPEDITION ACCOMPLISHED!]</b></color> Press [R] to Play Again.", 999f);
    }

    public void RestartMission()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ShowNotification(string message, float duration = 4f)
    {
        if (feedbackNotificationText != null)
        {
            feedbackNotificationText.text = message;
            notificationTimer = duration;
        }
    }
    #endregion

    #region HUD Visual Refresh
    private void UpdateHUDVisuals()
    {
        if (solText != null)
            solText.text = $"SOL: {CurrentSol} / 5";

        if (powerText != null)
        {
            string status = isShieldActive ? "(-4%/s)" : (areSolarPanelsDamaged ? "(OFFLINE)" : "(+3%/s)");
            powerText.text = $"POWER: {Mathf.RoundToInt(Power)}%  {status}";
        }

        if (oxygenText != null)
            oxygenText.text = $"OXYGEN: {Mathf.RoundToInt(Oxygen)}%";

        if (shieldText != null)
        {
            string status = isShieldActive ? "(+5%/s)" : "(-2%/s)";
            shieldText.text = $"SHIELD: {Mathf.RoundToInt(ShieldIntegrity)}%  {status}";
        }

        if (domeStatusText != null)
        {
            if (isShieldActive)
                domeStatusText.text = "<color=#00F0FF>DOME STATUS: ON (SHIELDING ACTIVE)</color>";
            else
                domeStatusText.text = "<color=#FF6655>DOME STATUS: OFF (UNSHIELDED)</color>";
        }

        if (powerBar != null)
            powerBar.fillAmount = Power / 100f;

        if (oxygenBar != null)
            oxygenBar.fillAmount = Oxygen / 100f;

        if (shieldBar != null)
            shieldBar.fillAmount = ShieldIntegrity / 100f;

        if (controlsGuideText != null)
        {
            controlsGuideText.text = "<b>MISSION CONTROLS:</b>\n" +
                                     "[1] Toggle Blue Shield Dome\n" +
                                     "[2] Advance Sol (NASA Event)\n" +
                                     "[E] Repair Solar / Harvest O2\n" +
                                     "[R] Restart Mission";
        }
    }
    #endregion
}

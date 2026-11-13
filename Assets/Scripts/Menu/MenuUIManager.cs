using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AstroSurvive.Menu
{
    /// <summary>
    /// Master coordinator for World Space UI panels, button events, and camera states.
    /// Choreographs the menu flow between the cruising shuttle, solar system overview,
    /// planetary fact cards, Mars mission briefing, and the secret Black Hole easter egg.
    /// </summary>
    public class MenuUIManager : MonoBehaviour
    {
        public static MenuUIManager Instance { get; private set; }

        [Header("Camera Rig Reference")]
        public MenuCameraController cameraController;

        [Header("Camera Mounts")]
        public Transform mountMainMenuShuttle;
        public Transform mountMarsBriefing;
        public Transform mountSolarOverview;
        public Transform mountBlackHoleSecret;
        public Transform mountOptions;

        [Header("UI Canvas Panels (World Space)")]
        public GameObject mainMenuPanel;
        public GameObject missionBriefingPanel;
        public GameObject solarSystemOverviewPanel;
        public GameObject planetFactPanel;
        public GameObject secretEasterEggBanner;
        public GameObject optionsPanel;
        public GameObject skipIntroPrompt;

        [Header("Planet Fact Card Elements")]
        public TextMeshProUGUI factPlanetNameText;
        public TextMeshProUGUI factHeadlineText;
        public TextMeshProUGUI fact1Text;
        public TextMeshProUGUI fact2Text;
        public TextMeshProUGUI fact3Text;
        public TextMeshProUGUI factStatTagText;
        public Image factHeaderAccentImage;

        [Header("Interactive Planet Items In Scene")]
        public List<PlanetInteractiveItem> planetItems = new List<PlanetInteractiveItem>();
        public PlanetInteractiveItem blackHoleItem;

        [Header("Audio Feedback")]
        public AudioSource menuAudioSource;
        public AudioClip clickSfx;
        public AudioClip whooshSfx;
        public AudioClip secretFoundSfx;

        [Header("Gameplay Scene Setup")]
        public string gameplaySceneName = "Topal_TestScene";

        private Dictionary<string, PlanetFact> factDatabase = new Dictionary<string, PlanetFact>();
        private int currentPlanetIndex = 0;
        private PlanetInteractiveItem selectedPlanet;

        // All world-space UI panels (populated at runtime for LookAt)
        private GameObject[] _allWorldSpacePanels;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            InitializeFactDatabase();
        }

        private void Start()
        {
            AutoWireButtons();

            // Collect all canvas panels for LookAt facing
            _allWorldSpacePanels = new GameObject[]
            {
                mainMenuPanel, missionBriefingPanel, solarSystemOverviewPanel,
                planetFactPanel, optionsPanel
            };

            // MenuCameraController has [DefaultExecutionOrder(-100)] so IsIntroActive is reliable here
            if (cameraController != null && cameraController.IsIntroActive)
            {
                HideAllPanels();
                if (skipIntroPrompt != null) skipIntroPrompt.SetActive(true);
                cameraController.onIntroComplete = () =>
                {
                    ShowMainMenu();
                    if (skipIntroPrompt != null) skipIntroPrompt.SetActive(false);
                };
            }
            else
            {
                ShowMainMenu();
            }
        }

        private void LateUpdate()
        {
            // Keep all active World Space canvases facing the camera
            if (Camera.main == null) return;
            Vector3 camPos = Camera.main.transform.position;

            if (_allWorldSpacePanels == null) return;
            foreach (var panel in _allWorldSpacePanels)
            {
                if (panel == null || !panel.activeInHierarchy) continue;
                Vector3 dir = panel.transform.position - camPos;
                if (dir.sqrMagnitude > 0.001f)
                    panel.transform.rotation = Quaternion.LookRotation(dir);
            }
        }

        private void AutoWireButtons()
        {
            // Auto-wire all buttons across loaded canvases so interactions work automatically
            var allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var btn in allButtons)
            {
                if (btn == null) continue;
                string bName = btn.gameObject.name;

                btn.onClick.RemoveAllListeners();

                if (bName == "Btn_Play") btn.onClick.AddListener(OnPlayClicked);
                else if (bName == "Btn_Explore") btn.onClick.AddListener(OnExplorePlanetsClicked);
                else if (bName == "Btn_Options") btn.onClick.AddListener(OnOptionsClicked);
                else if (bName == "Btn_Exit") btn.onClick.AddListener(OnExitClicked);
                else if (bName == "Btn_Launch") btn.onClick.AddListener(OnLaunchMissionClicked);
                else if (bName == "Btn_BackShuttle" || bName == "Btn_BackToMenu" || bName == "Btn_FactMainMenu" || bName == "Btn_OptBack")
                    btn.onClick.AddListener(OnBackToMainMenuClicked);
                else if (bName == "Btn_BackSolar") btn.onClick.AddListener(OnBackToSolarOverviewClicked);
                else if (bName == "Btn_PrevPlanet") btn.onClick.AddListener(PreviousPlanet);
                else if (bName == "Btn_NextPlanet") btn.onClick.AddListener(NextPlanet);
                else if (bName.StartsWith("Btn_Quick_"))
                {
                    string pName = bName.Substring("Btn_Quick_".Length);
                    btn.onClick.AddListener(() => SelectPlanetByName(pName));
                }
            }
        }

        private void Update()
        {
            // Allow skipping intro with Space or Escape or Left Click
            if (skipIntroPrompt != null && skipIntroPrompt.activeSelf)
            {
                if (Input.anyKeyDown)
                {
                    SkipIntro();
                }
            }
        }

        private void InitializeFactDatabase()
        {
            factDatabase.Clear();
            foreach (var fact in PlanetFactDatabase.GetDefaultFacts())
            {
                factDatabase[fact.planetName.ToLower()] = fact;
            }
        }

        #region Navigation & Camera Transitions

        public void SkipIntro()
        {
            if (cameraController != null)
            {
                cameraController.SkipIntro();
            }
            if (skipIntroPrompt != null)
            {
                skipIntroPrompt.SetActive(false);
            }
        }

        public void OnPlayClicked()
        {
            PlaySound(whooshSfx);
            HideAllPanels();
            if (missionBriefingPanel != null) missionBriefingPanel.SetActive(true);

            if (cameraController != null && mountMarsBriefing != null)
            {
                cameraController.SetMount(mountMarsBriefing);
            }
        }

        public void OnLaunchMissionClicked()
        {
            PlaySound(clickSfx);
            if (cameraController != null)
            {
                cameraController.LoadScene(gameplaySceneName);
            }
        }

        public void OnExplorePlanetsClicked()
        {
            PlaySound(whooshSfx);
            HideAllPanels();
            if (solarSystemOverviewPanel != null) solarSystemOverviewPanel.SetActive(true);

            if (cameraController != null && mountSolarOverview != null)
            {
                cameraController.SetMount(mountSolarOverview);
            }
        }

        public void OnOptionsClicked()
        {
            PlaySound(clickSfx);
            HideAllPanels();
            if (optionsPanel != null) optionsPanel.SetActive(true);

            if (cameraController != null && mountOptions != null)
            {
                cameraController.SetMount(mountOptions);
            }
        }

        public void OnBackToMainMenuClicked()
        {
            PlaySound(whooshSfx);
            ShowMainMenu();

            if (cameraController != null && mountMainMenuShuttle != null)
            {
                cameraController.SetMount(mountMainMenuShuttle);
            }
        }

        public void OnBackToSolarOverviewClicked()
        {
            PlaySound(whooshSfx);
            HideAllPanels();
            if (solarSystemOverviewPanel != null) solarSystemOverviewPanel.SetActive(true);

            if (cameraController != null && mountSolarOverview != null)
            {
                cameraController.SetMount(mountSolarOverview);
            }
        }

        public void OnExitClicked()
        {
            PlaySound(clickSfx);
            if (cameraController != null)
            {
                cameraController.Quit();
            }
        }

        #endregion

        #region Planet Selection & Easter Egg

        public void SelectPlanetByName(string name)
        {
            for (int i = 0; i < planetItems.Count; i++)
            {
                if (planetItems[i] != null && planetItems[i].planetName.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                {
                    SelectPlanet(planetItems[i]);
                    return;
                }
            }
        }

        public void SelectPlanet(PlanetInteractiveItem item)
        {
            if (item == null) return;
            selectedPlanet = item;

            HideAllPanels();

            if (item.isSecretEasterEgg)
            {
                // Trigger the secret Easter Egg discovery!
                PlaySound(secretFoundSfx != null ? secretFoundSfx : whooshSfx);

                if (secretEasterEggBanner != null)
                {
                    secretEasterEggBanner.SetActive(true);
                }

                if (planetFactPanel != null)
                {
                    planetFactPanel.SetActive(true);
                    PopulateFactCard(item.planetName);
                }

                if (cameraController != null)
                {
                    Transform targetMount = item.cameraFocusPoint != null ? item.cameraFocusPoint : mountBlackHoleSecret;
                    cameraController.SetMount(targetMount);
                }
            }
            else
            {
                PlaySound(whooshSfx);

                if (planetFactPanel != null)
                {
                    planetFactPanel.SetActive(true);
                    PopulateFactCard(item.planetName);
                }

                // Update current index for Next / Previous cycling
                for (int i = 0; i < planetItems.Count; i++)
                {
                    if (planetItems[i] == item)
                    {
                        currentPlanetIndex = i;
                        break;
                    }
                }

                if (cameraController != null && item.cameraFocusPoint != null)
                {
                    cameraController.SetMount(item.cameraFocusPoint);
                }
            }
        }

        public void NextPlanet()
        {
            if (planetItems.Count == 0) return;
            currentPlanetIndex = (currentPlanetIndex + 1) % planetItems.Count;
            SelectPlanet(planetItems[currentPlanetIndex]);
        }

        public void PreviousPlanet()
        {
            if (planetItems.Count == 0) return;
            currentPlanetIndex = (currentPlanetIndex - 1 + planetItems.Count) % planetItems.Count;
            SelectPlanet(planetItems[currentPlanetIndex]);
        }

        private void PopulateFactCard(string name)
        {
            string key = name.ToLower();
            if (factDatabase.TryGetValue(key, out PlanetFact fact))
            {
                if (factPlanetNameText != null) factPlanetNameText.text = fact.planetName.ToUpper();
                if (factHeadlineText != null) factHeadlineText.text = fact.headline;
                if (fact1Text != null) fact1Text.text = "- " + fact.fact1;
                if (fact2Text != null) fact2Text.text = "- " + fact.fact2;
                if (fact3Text != null) fact3Text.text = "- " + fact.fact3;
                if (factStatTagText != null) factStatTagText.text = fact.statTag;

                if (factHeaderAccentImage != null)
                {
                    factHeaderAccentImage.color = fact.themeColor;
                }
            }
        }

        #endregion

        #region Helpers

        private void ShowMainMenu()
        {
            HideAllPanels();
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (skipIntroPrompt != null) skipIntroPrompt.SetActive(true);
        }

        private void HideAllPanels()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (missionBriefingPanel != null) missionBriefingPanel.SetActive(false);
            if (solarSystemOverviewPanel != null) solarSystemOverviewPanel.SetActive(false);
            if (planetFactPanel != null) planetFactPanel.SetActive(false);
            if (secretEasterEggBanner != null) secretEasterEggBanner.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(false);
        }

        private void PlaySound(AudioClip clip)
        {
            if (menuAudioSource != null && clip != null)
            {
                menuAudioSource.PlayOneShot(clip);
            }
        }

        #endregion
    }
}

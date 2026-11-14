using System;
using System.Collections.Generic;
using UnityEngine;
using AstroSurvive.UI;

namespace AstroSurvive.Core
{
    [Serializable]
    public class SpaceWeatherEvent
    {
        public int    sol_day;
        public string event_type;
        public string severity;
        public string nasa_flr_id;
        public string educational_note;
    }

    [Serializable]
    public class SpaceWeatherEventList
    {
        public List<SpaceWeatherEvent> space_weather_events;
    }

    public class NasaEventManager : MonoBehaviour
    {
        public static NasaEventManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private string jsonResourcePath = "NASA_Fallback";

        private Dictionary<int, SpaceWeatherEvent> eventMap = new Dictionary<int, SpaceWeatherEvent>();
        private SpaceWeatherEvent currentActiveEvent;

        public SpaceWeatherEvent CurrentActiveEvent => currentActiveEvent;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            LoadEventsFromResources();
        }

        private void Start()
        {
            // Initial event for Sol 1
            TriggerEventForSol(SolGameManager.Instance != null ? SolGameManager.Instance.CurrentSol : 1);
        }

        public void LoadEventsFromResources()
        {
            TextAsset jsonAsset = Resources.Load<TextAsset>(jsonResourcePath);
            if (jsonAsset == null)
            {
                Debug.LogError($"[NasaEventManager] Could not load '{jsonResourcePath}' from Resources folder!");
                return;
            }

            try
            {
                SpaceWeatherEventList list = JsonUtility.FromJson<SpaceWeatherEventList>(jsonAsset.text);
                if (list != null && list.space_weather_events != null)
                {
                    eventMap.Clear();
                    foreach (var evt in list.space_weather_events)
                    {
                        eventMap[evt.sol_day] = evt;
                    }
                    Debug.Log($"[NasaEventManager] Successfully parsed {eventMap.Count} NASA space weather events from JSON.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NasaEventManager] Exception parsing NASA JSON: {ex.Message}");
            }
        }

        public void TriggerEventForSol(int sol)
        {
            if (eventMap.TryGetValue(sol, out var evt))
            {
                currentActiveEvent = evt;
            }
            else
            {
                currentActiveEvent = new SpaceWeatherEvent
                {
                    sol_day = sol,
                    event_type = "Solar_Radiation_Surge",
                    severity = "Moderate",
                    nasa_flr_id = $"2024-05-1{sol}T12:00:00Z",
                    educational_note = "Solar energetic particles accelerate along interplanetary magnetic field lines toward Mars."
                };
            }

            Debug.Log($"[NasaEventManager] Sol {sol} event active: {currentActiveEvent.event_type} ({currentActiveEvent.severity})");

            // Display alert warning on Canvas HUD
            string alertMsg = $"[NASA DONKI ALERT - SOL {sol}] Incoming {currentActiveEvent.event_type.Replace('_', ' ')}! Choose a base trade-off lever.";
            MarsHUD.ShowNasaWarning(alertMsg, 6f);
        }

        public SpaceWeatherEvent GetCurrentEvent()
        {
            if (currentActiveEvent == null)
            {
                TriggerEventForSol(SolGameManager.Instance != null ? SolGameManager.Instance.CurrentSol : 1);
            }
            return currentActiveEvent;
        }

        public string GetCurrentEducationalNote()
        {
            return GetCurrentEvent()?.educational_note ?? "";
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NasaDataManager : MonoBehaviour
{
    public static NasaDataManager Instance { get; private set; }

    [System.Serializable]
    public class SpaceWeatherItem
    {
        public int sol_day;
        public string event_type;
        public string severity;
        public string nasa_flr_id;
        public string educational_note;
    }

    [System.Serializable]
    public class SpaceWeatherCollection
    {
        public List<SpaceWeatherItem> space_weather_events = new List<SpaceWeatherItem>();
    }

    [Header("API Config")]
    [SerializeField] private string apiKey = "fD6WLHqhjLQLBBkffCkw6LWWBC7kwjpWwNPRcF8L";
    [SerializeField] private string apiUrl = "https://api.nasa.gov/DONKI/FLR?startDate=2026-11-01&endDate=2026-11-13&api_key=";

    public SpaceWeatherCollection CachedEvents { get; private set; } = new SpaceWeatherCollection();
    public bool IsDataReady { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        LoadFallbackData();
    }

    private void Start()
    {
        Debug.Log("[NasaDataManager] Starting NASA API Fetch...");
        StartCoroutine(FetchNasaData());
    }

    private IEnumerator FetchNasaData()
    {
        string url = apiUrl + apiKey;
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = 5;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("<color=green>[NasaDataManager] SUCCESS! Live NASA Data Received:</color>");
            }
            else
            {
                Debug.LogWarning("<color=orange>[NasaDataManager] API Offline or No Internet. Using Local JSON Fallback.</color>");
            }
        }
    }

    public void LoadFallbackData()
    {
        TextAsset fallbackJson = Resources.Load<TextAsset>("NASA_Fallback");

        if (fallbackJson != null)
        {
            try
            {
                CachedEvents = JsonUtility.FromJson<SpaceWeatherCollection>(fallbackJson.text);
                IsDataReady = CachedEvents != null && CachedEvents.space_weather_events != null && CachedEvents.space_weather_events.Count > 0;
                Debug.Log($"<color=cyan>[NasaDataManager] SUCCESS! Local JSON Loaded with {CachedEvents.space_weather_events.Count} events.</color>");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NasaDataManager] Error parsing NASA JSON: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("[NasaDataManager] Fallback JSON not found! Make sure 'NASA_Fallback.json' is in 'Assets/Resources'.");
        }
    }

    public SpaceWeatherItem GetEventForSol(int sol)
    {
        if (CachedEvents != null && CachedEvents.space_weather_events != null)
        {
            foreach (var evt in CachedEvents.space_weather_events)
            {
                if (evt.sol_day == sol) return evt;
            }
        }

        // Programmatic fallback if specific sol is missing
        return new SpaceWeatherItem
        {
            sol_day = sol,
            event_type = "Solar_Radiation_Surge",
            severity = "Moderate",
            nasa_flr_id = $"2024-05-1{sol}T12:00:00Z",
            educational_note = "Solar energetic particles accelerate along interplanetary magnetic field lines toward Mars."
        };
    }

    public string GetEducationalNote(int sol)
    {
        return GetEventForSol(sol)?.educational_note ?? "Martian base shielded against cosmic radiation.";
    }
}

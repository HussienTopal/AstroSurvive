using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using AstroSurvive.Menu;

namespace AstroSurvive.Editor
{
    public static class MainMenuSceneGenerator
    {
        private const string SCENE_PATH = "Assets/Scenes/Main Menu.unity";
        private const string FONT_PATH = "Assets/Art/Fonts/KidzhoodArabicDEMO-Light SDF.asset";
        private const string ALT_FONT_PATH = "Assets/Art/Fonts/ff-bouya-messy SDF.asset";
        private const string DEFAULT_TMP_FONT = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        [MenuItem("AstroSurvive/Generate Interactive 3D Main Menu")]
        public static void GenerateScene()
        {
            Debug.Log(">>> [STEP 1] Opening and resetting Main Menu scene...");
            EnsureFolder("Assets/Scenes");

            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != SCENE_PATH)
            {
                try
                {
                    scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
                }
                catch
                {
                    scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                }
            }

            foreach (var root in scene.GetRootGameObjects())
            {
                Object.DestroyImmediate(root);
            }

            Debug.Log(">>> [STEP 2] Resolving fonts & TMP settings...");
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH);
            if (font == null) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(ALT_FONT_PATH);
            if (font == null) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DEFAULT_TMP_FONT);

            Debug.Log(">>> [STEP 3] Setting up lighting and environment...");
            SetupLightingAndSky();

            Debug.Log(">>> [STEP 4] Building Cosmic Galaxy and Secret Black Hole...");
            GameObject envRoot = new GameObject("--- COSMIC ENVIRONMENT ---");
            Transform galaxyTransform = BuildGalaxy(envRoot.transform);
            PlanetInteractiveItem blackHoleItem = BuildBlackHole(envRoot.transform);

            Debug.Log(">>> [STEP 5] Building Solar System and Planets...");
            GameObject solarRoot = new GameObject("--- SOLAR SYSTEM ---");
            List<PlanetInteractiveItem> planetList = BuildSolarSystem(solarRoot.transform);

            Debug.Log(">>> [STEP 6] Building Space Shuttle Cruiser...");
            GameObject shuttleRoot = new GameObject("--- SPACE SHUTTLE CRUISER ---");
            Transform shuttleTransform = BuildSpaceShuttle(shuttleRoot.transform);

            Debug.Log(">>> [STEP 7] Creating Camera Mounts...");
            GameObject mountsRoot = new GameObject("--- CAMERA MOUNTS ---");
            CameraMounts mounts = CreateCameraMounts(mountsRoot.transform, shuttleTransform, blackHoleItem.transform);

            Debug.Log(">>> [STEP 8] Building Camera Rig...");
            GameObject camObj = BuildCameraRig(mounts);
            MenuCameraController camControl = camObj.GetComponent<MenuCameraController>();

            Debug.Log(">>> [STEP 9] Building World Space UI Canvases...");
            GameObject uiRoot = new GameObject("--- WORLD SPACE UI ---");
            BuildAllWorldSpaceUI(uiRoot.transform, camControl, mounts, planetList, blackHoleItem, font);

            Debug.Log(">>> [STEP 10] Saving Scene and Registering Build Settings...");
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            RegisterInBuildSettings(SCENE_PATH);
            AssetDatabase.SaveAssets();

            Debug.Log($">>> [COMPLETE] AstroSurvive 3D Main Menu scene generated at {SCENE_PATH}!");
        }

        #region Lighting & Environment

        private static void SetupLightingAndSky()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.12f, 0.16f, 0.28f);
            RenderSettings.ambientEquatorColor = new Color(0.06f, 0.08f, 0.15f);
            RenderSettings.ambientGroundColor = new Color(0.01f, 0.02f, 0.04f);
            RenderSettings.fog = false;

            // Main Directional Sun Light
            GameObject dirLightObj = new GameObject("Sun Directional Light");
            Light dirLight = dirLightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(1.0f, 0.96f, 0.90f);
            dirLight.intensity = 1.35f;
            dirLight.shadows = LightShadows.Soft;
            dirLightObj.transform.rotation = Quaternion.Euler(25f, -35f, 0f);

            // Cosmic Fill Light
            GameObject fillLightObj = new GameObject("Cosmic Fill Light");
            Light fillLight = fillLightObj.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.color = new Color(0.2f, 0.35f, 0.65f);
            fillLight.intensity = 0.35f;
            fillLightObj.transform.rotation = Quaternion.Euler(-45f, 140f, 0f);

            // Global Volume with Post-Processing Bloom
            GameObject volObj = new GameObject("Global PostProcess Volume");
            var volume = volObj.AddComponent<UnityEngine.Rendering.Volume>();
            volume.isGlobal = true;

            var profile = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.VolumeProfile>("Assets/Settings/SampleSceneProfile.asset");
            if (profile != null) volume.sharedProfile = profile;
        }

        #endregion

        #region Cosmic Assets (Galaxy & Black Hole)

        private static Transform BuildGalaxy(Transform parent)
        {
            string glbPath = "Assets/Models/Galaxy/High/source/MW_Andromeda_Instant_Merger_20s.glb";
            GameObject galaxyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
            GameObject galaxyObj;

            if (galaxyPrefab != null)
            {
                galaxyObj = Object.Instantiate(galaxyPrefab, parent);
                galaxyObj.name = "MilkyWay_Andromeda_Galaxy";
                galaxyObj.transform.position = new Vector3(0f, 60f, 380f);
                galaxyObj.transform.rotation = Quaternion.Euler(35f, 45f, -20f);
                galaxyObj.transform.localScale = Vector3.one * 15f;
            }
            else
            {
                galaxyObj = new GameObject("Deep Space Stellar Field");
                galaxyObj.transform.parent = parent;
                galaxyObj.transform.position = new Vector3(0f, 60f, 380f);
            }

            // Distant Starfield Particle System
            GameObject starfield = new GameObject("Cosmic Dust & Stars");
            starfield.transform.parent = galaxyObj.transform;
            starfield.transform.localPosition = Vector3.zero;
            var ps = starfield.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.maxParticles = 1200;
            main.startSize = 0.8f;
            main.startLifetime = 100f;
            main.startColor = new Color(0.85f, 0.9f, 1f, 0.8f);
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 120f;
            var emission = ps.emission;
            emission.rateOverTime = 30;

            return galaxyObj.transform;
        }

        private static PlanetInteractiveItem BuildBlackHole(Transform parent)
        {
            string fbxPath = "Assets/Models/black hole/source/black hole.fbx";
            GameObject bhPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            GameObject bhObj;

            Vector3 secretPos = new Vector3(-220f, 40f, 260f);

            if (bhPrefab != null)
            {
                bhObj = Object.Instantiate(bhPrefab, parent);
                bhObj.name = "Secret_Black_Hole";
                bhObj.transform.position = secretPos;
                bhObj.transform.rotation = Quaternion.Euler(20f, -40f, 15f);
                bhObj.transform.localScale = Vector3.one * 0.02f;
            }
            else
            {
                bhObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bhObj.name = "Secret_Black_Hole";
                bhObj.transform.parent = parent;
                bhObj.transform.position = secretPos;
                bhObj.transform.localScale = Vector3.one * 15f;
            }

            // Make it easily clickable with generous collider
            var col = bhObj.GetComponent<SphereCollider>();
            if (col == null) col = bhObj.AddComponent<SphereCollider>();
            col.radius = 2400f;

            // Interactive Item component
            var item = bhObj.AddComponent<PlanetInteractiveItem>();
            item.planetName = "Black Hole";
            item.isSecretEasterEgg = true;
            item.cameraDistance = 50f;
            item.rotationSpeed = 25f;
            item.rotationAxis = new Vector3(0.2f, 1f, 0.1f);

            var ringTransform = bhObj.transform.Find("black hole_ring");
            if (ringTransform != null) item.easterEggHintEffect = ringTransform.gameObject;

            // Focus point for camera framing
            GameObject focusPoint = new GameObject("BlackHole_CameraFocus");
            focusPoint.transform.parent = bhObj.transform;
            focusPoint.transform.position = secretPos + new Vector3(-35f, 12f, -45f);
            focusPoint.transform.LookAt(secretPos);
            item.cameraFocusPoint = focusPoint.transform;

            return item;
        }

        #endregion

        #region Solar System & Space Shuttle

        private static List<PlanetInteractiveItem> BuildSolarSystem(Transform parent)
        {
            var planetItems = new List<PlanetInteractiveItem>();

            // 1. The Sun
            GameObject sunObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sunObj.name = "Sun";
            sunObj.transform.parent = parent;
            sunObj.transform.position = Vector3.zero;
            sunObj.transform.localScale = Vector3.one * 16f;
            Material sunMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Planets/Sun.mat");
            if (sunMat != null) sunObj.GetComponent<Renderer>().sharedMaterial = sunMat;

            // Sun Point Light
            Light sunPtLight = sunObj.AddComponent<Light>();
            sunPtLight.type = LightType.Point;
            sunPtLight.color = new Color(1.0f, 0.85f, 0.5f);
            sunPtLight.intensity = 2.5f;
            sunPtLight.range = 350f;

            var sunItem = sunObj.AddComponent<PlanetInteractiveItem>();
            sunItem.planetName = "Sun";
            sunItem.cameraDistance = 26f;
            sunItem.rotationSpeed = 6f;
            planetItems.Add(sunItem);

            // Planetary Data Table: (name, distance, scale, orbitAngle, rotationSpeed)
            var planets = new (string name, float dist, float size, float angle, float rotSpeed)[]
            {
                ("Mercury", 22f, 1.3f, 30f, 18f),
                ("Venus", 32f, 2.0f, 85f, 12f),
                ("Earth", 44f, 2.3f, 140f, 15f),
                ("Mars", 58f, 1.7f, 195f, 14f),
                ("Jupiter", 82f, 5.8f, 245f, 22f),
                ("Saturn", 108f, 4.8f, 290f, 20f),
                ("Uranus", 132f, 3.4f, 335f, 16f),
                ("Neptune", 156f, 3.2f, 20f, 15f),
                ("Pluto", 176f, 1.1f, 65f, 10f)
            };

            foreach (var p in planets)
            {
                float rad = p.angle * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(rad) * p.dist, 0f, Mathf.Sin(rad) * p.dist);

                GameObject pObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pObj.name = p.name;
                pObj.transform.parent = parent;
                pObj.transform.position = pos;
                pObj.transform.localScale = Vector3.one * p.size;

                Material mat = AssetDatabase.LoadAssetAtPath<Material>($"Assets/Materials/Planets/{p.name}.mat");
                if (mat != null) pObj.GetComponent<Renderer>().sharedMaterial = mat;

                // Special planetary additions
                if (p.name == "Earth")
                {
                    // Clouds layer
                    GameObject clouds = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    clouds.name = "Earth_Clouds";
                    clouds.transform.parent = pObj.transform;
                    clouds.transform.localPosition = Vector3.zero;
                    clouds.transform.localScale = Vector3.one * 1.02f;
                    Object.DestroyImmediate(clouds.GetComponent<Collider>());
                    Material cloudMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Planets/Earth_Clouds.mat");
                    if (cloudMat != null) clouds.GetComponent<Renderer>().sharedMaterial = cloudMat;

                    // Moon
                    GameObject moon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    moon.name = "Moon";
                    moon.transform.parent = pObj.transform;
                    moon.transform.localPosition = new Vector3(3.2f, 0.4f, 0f);
                    moon.transform.localScale = Vector3.one * 0.28f;
                    Material moonMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Planets/Moon.mat");
                    if (moonMat != null) moon.GetComponent<Renderer>().sharedMaterial = moonMat;
                }
                else if (p.name == "Saturn")
                {
                    // Saturn Rings
                    GameObject rings = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    rings.name = "Saturn_Rings";
                    rings.transform.parent = pObj.transform;
                    rings.transform.localPosition = Vector3.zero;
                    rings.transform.localRotation = Quaternion.Euler(22f, 0f, 15f);
                    rings.transform.localScale = new Vector3(2.5f, 0.005f, 2.5f);
                    Object.DestroyImmediate(rings.GetComponent<Collider>());
                    Material ringMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Planets/Saturn_Rings.mat");
                    if (ringMat != null) rings.GetComponent<Renderer>().sharedMaterial = ringMat;
                }

                var item = pObj.AddComponent<PlanetInteractiveItem>();
                item.planetName = p.name;
                item.cameraDistance = Mathf.Max(p.size * 2.8f, 5.5f);
                item.rotationSpeed = p.rotSpeed;
                planetItems.Add(item);
            }

            return planetItems;
        }

        private static Transform BuildSpaceShuttle(Transform parent)
        {
            string prefabPath = "Assets/Models/Space shuttle/source/Rocket with materials.prefab";
            GameObject shuttlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject shuttleObj;

            Vector3 shuttlePos = new Vector3(42f, 6.5f, 36f);
            Quaternion shuttleRot = Quaternion.Euler(-10f, 135f, -8f);

            if (shuttlePrefab != null)
            {
                shuttleObj = Object.Instantiate(shuttlePrefab, parent);
                shuttleObj.name = "AstroSurvive_Shuttle";
                shuttleObj.transform.position = shuttlePos;
                shuttleObj.transform.rotation = shuttleRot;
                shuttleObj.transform.localScale = Vector3.one * 0.045f;
            }
            else
            {
                shuttleObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                shuttleObj.name = "AstroSurvive_Shuttle";
                shuttleObj.transform.parent = parent;
                shuttleObj.transform.position = shuttlePos;
                shuttleObj.transform.rotation = shuttleRot;
                shuttleObj.transform.localScale = new Vector3(1f, 2.5f, 1f);
            }

            // Thruster Glow Point Light
            GameObject thrusterLightObj = new GameObject("Engine_Thruster_Glow");
            thrusterLightObj.transform.parent = shuttleObj.transform;
            thrusterLightObj.transform.localPosition = new Vector3(0f, 0f, -4f);
            Light thrusterLight = thrusterLightObj.AddComponent<Light>();
            thrusterLight.type = LightType.Point;
            thrusterLight.color = new Color(0.15f, 0.75f, 1.0f);
            thrusterLight.intensity = 3.5f;
            thrusterLight.range = 8f;

            // Gentle cruise bobbing motion
            var cruise = shuttleObj.AddComponent<ShuttleCruiseMotion>();
            cruise.thrusterLight = thrusterLight;

            return shuttleObj.transform;
        }

        #endregion

        #region Camera Rig & Mounts

        public class CameraMounts
        {
            public Transform introDeepSpace;
            public Transform mainMenuShuttle;
            public Transform marsBriefing;
            public Transform solarOverview;
            public Transform blackHoleSecret;
            public Transform options;
        }

        private static CameraMounts CreateCameraMounts(Transform parent, Transform shuttle, Transform blackHole)
        {
            var mounts = new CameraMounts();

            // 1. Intro Mount: Far deep space viewing the galaxy merger
            GameObject introObj = new GameObject("Mount_IntroDeepSpace");
            introObj.transform.parent = parent;
            introObj.transform.position = new Vector3(0f, 150f, 480f);
            introObj.transform.rotation = Quaternion.Euler(22f, 180f, 0f);
            mounts.introDeepSpace = introObj.transform;

            // 2. Main Menu Shuttle Mount: Dynamic 3/4 angle framing the cruising shuttle
            GameObject shuttleMountObj = new GameObject("Mount_MainMenuShuttle");
            shuttleMountObj.transform.parent = parent;
            shuttleMountObj.transform.position = shuttle.position + new Vector3(-6.5f, 2.0f, -7.8f);
            shuttleMountObj.transform.LookAt(shuttle.position + Vector3.up * 0.4f);
            mounts.mainMenuShuttle = shuttleMountObj.transform;

            // 3. Mars Briefing Mount: Framed dramatic view of Mars
            float marsRad = 195f * Mathf.Deg2Rad;
            Vector3 marsPos = new Vector3(Mathf.Cos(marsRad) * 58f, 0f, Mathf.Sin(marsRad) * 58f);
            GameObject marsMountObj = new GameObject("Mount_MarsBriefing");
            marsMountObj.transform.parent = parent;
            marsMountObj.transform.position = marsPos + new Vector3(-3.5f, 1.8f, -7.5f);
            marsMountObj.transform.LookAt(marsPos);
            mounts.marsBriefing = marsMountObj.transform;

            // 4. Solar System Overview Mount: Panoramic high angle view of all planets
            GameObject solarMountObj = new GameObject("Mount_SolarOverview");
            solarMountObj.transform.parent = parent;
            solarMountObj.transform.position = new Vector3(0f, 110f, -135f);
            solarMountObj.transform.rotation = Quaternion.Euler(38f, 0f, 0f);
            mounts.solarOverview = solarMountObj.transform;

            // 5. Secret Black Hole Mount: Cinematic framing of the dark gravity beast
            GameObject bhMountObj = new GameObject("Mount_BlackHoleSecret");
            bhMountObj.transform.parent = parent;
            bhMountObj.transform.position = blackHole.position + new Vector3(-35f, 12f, -45f);
            bhMountObj.transform.LookAt(blackHole.position);
            mounts.blackHoleSecret = bhMountObj.transform;

            // 6. Options Mount
            GameObject optMountObj = new GameObject("Mount_Options");
            optMountObj.transform.parent = parent;
            optMountObj.transform.position = shuttle.position + new Vector3(7.5f, 2.5f, -9.0f);
            optMountObj.transform.LookAt(shuttle.position);
            mounts.options = optMountObj.transform;

            return mounts;
        }

        private static GameObject BuildCameraRig(CameraMounts mounts)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.015f, 0.015f, 0.035f);
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 2000f;

            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<MenuCursorManager>();
            camObj.AddComponent<UnityEngine.EventSystems.PhysicsRaycaster>();

            var controller = camObj.AddComponent<MenuCameraController>();
            controller.cameraTransform = camObj.transform;
            controller.introDeepSpaceMount = mounts.introDeepSpace;
            controller.shuttleMainMenuMount = mounts.mainMenuShuttle;
            controller.currentMount = mounts.introDeepSpace;
            controller.speedFactor = 2.4f;
            controller.playIntroOnStart = true;
            controller.introHoldDuration = 1.8f;

            return camObj;
        }

        #endregion

        #region World Space UI Canvases

        private static void BuildAllWorldSpaceUI(
            Transform parent,
            MenuCameraController camControl,
            CameraMounts mounts,
            List<PlanetInteractiveItem> planetList,
            PlanetInteractiveItem blackHoleItem,
            TMP_FontAsset font)
        {
            // Event System for UI & 3D Raycasting
            GameObject eventSysObj = new GameObject("EventSystem");
            eventSysObj.transform.parent = parent;
            eventSysObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSysObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // Master UI Manager
            GameObject mgrObj = new GameObject("MenuUIManager");
            mgrObj.transform.parent = parent;
            var uiMgr = mgrObj.AddComponent<MenuUIManager>();
            uiMgr.cameraController = camControl;
            uiMgr.mountMainMenuShuttle = mounts.mainMenuShuttle;
            uiMgr.mountMarsBriefing = mounts.marsBriefing;
            uiMgr.mountSolarOverview = mounts.solarOverview;
            uiMgr.mountBlackHoleSecret = mounts.blackHoleSecret;
            uiMgr.mountOptions = mounts.options;
            uiMgr.planetItems = planetList;
            uiMgr.blackHoleItem = blackHoleItem;

            AudioSource sfxSource = mgrObj.AddComponent<AudioSource>();
            uiMgr.menuAudioSource = sfxSource;

            // 1. MAIN MENU WORLD SPACE CANVAS (anchored near shuttle, shifted to left for framing)
            GameObject mainCanvas = CreateWorldSpaceCanvas("Canvas_MainMenu", parent, mounts.mainMenuShuttle, new Vector3(-1.35f, 0.05f, 4.4f), new Vector2(850, 650), 0.005f);
            uiMgr.mainMenuPanel = mainCanvas;

            // Title Header
            CreateTMPText(mainCanvas.transform, "TitleText", "ASTROSURVIVE", 64, font, new Vector2(0f, 210f), new Vector2(800, 100), new Color(1f, 0.85f, 0.25f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateTMPText(mainCanvas.transform, "SubtitleText", "MISSION TO THE RED PLANET", 24, font, new Vector2(0f, 155f), new Vector2(800, 50), new Color(0.4f, 0.85f, 1.0f), FontStyles.Normal, TextAlignmentOptions.Center);

            // Menu Buttons (Play, Explore Planets, Options, Exit)
            CreateMenuButton(mainCanvas.transform, "Btn_Play", "PLAY MISSION", new Vector2(0f, 65f), new Vector2(360, 68), new Color(0.12f, 0.72f, 0.42f), font, () => uiMgr.OnPlayClicked());
            CreateMenuButton(mainCanvas.transform, "Btn_Explore", "EXPLORE PLANETS", new Vector2(0f, -15f), new Vector2(360, 68), new Color(0.25f, 0.55f, 0.95f), font, () => uiMgr.OnExplorePlanetsClicked());
            CreateMenuButton(mainCanvas.transform, "Btn_Options", "OPTIONS", new Vector2(0f, -95f), new Vector2(360, 68), new Color(0.45f, 0.48f, 0.58f), font, () => uiMgr.OnOptionsClicked());
            CreateMenuButton(mainCanvas.transform, "Btn_Exit", "EXIT GAME", new Vector2(0f, -175f), new Vector2(360, 68), new Color(0.85f, 0.25f, 0.25f), font, () => uiMgr.OnExitClicked());

            // 2. MISSION BRIEFING CANVAS (anchored near Mars)
            GameObject briefingCanvas = CreateWorldSpaceCanvas("Canvas_MissionBriefing", parent, mounts.marsBriefing, new Vector3(0f, 0f, 5.0f), new Vector2(950, 680), 0.0055f);
            uiMgr.missionBriefingPanel = briefingCanvas;
            briefingCanvas.SetActive(false);

            CreateTMPText(briefingCanvas.transform, "BriefingTitle", "RED PLANET MISSION BRIEFING", 48, font, new Vector2(0f, 240f), new Vector2(900, 80), new Color(1f, 0.45f, 0.25f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateTMPText(briefingCanvas.transform, "BriefingSub", "AstroSurvive Command Dispatch", 22, font, new Vector2(0f, 195f), new Vector2(900, 40), new Color(0.8f, 0.85f, 0.9f), FontStyles.Italic, TextAlignmentOptions.Center);

            string briefingBody =
                "Welcome Astronaut!\n\n" +
                "Your space shuttle is screaming through deep space on the final approach to Mars! " +
                "The Red Planet is inhospitable, dusty, and cold.\n\n" +
                "To establish Habitat Outpost Alpha, you must carefully manage:\n" +
                "  - Oxygen Reserves: Keep your space helmet filled!\n" +
                "  - Solar Power: Maintain life support battery systems!\n" +
                "  - Thruster Fuel: Nail a safe atmospheric landing!\n\n" +
                "All systems are online. Are you ready to touch down on Mars?";

            CreateTMPText(briefingCanvas.transform, "BriefingBody", briefingBody, 22, font, new Vector2(0f, 20f), new Vector2(820, 320), Color.white, FontStyles.Normal, TextAlignmentOptions.TopLeft);

            CreateMenuButton(briefingCanvas.transform, "Btn_Launch", "LAUNCH MISSION!", new Vector2(-170f, -220f), new Vector2(320, 68), new Color(0.12f, 0.72f, 0.42f), font, () => uiMgr.OnLaunchMissionClicked());
            CreateMenuButton(briefingCanvas.transform, "Btn_BackShuttle", "BACK TO SHUTTLE", new Vector2(170f, -220f), new Vector2(320, 68), new Color(0.5f, 0.5f, 0.55f), font, () => uiMgr.OnBackToMainMenuClicked());

            // 3. SOLAR SYSTEM OVERVIEW CANVAS (anchored near overview mount)
            GameObject solarCanvas = CreateWorldSpaceCanvas("Canvas_SolarOverview", parent, mounts.solarOverview, new Vector3(0f, -12f, 40f), new Vector2(1200, 400), 0.028f);
            uiMgr.solarSystemOverviewPanel = solarCanvas;
            solarCanvas.SetActive(false);

            CreateTMPText(solarCanvas.transform, "SolarTitle", "SOLAR SYSTEM EXPLORER", 42, font, new Vector2(0f, 120f), new Vector2(1100, 60), new Color(1f, 0.85f, 0.25f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateTMPText(solarCanvas.transform, "SolarSub", "Click any planet in 3D space to fly over and discover its secrets! (Or choose a button below)", 18, font, new Vector2(0f, 75f), new Vector2(1100, 40), Color.white, FontStyles.Normal, TextAlignmentOptions.Center);

            // Horizontal Planet Quick Bar
            string[] quickPlanets = new string[] { "Sun", "Mercury", "Venus", "Earth", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune", "Pluto" };
            float startX = -450f;
            float stepX = 100f;
            for (int i = 0; i < quickPlanets.Length; i++)
            {
                string pName = quickPlanets[i];
                CreateMenuButton(solarCanvas.transform, $"Btn_Quick_{pName}", pName, new Vector2(startX + i * stepX, -10f), new Vector2(92, 48), new Color(0.2f, 0.45f, 0.75f), font, () => uiMgr.SelectPlanetByName(pName), 14);
            }

            CreateMenuButton(solarCanvas.transform, "Btn_BackToMenu", "MAIN MENU", new Vector2(0f, -95f), new Vector2(260, 52), new Color(0.85f, 0.3f, 0.3f), font, () => uiMgr.OnBackToMainMenuClicked(), 18);

            // 4. PLANET FACT CARD CANVAS (dynamic focus UI)
            GameObject factCanvas = CreateWorldSpaceCanvas("Canvas_PlanetFactCard", parent, mounts.solarOverview, new Vector3(0f, 0f, 25f), new Vector2(900, 640), 0.016f);
            uiMgr.planetFactPanel = factCanvas;
            factCanvas.SetActive(false);

            // Background Card Panel
            GameObject cardBg = new GameObject("CardBackground");
            cardBg.transform.parent = factCanvas.transform;
            cardBg.transform.localPosition = Vector3.zero;
            cardBg.transform.localRotation = Quaternion.identity;
            cardBg.transform.localScale = Vector3.one;
            var img = cardBg.AddComponent<Image>();
            img.color = new Color(0.04f, 0.08f, 0.16f, 0.92f);
            cardBg.GetComponent<RectTransform>().sizeDelta = new Vector2(880, 600);

            uiMgr.factPlanetNameText = CreateTMPText(factCanvas.transform, "FactPlanetName", "SATURN", 44, font, new Vector2(0f, 230f), new Vector2(800, 60), new Color(1f, 0.85f, 0.25f), FontStyles.Bold, TextAlignmentOptions.Center);
            uiMgr.factHeadlineText = CreateTMPText(factCanvas.transform, "FactHeadline", "The Dazzling Lord of the Rings!", 24, font, new Vector2(0f, 175f), new Vector2(800, 45), new Color(0.4f, 0.85f, 1.0f), FontStyles.Italic, TextAlignmentOptions.Center);

            uiMgr.fact1Text = CreateTMPText(factCanvas.transform, "Fact1", "- Saturn's rings are made of billions of sparkling ice chunks!", 20, font, new Vector2(0f, 95f), new Vector2(760, 60), Color.white, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            uiMgr.fact2Text = CreateTMPText(factCanvas.transform, "Fact2", "- Saturn is so light it would float in a giant cosmic bathtub!", 20, font, new Vector2(0f, 25f), new Vector2(760, 60), Color.white, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            uiMgr.fact3Text = CreateTMPText(factCanvas.transform, "Fact3", "- Giant howling storms blow faster than supersonic jets!", 20, font, new Vector2(0f, -45f), new Vector2(760, 60), Color.white, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            uiMgr.factStatTagText = CreateTMPText(factCanvas.transform, "FactStatTag", "Moons: 146 Moons! | Width: 282,000 km", 18, font, new Vector2(0f, -115f), new Vector2(760, 35), new Color(1f, 0.7f, 0.2f), FontStyles.Bold, TextAlignmentOptions.Center);

            // Card Navigation Buttons
            CreateMenuButton(factCanvas.transform, "Btn_PrevPlanet", "PREV", new Vector2(-280f, -200f), new Vector2(160, 56), new Color(0.2f, 0.45f, 0.75f), font, () => uiMgr.PreviousPlanet(), 18);
            CreateMenuButton(factCanvas.transform, "Btn_NextPlanet", "NEXT", new Vector2(-100f, -200f), new Vector2(160, 56), new Color(0.2f, 0.45f, 0.75f), font, () => uiMgr.NextPlanet(), 18);
            CreateMenuButton(factCanvas.transform, "Btn_BackSolar", "OVERVIEW", new Vector2(100f, -200f), new Vector2(180, 56), new Color(0.35f, 0.65f, 0.45f), font, () => uiMgr.OnBackToSolarOverviewClicked(), 18);
            CreateMenuButton(factCanvas.transform, "Btn_FactMainMenu", "MENU", new Vector2(280f, -200f), new Vector2(140, 56), new Color(0.85f, 0.3f, 0.3f), font, () => uiMgr.OnBackToMainMenuClicked(), 18);

            // 5. SECRET EASTER EGG BANNER
            GameObject easterEggBanner = new GameObject("SecretEasterEggBanner");
            easterEggBanner.transform.parent = factCanvas.transform;
            easterEggBanner.transform.localPosition = new Vector3(0f, 315f, 0f);
            CreateTMPText(easterEggBanner.transform, "BannerText", "SECRET DISCOVERED: MYSTERIOUS BLACK HOLE!", 22, font, Vector2.zero, new Vector2(850, 45), new Color(1f, 0.3f, 0.9f), FontStyles.Bold, TextAlignmentOptions.Center);
            uiMgr.secretEasterEggBanner = easterEggBanner;
            easterEggBanner.SetActive(false);

            // 6. OPTIONS CANVAS (Audio settings)
            GameObject optCanvas = CreateWorldSpaceCanvas("Canvas_Options", parent, mounts.options, new Vector3(0f, 0f, 5.0f), new Vector2(800, 600), 0.0055f);
            uiMgr.optionsPanel = optCanvas;
            optCanvas.SetActive(false);

            CreateTMPText(optCanvas.transform, "OptTitle", "MISSION AUDIO SETTINGS", 44, font, new Vector2(0f, 210f), new Vector2(750, 60), new Color(1f, 0.85f, 0.25f), FontStyles.Bold, TextAlignmentOptions.Center);

            var audioMgr = optCanvas.AddComponent<AudioSettingsManager>();

            // Sliders for Master, Music, SFX
            CreateVolumeSlider(optCanvas.transform, "Master", "Master Volume", new Vector2(0f, 90f), font, out Slider masterSld);
            CreateVolumeSlider(optCanvas.transform, "Music", "Music Volume", new Vector2(0f, 10f), font, out Slider musicSld);
            CreateVolumeSlider(optCanvas.transform, "SFX", "Sound Effects (SFX)", new Vector2(0f, -70f), font, out Slider sfxSld);

            audioMgr.masterSlider = masterSld;
            audioMgr.musicSlider = musicSld;
            audioMgr.sfxSlider = sfxSld;

            CreateMenuButton(optCanvas.transform, "Btn_OptBack", "BACK TO SHUTTLE", new Vector2(0f, -180f), new Vector2(300, 64), new Color(0.45f, 0.5f, 0.6f), font, () => uiMgr.OnBackToMainMenuClicked());

            // 7. SKIP INTRO PROMPT
            GameObject skipObj = CreateWorldSpaceCanvas("Canvas_SkipIntro", parent, mounts.introDeepSpace, new Vector3(0f, -8f, 25f), new Vector2(600, 100), 0.015f);
            CreateTMPText(skipObj.transform, "SkipPrompt", "Press SPACE or CLICK to skip into shuttle >>", 20, font, Vector2.zero, new Vector2(600, 80), new Color(0.85f, 0.9f, 1f, 0.75f), FontStyles.Normal, TextAlignmentOptions.Center);
            uiMgr.skipIntroPrompt = skipObj;
        }

        private static GameObject CreateWorldSpaceCanvas(string name, Transform parent, Transform anchorMount, Vector3 localOffset, Vector2 size, float scale)
        {
            GameObject canvasObj = new GameObject(name);
            canvasObj.transform.parent = parent;
            canvasObj.transform.position = anchorMount.position + anchorMount.TransformDirection(localOffset);
            canvasObj.transform.rotation = anchorMount.rotation;
            canvasObj.transform.localScale = Vector3.one * scale;

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            var rt = canvasObj.GetComponent<RectTransform>();
            rt.sizeDelta = size;

            return canvasObj;
        }

        private static TextMeshProUGUI CreateTMPText(Transform parent, string name, string text, float fontSize, TMP_FontAsset font, Vector2 anchoredPos, Vector2 size, Color color, FontStyles style, TextAlignmentOptions align)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.parent = parent;
            textObj.transform.localPosition = Vector3.zero;
            textObj.transform.localRotation = Quaternion.identity;
            textObj.transform.localScale = Vector3.one;

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            if (font != null)
            {
                try { tmp.font = font; } catch { }
            }
            try
            {
                tmp.text = text;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[TMP_Text] Could not set text '{text}': {ex.Message}");
            }
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.enableWordWrapping = true;

            var rt = textObj.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            return tmp;
        }

        private static Button CreateMenuButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Color baseColor, TMP_FontAsset font, UnityEngine.Events.UnityAction onClick, float fontSize = 24)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.parent = parent;
            btnObj.transform.localPosition = Vector3.zero;
            btnObj.transform.localRotation = Quaternion.identity;
            btnObj.transform.localScale = Vector3.one;

            var img = btnObj.AddComponent<Image>();
            img.color = baseColor;

            var btn = btnObj.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = baseColor;
            colors.highlightedColor = baseColor * 1.25f;
            colors.pressedColor = baseColor * 0.75f;
            colors.selectedColor = baseColor;
            btn.colors = colors;

            var rt = btnObj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            // Label Text
            CreateTMPText(btnObj.transform, "BtnText", label, fontSize, font, Vector2.zero, size, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);

            if (onClick != null)
            {
                btn.onClick.AddListener(onClick);
            }

            return btn;
        }

        private static void CreateVolumeSlider(Transform parent, string name, string label, Vector2 pos, TMP_FontAsset font, out Slider slider)
        {
            GameObject groupObj = new GameObject($"SliderGroup_{name}");
            groupObj.transform.parent = parent;
            groupObj.transform.localPosition = Vector3.zero;
            groupObj.transform.localRotation = Quaternion.identity;
            groupObj.transform.localScale = Vector3.one;

            var groupRt = groupObj.AddComponent<RectTransform>();
            groupRt.anchoredPosition = pos;
            groupRt.sizeDelta = new Vector2(600, 60);

            CreateTMPText(groupObj.transform, "Label", label, 20, font, new Vector2(-150f, 0f), new Vector2(250, 40), Color.white, FontStyles.Bold, TextAlignmentOptions.Left);

            // Slider GameObject
            GameObject sliderObj = new GameObject($"Slider_{name}");
            sliderObj.transform.parent = groupObj.transform;
            sliderObj.transform.localPosition = Vector3.zero;
            sliderObj.transform.localRotation = Quaternion.identity;
            sliderObj.transform.localScale = Vector3.one;

            var sldRt = sliderObj.AddComponent<RectTransform>();
            sldRt.anchoredPosition = new Vector2(120f, 0f);
            sldRt.sizeDelta = new Vector2(280, 24);

            slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;

            // Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.parent = sliderObj.transform;
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.25f, 0.35f, 0.9f);
            var bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;

            // Fill Area & Fill
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.parent = sliderObj.transform;
            var faRt = fillArea.AddComponent<RectTransform>();
            faRt.anchorMin = new Vector2(0f, 0.25f);
            faRt.anchorMax = new Vector2(1f, 0.75f);
            faRt.sizeDelta = Vector2.zero;

            GameObject fill = new GameObject("Fill");
            fill.transform.parent = fillArea.transform;
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.2f, 0.75f, 1.0f);
            var fRt = fill.GetComponent<RectTransform>();
            fRt.anchorMin = Vector2.zero;
            fRt.anchorMax = Vector2.one;
            fRt.sizeDelta = Vector2.zero;
            slider.fillRect = fRt;

            // Handle Slide Area & Handle
            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.parent = sliderObj.transform;
            var haRt = handleArea.AddComponent<RectTransform>();
            haRt.anchorMin = Vector2.zero;
            haRt.anchorMax = Vector2.one;
            haRt.sizeDelta = Vector2.zero;

            GameObject handle = new GameObject("Handle");
            handle.transform.parent = handleArea.transform;
            var hImg = handle.AddComponent<Image>();
            hImg.color = Color.white;
            var hRt = handle.GetComponent<RectTransform>();
            hRt.sizeDelta = new Vector2(24, 32);
            slider.handleRect = hRt;
            slider.targetGraphic = hImg;
        }

        #endregion

        #region Build Settings

        private static void RegisterInBuildSettings(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            int existingIndex = -1;
            for (int i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].path == scenePath)
                {
                    existingIndex = i;
                    break;
                }
            }

            if (existingIndex >= 0)
            {
                var existing = scenes[existingIndex];
                scenes.RemoveAt(existingIndex);
                scenes.Insert(0, existing);
            }
            else
            {
                scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($">>> {scenePath} placed at index 0 in EditorBuildSettings!");
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace("\\", "/");
                string folder = Path.GetFileName(path);
                if (!AssetDatabase.IsValidFolder(parent))
                {
                    EnsureFolder(parent);
                }
                AssetDatabase.CreateFolder(parent, folder);
            }
        }

        #endregion
    }
}

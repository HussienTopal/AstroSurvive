using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using AstroSurvive.Player;
using AstroSurvive.Interaction;
using AstroSurvive.UI;

namespace AstroSurvive.Editor
{
    public static class MarsSceneGenerator
    {
        [MenuItem("AstroSurvive/Generate Topal_TestScene")]
        public static void GenerateScene()
        {
            Debug.Log(">>> Starting Mars Topal_TestScene generation...");

            // 1. Create a fresh scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Ensure Asset folders exist
            EnsureFolder("Assets/Materials");
            EnsureFolder("Assets/Scenes");

            // 3. Create or load materials with URP Lit shader
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            Material sandMat = GetOrCreateMaterial("Assets/Materials/Mars_Sand.mat", litShader, new Color(0.76f, 0.34f, 0.16f), 0.1f, 0.2f);
            Material rockMat = GetOrCreateMaterial("Assets/Materials/Mars_Rock.mat", litShader, new Color(0.42f, 0.19f, 0.11f), 0.15f, 0.2f);
            Material habMat = GetOrCreateMaterial("Assets/Materials/Habitat_Hull.mat", litShader, new Color(0.88f, 0.90f, 0.92f), 0.75f, 0.65f);
            Material frameMat = GetOrCreateMaterial("Assets/Materials/Habitat_DarkFrame.mat", litShader, new Color(0.18f, 0.20f, 0.24f), 0.85f, 0.5f);
            Material solarMat = GetOrCreateMaterial("Assets/Materials/Solar_Panel.mat", litShader, new Color(0.06f, 0.15f, 0.26f), 0.9f, 0.85f);
            Material oxygenMat = GetOrCreateMaterial("Assets/Materials/Oxygen_Tank.mat", litShader, new Color(0.12f, 0.52f, 0.85f), 0.4f, 0.6f);
            Material crystalMat = GetOrCreateMaterial("Assets/Materials/Mars_Crystal.mat", litShader, new Color(0.95f, 0.3f, 0.15f), 0.2f, 0.9f);
            Material glowMat = GetOrCreateMaterial("Assets/Materials/Beacon_Glow.mat", litShader, new Color(0.1f, 0.85f, 1.0f), 0.1f, 0.9f);
            glowMat.EnableKeyword("_EMISSION");
            glowMat.SetColor("_EmissionColor", new Color(0.1f, 0.85f, 1.0f) * 2.2f);

            // 4. Configure Mars Lighting & Atmosphere
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.82f, 0.42f, 0.22f);
            RenderSettings.ambientEquatorColor = new Color(0.55f, 0.28f, 0.15f);
            RenderSettings.ambientGroundColor = new Color(0.25f, 0.12f, 0.06f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.78f, 0.38f, 0.20f);
            RenderSettings.fogDensity = 0.014f;

            // Mars Sun (Directional Light)
            GameObject sunObj = new GameObject("Mars Sun");
            Light sunLight = sunObj.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = new Color(1.0f, 0.86f, 0.74f);
            sunLight.intensity = 1.15f;
            sunLight.shadows = LightShadows.Soft;
            sunObj.transform.rotation = Quaternion.Euler(38f, -42f, 0f);

            // Hierarchy Root
            GameObject envRoot = new GameObject("--- MARS ENVIRONMENT ---");

            // 5. Build Martian Terrain Surface
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Mars Surface Ground";
            ground.transform.parent = envRoot.transform;
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(18f, 1f, 18f); // 180m x 180m
            ground.GetComponent<Renderer>().sharedMaterial = sandMat;

            // Dunes & Crater Rims
            BuildCraterRim(envRoot.transform, sandMat, rockMat);

            // Scattered Martian Boulders & Craters
            BuildBoulders(envRoot.transform, rockMat);

            // 6. Build Habitat Base (Hab-Alpha)
            GameObject outpostRoot = new GameObject("--- HABITAT OUTPOST ALPHA ---");
            BuildHabitatModule(outpostRoot.transform, habMat, frameMat, oxygenMat, glowMat);

            // 7. Interactive Objects
            // A. Habitat Airlock Door
            BuildAirlockSystem(outpostRoot.transform, frameMat, glowMat);

            // B. Oxygen Refill Dispenser Station
            BuildOxygenStation(outpostRoot.transform, oxygenMat, frameMat, glowMat);

            // C. Solar Power Terminal
            BuildSolarArray(outpostRoot.transform, solarMat, frameMat, glowMat);

            // D. Geological Martian Core Samples (Scattered outside in the crater)
            BuildGeologicalSamples(envRoot.transform, crystalMat);

            // E. Datapad Lore Log on crate
            BuildResearchDatapad(outpostRoot.transform, frameMat);

            // 8. Build Player
            BuildPlayer(new Vector3(0f, 1f, -12f));

            // 9. Save Scene
            string scenePath = "Assets/Scenes/Topal_TestScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($">>> Successfully generated and saved Topal_TestScene to: {scenePath}");

            // 10. Register scene in EditorBuildSettings
            RegisterSceneInBuildSettings(scenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void BuildHabitatModule(Transform parent, Material habMat, Material frameMat, Material o2Mat, Material glowMat)
        {
            GameObject hab = new GameObject("Habitat Central Dome");
            hab.transform.parent = parent;
            hab.transform.position = new Vector3(0f, 0f, 0f);

            // Main Dome Structure
            GameObject dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dome.name = "Hab Dome Shell";
            dome.transform.parent = hab.transform;
            dome.transform.position = new Vector3(0f, 0.5f, 0f);
            dome.transform.localScale = new Vector3(14f, 7.5f, 14f);
            dome.GetComponent<Renderer>().sharedMaterial = habMat;

            // Habitat Foundation Ring
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Foundation Base";
            ring.transform.parent = hab.transform;
            ring.transform.position = new Vector3(0f, 0.4f, 0f);
            ring.transform.localScale = new Vector3(14.5f, 0.4f, 14.5f);
            ring.GetComponent<Renderer>().sharedMaterial = frameMat;

            // Communications Mast & Beacon
            GameObject mast = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mast.name = "Comm Antenna Mast";
            mast.transform.parent = hab.transform;
            mast.transform.position = new Vector3(0f, 6.5f, 0f);
            mast.transform.localScale = new Vector3(0.25f, 3.5f, 0.25f);
            mast.GetComponent<Renderer>().sharedMaterial = frameMat;

            GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            beacon.name = "Beacon Light Sphere";
            beacon.transform.parent = mast.transform;
            beacon.transform.position = new Vector3(0f, 10.2f, 0f);
            beacon.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            beacon.GetComponent<Renderer>().sharedMaterial = glowMat;

            Light beaconLight = beacon.AddComponent<Light>();
            beaconLight.type = LightType.Point;
            beaconLight.color = new Color(0.1f, 0.85f, 1.0f);
            beaconLight.intensity = 2.5f;
            beaconLight.range = 28f;

            // Exterior Storage Tanks
            for (int i = 0; i < 3; i++)
            {
                GameObject tank = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                tank.name = $"Auxiliary Tank {i + 1}";
                tank.transform.parent = hab.transform;
                float angle = (i * 22f + 50f) * Mathf.Deg2Rad;
                tank.transform.position = new Vector3(Mathf.Sin(angle) * 7.5f, 1.4f, Mathf.Cos(angle) * 7.5f);
                tank.transform.localScale = new Vector3(1f, 1.3f, 1f);
                tank.GetComponent<Renderer>().sharedMaterial = o2Mat;
            }
        }

        private static void BuildAirlockSystem(Transform parent, Material frameMat, Material glowMat)
        {
            GameObject airlockRoot = new GameObject("Interactive Airlock System");
            airlockRoot.transform.parent = parent;
            airlockRoot.transform.position = new Vector3(0f, 0f, -6.8f);

            // Corridor frame
            GameObject corridor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            corridor.name = "Airlock Vestibule";
            corridor.transform.parent = airlockRoot.transform;
            corridor.transform.position = new Vector3(0f, 1.7f, 0f);
            corridor.transform.localScale = new Vector3(3.6f, 3.4f, 2.8f);
            corridor.GetComponent<Renderer>().sharedMaterial = frameMat;

            // Door Frame Cutout simulation (Left and Right pillars)
            GameObject pillarL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillarL.transform.parent = airlockRoot.transform;
            pillarL.transform.position = new Vector3(-1.3f, 1.5f, -1.4f);
            pillarL.transform.localScale = new Vector3(0.6f, 3.0f, 0.4f);
            pillarL.GetComponent<Renderer>().sharedMaterial = frameMat;

            GameObject pillarR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillarR.transform.parent = airlockRoot.transform;
            pillarR.transform.position = new Vector3(1.3f, 1.5f, -1.4f);
            pillarR.transform.localScale = new Vector3(0.6f, 3.0f, 0.4f);
            pillarR.GetComponent<Renderer>().sharedMaterial = frameMat;

            // Sliding Door Panel
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Sliding Hatch Door";
            door.transform.parent = airlockRoot.transform;
            door.transform.position = new Vector3(0f, 1.4f, -1.4f);
            door.transform.localScale = new Vector3(2.0f, 2.7f, 0.25f);
            door.GetComponent<Renderer>().sharedMaterial = frameMat;

            // Status Indicator Light
            GameObject lightObj = new GameObject("Airlock Indicator Light");
            lightObj.transform.parent = airlockRoot.transform;
            lightObj.transform.position = new Vector3(0f, 2.9f, -1.5f);
            Light indLight = lightObj.AddComponent<Light>();
            indLight.type = LightType.Point;
            indLight.color = new Color(1f, 0.2f, 0.1f);
            indLight.range = 5f;
            indLight.intensity = 1.8f;

            // Attach MarsAirlock script
            MarsAirlock airlock = airlockRoot.AddComponent<MarsAirlock>();
            airlock.doorPanel = door.transform;
            airlock.openOffset = new Vector3(0f, 2.8f, 0f);
            airlock.indicatorLight = indLight;
        }

        private static void BuildOxygenStation(Transform parent, Material o2Mat, Material frameMat, Material glowMat)
        {
            GameObject station = new GameObject("Oxygen Refill Station");
            station.transform.parent = parent;
            station.transform.position = new Vector3(-3.8f, 0f, -8f);

            // Base stand
            GameObject stand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stand.transform.parent = station.transform;
            stand.transform.position = new Vector3(0f, 0.8f, 0f);
            stand.transform.localScale = new Vector3(1.2f, 1.6f, 0.8f);
            stand.GetComponent<Renderer>().sharedMaterial = frameMat;

            // Dispenser Cylinder
            GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cyl.transform.parent = station.transform;
            cyl.transform.position = new Vector3(0f, 1.6f, 0f);
            cyl.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            cyl.GetComponent<Renderer>().sharedMaterial = o2Mat;

            // Status Light
            GameObject lightObj = new GameObject("O2 Status Light");
            lightObj.transform.parent = station.transform;
            lightObj.transform.position = new Vector3(0f, 2.1f, -0.4f);
            Light statusLight = lightObj.AddComponent<Light>();
            statusLight.type = LightType.Point;
            statusLight.color = new Color(0.2f, 0.85f, 1f);
            statusLight.range = 4f;
            statusLight.intensity = 1.5f;

            // Script
            var o2Comp = station.AddComponent<OxygenStation>();
            o2Comp.statusLight = statusLight;
            o2Comp.refillAmount = 45f;
        }

        private static void BuildSolarArray(Transform parent, Material solarMat, Material frameMat, Material glowMat)
        {
            GameObject solarRoot = new GameObject("Solar Power Terminal Array");
            solarRoot.transform.parent = parent;
            solarRoot.transform.position = new Vector3(9f, 0f, -4f);

            // Pole
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.transform.parent = solarRoot.transform;
            pole.transform.position = new Vector3(0f, 1.8f, 0f);
            pole.transform.localScale = new Vector3(0.3f, 1.8f, 0.3f);
            pole.GetComponent<Renderer>().sharedMaterial = frameMat;

            // Panels frame
            GameObject panelHolder = new GameObject("Solar Panels");
            panelHolder.transform.parent = solarRoot.transform;
            panelHolder.transform.position = new Vector3(0f, 3.4f, 0f);
            panelHolder.transform.rotation = Quaternion.Euler(35f, -30f, 0f);

            GameObject panel1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel1.transform.parent = panelHolder.transform;
            panel1.transform.localPosition = new Vector3(-2.2f, 0f, 0f);
            panel1.transform.localScale = new Vector3(3.8f, 0.12f, 2.2f);
            panel1.GetComponent<Renderer>().sharedMaterial = solarMat;

            GameObject panel2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel2.transform.parent = panelHolder.transform;
            panel2.transform.localPosition = new Vector3(2.2f, 0f, 0f);
            panel2.transform.localScale = new Vector3(3.8f, 0.12f, 2.2f);
            panel2.GetComponent<Renderer>().sharedMaterial = solarMat;

            // Control pedestal
            GameObject terminal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            terminal.name = "Solar Terminal Console";
            terminal.transform.parent = solarRoot.transform;
            terminal.transform.position = new Vector3(0f, 0.65f, -1.8f);
            terminal.transform.localScale = new Vector3(0.9f, 1.3f, 0.6f);
            terminal.GetComponent<Renderer>().sharedMaterial = frameMat;

            GameObject floodlightObj = new GameObject("Array Floodlight");
            floodlightObj.transform.parent = solarRoot.transform;
            floodlightObj.transform.position = new Vector3(0f, 3.6f, 0.5f);
            Light floodlight = floodlightObj.AddComponent<Light>();
            floodlight.type = LightType.Spot;
            floodlight.color = new Color(0.9f, 0.95f, 1f);
            floodlight.range = 22f;
            floodlight.spotAngle = 60f;
            floodlight.intensity = 2.0f;
            floodlightObj.transform.rotation = Quaternion.Euler(55f, 150f, 0f);

            var solarComp = terminal.AddComponent<SolarTerminal>();
            solarComp.solarPanels = panelHolder.transform;
            solarComp.outpostBeacon = floodlight;
        }

        private static void BuildGeologicalSamples(Transform parent, Material crystalMat)
        {
            Vector3[] samplePositions = new Vector3[]
            {
                new Vector3(-6f, 0.35f, -14f),
                new Vector3(7.5f, 0.4f, -16f),
                new Vector3(-14f, 0.5f, -8f),
                new Vector3(12f, 0.45f, 5f)
            };

            string[] types = new string[]
            {
                "Hematite Spherule (Blueberry)",
                "Olivine Volcanic Matrix",
                "Hydrated Silica Crust",
                "Subsurface Ice-Bearing Perchlorate"
            };

            for (int i = 0; i < samplePositions.Length; i++)
            {
                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = $"Martian Geological Sample [{i + 1}]";
                rock.transform.parent = parent;
                rock.transform.position = samplePositions[i];
                rock.transform.localScale = new Vector3(0.55f, 0.4f, 0.5f);
                rock.GetComponent<Renderer>().sharedMaterial = crystalMat;

                var sample = rock.AddComponent<MarsSampleRock>();
                sample.sampleType = types[i % types.Length];
                sample.scientificValue = 20f + (i * 10f);
            }
        }

        private static void BuildResearchDatapad(Transform parent, Material frameMat)
        {
            // Cargo crate
            GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crate.name = "Expedition Supply Crate";
            crate.transform.parent = parent;
            crate.transform.position = new Vector3(3.5f, 0.5f, -8.5f);
            crate.transform.localScale = new Vector3(1.4f, 1.0f, 1.4f);
            crate.GetComponent<Renderer>().sharedMaterial = frameMat;

            // Datapad resting on crate
            GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pad.name = "Mission Datapad #042";
            pad.transform.parent = crate.transform;
            pad.transform.localPosition = new Vector3(0f, 0.54f, 0f);
            pad.transform.localScale = new Vector3(0.4f, 0.04f, 0.6f);
            pad.transform.localRotation = Quaternion.Euler(0f, 25f, 0f);

            var padComp = pad.AddComponent<ResearchDatapad>();
            padComp.logTitle = "ARES EXPEDITION - SOL 114 LOG";
            padComp.logContent = "EVA suit telemetry calibrated for 0.38G surface gravity. Habitat airlock cycling confirmed nominal. Collect geological core samples across the Jezero perimeter and monitor O2 reserves.";
        }

        private static void BuildCraterRim(Transform parent, Material sandMat, Material rockMat)
        {
            // Create a ring of mounds surrounding the outpost area
            int segments = 16;
            float radius = 45f;
            for (int i = 0; i < segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                Vector3 pos = new Vector3(Mathf.Sin(angle) * radius, 1.5f, Mathf.Cos(angle) * radius);

                GameObject ridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ridge.name = $"Crater Rim Ridge {i + 1}";
                ridge.transform.parent = parent;
                ridge.transform.position = pos;
                ridge.transform.rotation = Quaternion.Euler(Random.Range(-10f, 10f), angle * Mathf.Rad2Deg + 90f, Random.Range(-10f, 10f));
                ridge.transform.localScale = new Vector3(Random.Range(16f, 24f), Random.Range(3.5f, 6.0f), Random.Range(8f, 14f));
                ridge.GetComponent<Renderer>().sharedMaterial = (i % 2 == 0) ? rockMat : sandMat;
            }
        }

        private static void BuildBoulders(Transform parent, Material rockMat)
        {
            int boulderCount = 28;
            for (int i = 0; i < boulderCount; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * 36f;
                // Avoid placing boulders directly inside the habitat circle (radius 10m)
                if (randomCircle.magnitude < 11f)
                {
                    randomCircle = randomCircle.normalized * 12f;
                }

                GameObject boulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                boulder.name = $"Martian Boulder {i + 1}";
                boulder.transform.parent = parent;
                boulder.transform.position = new Vector3(randomCircle.x, Random.Range(0.2f, 0.6f), randomCircle.y);
                boulder.transform.rotation = Random.rotation;
                boulder.transform.localScale = new Vector3(
                    Random.Range(1.2f, 3.2f),
                    Random.Range(0.8f, 2.2f),
                    Random.Range(1.2f, 3.2f)
                );
                boulder.GetComponent<Renderer>().sharedMaterial = rockMat;
            }
        }

        private static void BuildPlayer(Vector3 spawnPosition)
        {
            GameObject playerObj = new GameObject("Player");
            playerObj.tag = "Player";
            playerObj.transform.position = spawnPosition;

            // Character Controller
            var cc = playerObj.AddComponent<CharacterController>();
            cc.height = 2.0f;
            cc.radius = 0.45f;
            cc.center = new Vector3(0f, 1.0f, 0f);
            cc.stepOffset = 0.4f;

            // Camera Holder & Main Camera
            GameObject camObj = new GameObject("Main Camera");
            camObj.transform.parent = playerObj.transform;
            camObj.transform.localPosition = new Vector3(0f, 1.65f, 0f);
            camObj.transform.localRotation = Quaternion.identity;
            camObj.tag = "MainCamera";

            Camera cam = camObj.AddComponent<Camera>();
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 500f;
            cam.fieldOfView = 75f;

            camObj.AddComponent<AudioListener>();

            // Scripts
            var controller = playerObj.AddComponent<PlayerController>();
            var interaction = playerObj.AddComponent<PlayerInteraction>();
            playerObj.AddComponent<MarsHUD>();
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace("\\", "/");
                string folder = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }

        private static Material GetOrCreateMaterial(string path, Shader shader, Color color, float metallic, float smoothness)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                mat.color = color;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        private static void RegisterSceneInBuildSettings(string scenePath)
        {
            var currentScenes = EditorBuildSettings.scenes;
            foreach (var s in currentScenes)
            {
                if (s.path == scenePath) return;
            }

            var newScenes = new EditorBuildSettingsScene[currentScenes.Length + 1];
            for (int i = 0; i < currentScenes.Length; i++)
            {
                newScenes[i] = currentScenes[i];
            }
            newScenes[newScenes.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = newScenes;
        }
    }
}

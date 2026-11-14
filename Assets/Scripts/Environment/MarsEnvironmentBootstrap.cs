using UnityEngine;
using AstroSurvive.Player;
using AstroSurvive.Interaction;
using AstroSurvive.UI;
using AstroSurvive.Core;

namespace AstroSurvive.Environment
{
    public class MarsEnvironmentBootstrap : MonoBehaviour
    {
        [Header("Auto Setup")]
        public bool buildOnStart = false;
        [SerializeField] private bool hasGenerated = true;

        private void Start()
        {
            // All objects are directly in the scene hierarchy for full visual editability.
        }

        private void EnsureCoreManagers()
        {
            if (FindFirstObjectByType<SolGameManager>() == null)
            {
                GameObject gm = new GameObject("--- SolGameManager ---");
                gm.AddComponent<SolGameManager>();
            }

            if (FindFirstObjectByType<NasaEventManager>() == null)
            {
                GameObject nm = new GameObject("--- NasaEventManager ---");
                nm.AddComponent<NasaEventManager>();
            }
        }

        [ContextMenu("Regenerate Mars World")]
        public void GenerateMarsWorld()
        {
            // Remove previous generated environment child if any
            var existing = transform.Find("Generated_Mars_Surface");
            if (existing != null)
            {
                if (Application.isPlaying) Destroy(existing.gameObject);
                else DestroyImmediate(existing.gameObject);
            }

            GameObject root = new GameObject("Generated_Mars_Surface");
            root.transform.parent = transform;
            root.transform.localPosition = Vector3.zero;

            // 1. Atmosphere & Fog setup
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.78f, 0.38f, 0.20f);
            RenderSettings.fogDensity = 0.012f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.85f, 0.44f, 0.24f);
            RenderSettings.ambientEquatorColor = new Color(0.58f, 0.30f, 0.16f);
            RenderSettings.ambientGroundColor = new Color(0.28f, 0.14f, 0.07f);

            // Shaders & Materials
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            Material sandMat = CreateRuntimeMat(litShader, new Color(0.78f, 0.35f, 0.16f), 0.1f, 0.2f);
            Material rockMat = CreateRuntimeMat(litShader, new Color(0.44f, 0.20f, 0.12f), 0.15f, 0.2f);
            Material habMat = CreateRuntimeMat(litShader, new Color(0.88f, 0.90f, 0.94f), 0.75f, 0.65f);
            Material frameMat = CreateRuntimeMat(litShader, new Color(0.18f, 0.20f, 0.25f), 0.85f, 0.5f);
            Material solarMat = CreateRuntimeMat(litShader, new Color(0.06f, 0.16f, 0.28f), 0.9f, 0.85f);
            Material o2Mat = CreateRuntimeMat(litShader, new Color(0.12f, 0.55f, 0.88f), 0.4f, 0.6f);
            Material crystalMat = CreateRuntimeMat(litShader, new Color(0.96f, 0.32f, 0.16f), 0.2f, 0.9f);
            Material glowMat = CreateRuntimeMat(litShader, new Color(0.1f, 0.85f, 1.0f), 0.1f, 0.9f);
            glowMat.EnableKeyword("_EMISSION");
            glowMat.SetColor("_EmissionColor", new Color(0.1f, 0.85f, 1.0f) * 2.0f);

            // 2. Main Ground
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Martian Crust Surface";
            ground.transform.parent = root.transform;
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(16f, 1f, 16f);
            ground.GetComponent<Renderer>().sharedMaterial = sandMat;

            // 3. Crater Rims
            int rimSegments = 16;
            float rimRadius = 42f;
            for (int i = 0; i < rimSegments; i++)
            {
                float angle = (i / (float)rimSegments) * Mathf.PI * 2f;
                Vector3 pos = new Vector3(Mathf.Sin(angle) * rimRadius, 1.2f, Mathf.Cos(angle) * rimRadius);
                GameObject mound = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mound.name = $"Crater Ridge {i + 1}";
                mound.transform.parent = root.transform;
                mound.transform.position = pos;
                mound.transform.rotation = Quaternion.Euler(0f, angle * Mathf.Rad2Deg + 90f, 0f);
                mound.transform.localScale = new Vector3(18f, 3.8f, 9f);
                mound.GetComponent<Renderer>().sharedMaterial = (i % 2 == 0) ? rockMat : sandMat;
            }

            // 4. Scattered Boulders
            Random.InitState(42);
            for (int i = 0; i < 26; i++)
            {
                Vector2 circle = Random.insideUnitCircle * 32f;
                if (circle.magnitude < 10f) circle = circle.normalized * 11.5f;

                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = $"Mars Boulder {i + 1}";
                rock.transform.parent = root.transform;
                rock.transform.position = new Vector3(circle.x, Random.Range(0.2f, 0.5f), circle.y);
                rock.transform.rotation = Random.rotation;
                rock.transform.localScale = new Vector3(Random.Range(1.3f, 3.0f), Random.Range(0.8f, 2.0f), Random.Range(1.3f, 3.0f));
                rock.GetComponent<Renderer>().sharedMaterial = rockMat;
            }

            // 5. Habitat Dome Base
            GameObject hab = new GameObject("Habitat Module Alpha");
            hab.transform.parent = root.transform;
            hab.transform.position = Vector3.zero;

            GameObject dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dome.name = "Dome Exterior Shell";
            dome.transform.parent = hab.transform;
            dome.transform.position = new Vector3(0f, 0.6f, 0f);
            dome.transform.localScale = new Vector3(13.5f, 7.2f, 13.5f);
            dome.GetComponent<Renderer>().sharedMaterial = habMat;

            GameObject foundation = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            foundation.name = "Base Foundation Ring";
            foundation.transform.parent = hab.transform;
            foundation.transform.position = new Vector3(0f, 0.35f, 0f);
            foundation.transform.localScale = new Vector3(14.2f, 0.35f, 14.2f);
            foundation.GetComponent<Renderer>().sharedMaterial = frameMat;

            // Beacon Mast
            GameObject mast = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mast.name = "Communications Tower";
            mast.transform.parent = hab.transform;
            mast.transform.position = new Vector3(0f, 6.2f, 0f);
            mast.transform.localScale = new Vector3(0.25f, 3.2f, 0.25f);
            mast.GetComponent<Renderer>().sharedMaterial = frameMat;

            GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            beacon.name = "Tower Beacon";
            beacon.transform.parent = mast.transform;
            beacon.transform.position = new Vector3(0f, 9.6f, 0f);
            beacon.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
            beacon.GetComponent<Renderer>().sharedMaterial = glowMat;

            Light bLight = beacon.AddComponent<Light>();
            bLight.type = LightType.Point;
            bLight.color = new Color(0.1f, 0.85f, 1.0f);
            bLight.intensity = 2.2f;
            bLight.range = 25f;

            // 6. Interactive Airlock Door
            GameObject airlockRoot = new GameObject("Interactive Airlock System");
            airlockRoot.transform.parent = root.transform;
            airlockRoot.transform.position = new Vector3(0f, 0f, -6.6f);

            GameObject corridor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            corridor.name = "Vestibule Frame";
            corridor.transform.parent = airlockRoot.transform;
            corridor.transform.position = new Vector3(0f, 1.6f, 0f);
            corridor.transform.localScale = new Vector3(3.4f, 3.2f, 2.6f);
            corridor.GetComponent<Renderer>().sharedMaterial = frameMat;

            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Sliding Hatch Door";
            door.transform.parent = airlockRoot.transform;
            door.transform.position = new Vector3(0f, 1.35f, -1.35f);
            door.transform.localScale = new Vector3(1.9f, 2.6f, 0.25f);
            door.GetComponent<Renderer>().sharedMaterial = frameMat;

            GameObject indObj = new GameObject("Airlock Light");
            indObj.transform.parent = airlockRoot.transform;
            indObj.transform.position = new Vector3(0f, 2.8f, -1.45f);
            Light airlockLight = indObj.AddComponent<Light>();
            airlockLight.type = LightType.Point;
            airlockLight.color = new Color(1f, 0.25f, 0.1f);
            airlockLight.range = 5f;
            airlockLight.intensity = 1.6f;

            var airlockComp = airlockRoot.AddComponent<MarsAirlock>();
            airlockComp.doorPanel = door.transform;
            airlockComp.openOffset = new Vector3(0f, 2.7f, 0f);
            airlockComp.indicatorLight = airlockLight;

            // 7. Interactive Oxygen Dispenser
            GameObject o2Obj = new GameObject("Interactive Oxygen Station");
            o2Obj.transform.parent = root.transform;
            o2Obj.transform.position = new Vector3(-3.6f, 0f, -7.8f);

            GameObject o2Stand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            o2Stand.transform.parent = o2Obj.transform;
            o2Stand.transform.position = new Vector3(0f, 0.75f, 0f);
            o2Stand.transform.localScale = new Vector3(1.1f, 1.5f, 0.8f);
            o2Stand.GetComponent<Renderer>().sharedMaterial = frameMat;

            GameObject o2Cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            o2Cyl.transform.parent = o2Obj.transform;
            o2Cyl.transform.position = new Vector3(0f, 1.55f, 0f);
            o2Cyl.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            o2Cyl.GetComponent<Renderer>().sharedMaterial = o2Mat;

            GameObject o2LightObj = new GameObject("Status Light");
            o2LightObj.transform.parent = o2Obj.transform;
            o2LightObj.transform.position = new Vector3(0f, 2.0f, -0.35f);
            Light o2Light = o2LightObj.AddComponent<Light>();
            o2Light.type = LightType.Point;
            o2Light.color = new Color(0.2f, 0.85f, 1f);
            o2Light.range = 3.5f;

            var o2Comp = o2Obj.AddComponent<OxygenStation>();
            o2Comp.statusLight = o2Light;
            o2Comp.refillAmount = 50f;

            // 8. Interactive Solar Power Array
            GameObject solarObj = new GameObject("Interactive Solar Array");
            solarObj.transform.parent = root.transform;
            solarObj.transform.position = new Vector3(8.5f, 0f, -4.5f);

            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.transform.parent = solarObj.transform;
            pole.transform.position = new Vector3(0f, 1.7f, 0f);
            pole.transform.localScale = new Vector3(0.25f, 1.7f, 0.25f);
            pole.GetComponent<Renderer>().sharedMaterial = frameMat;

            GameObject panelsHolder = new GameObject("Solar Panels");
            panelsHolder.transform.parent = solarObj.transform;
            panelsHolder.transform.position = new Vector3(0f, 3.3f, 0f);
            panelsHolder.transform.rotation = Quaternion.Euler(32f, -25f, 0f);

            GameObject p1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            p1.transform.parent = panelsHolder.transform;
            p1.transform.localPosition = new Vector3(-2f, 0f, 0f);
            p1.transform.localScale = new Vector3(3.6f, 0.1f, 2.0f);
            p1.GetComponent<Renderer>().sharedMaterial = solarMat;

            GameObject p2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            p2.transform.parent = panelsHolder.transform;
            p2.transform.localPosition = new Vector3(2f, 0f, 0f);
            p2.transform.localScale = new Vector3(3.6f, 0.1f, 2.0f);
            p2.GetComponent<Renderer>().sharedMaterial = solarMat;

            GameObject terminal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            terminal.name = "Solar Terminal Console";
            terminal.transform.parent = solarObj.transform;
            terminal.transform.position = new Vector3(0f, 0.6f, -1.6f);
            terminal.transform.localScale = new Vector3(0.85f, 1.2f, 0.55f);
            terminal.GetComponent<Renderer>().sharedMaterial = frameMat;

            var solarComp = terminal.AddComponent<SolarTerminal>();
            solarComp.solarPanels = panelsHolder.transform;

            // 9. Interactive Datapad & Supply Crate
            GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crate.name = "Supply Cargo Crate";
            crate.transform.parent = root.transform;
            crate.transform.position = new Vector3(3.2f, 0.5f, -8.0f);
            crate.transform.localScale = new Vector3(1.3f, 1.0f, 1.3f);
            crate.GetComponent<Renderer>().sharedMaterial = frameMat;

            GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pad.name = "Expedition Datapad #042";
            pad.transform.parent = crate.transform;
            pad.transform.localPosition = new Vector3(0f, 0.53f, 0f);
            pad.transform.localScale = new Vector3(0.38f, 0.04f, 0.55f);
            pad.transform.localRotation = Quaternion.Euler(0f, 20f, 0f);
            var padComp = pad.AddComponent<ResearchDatapad>();
            padComp.logTitle = "ARES EXPEDITION - SOL 142 LOG";
            padComp.logContent = "Telemetry calibrated for Mars surface gravity (0.38G). O2 dispensers active. Maintain suit seals and report all geological findings.";

            // 10. Interactive Geological Mineral Samples
            Vector3[] sampleOffsets = new Vector3[]
            {
                new Vector3(-5.5f, 0.35f, -13f),
                new Vector3(6.5f, 0.35f, -15f),
                new Vector3(-12.5f, 0.45f, -7.5f)
            };
            string[] sampleNames = new string[]
            {
                "Hematite Spherule (Blueberry)",
                "Olivine Volcanic Matrix",
                "Hydrated Perchlorate Core"
            };

            for (int i = 0; i < sampleOffsets.Length; i++)
            {
                GameObject sampleObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sampleObj.name = $"Geological Sample [{i + 1}]";
                sampleObj.transform.parent = root.transform;
                sampleObj.transform.position = sampleOffsets[i];
                sampleObj.transform.localScale = new Vector3(0.5f, 0.35f, 0.5f);
                sampleObj.GetComponent<Renderer>().sharedMaterial = crystalMat;

                var sampleComp = sampleObj.AddComponent<MarsSampleRock>();
                sampleComp.sampleType = sampleNames[i];
                sampleComp.scientificValue = 30f + (i * 15f);
            }

            // 11. Interactive Sol Cycle Management Terminal
            GameObject solTermObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            solTermObj.name = "Outpost Sol Terminal";
            solTermObj.transform.parent = root.transform;
            solTermObj.transform.position = new Vector3(-0.8f, 0.75f, -4.6f);
            solTermObj.transform.localScale = new Vector3(0.9f, 1.4f, 0.6f);
            solTermObj.GetComponent<Renderer>().sharedMaterial = frameMat;
            solTermObj.AddComponent<SolTerminal>();

            hasGenerated = true;
        }

        private Material CreateRuntimeMat(Shader shader, Color color, float metallic, float smoothness)
        {
            Material mat = new Material(shader)
            {
                color = color
            };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            return mat;
        }
    }
}

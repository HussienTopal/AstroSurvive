using System.IO;
using UnityEngine;
using UnityEditor;

namespace AstroSurvive.Editor
{
    public static class PlanetaryMaterialGenerator
    {
        private const string PLANET_MAT_DIR = "Assets/Materials/Planets";
        private const string PLANET_TEX_DIR = "Assets/Models/Solar System/textures";
        private const string BLACKHOLE_MAT_DIR = "Assets/Models/black hole/Materials";
        private const string BLACKHOLE_TEX_DIR = "Assets/Models/black hole/textures";
        private const string SHUTTLE_MAT_DIR = "Assets/Models/Space shuttle/Materials";
        private const string SHUTTLE_TEX_DIR = "Assets/Models/Space shuttle/textures";

        [MenuItem("AstroSurvive/Setup All Materials (Planets, Black Hole, Shuttle)")]
        public static void GenerateAllMaterials()
        {
            Debug.Log(">>> Setting up all AstroSurvive URP Materials...");

            EnsureFolder("Assets/Materials");
            EnsureFolder(PLANET_MAT_DIR);

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (unlitShader == null) unlitShader = Shader.Find("Unlit/Texture");

            // 1. Setup Planets
            SetupSun(litShader);
            SetupPlanet("Mercury", litShader, "MercuryAlbedo.jpeg", "MercuryNormal.jpeg", "MercuryEmissive.png");
            SetupPlanet("Venus", litShader, "VenusAlbedo.jpeg", "VenusNormal.jpeg", null);
            SetupEarthAndClouds(litShader);
            SetupPlanet("Moon", litShader, "MoonAlbedo.jpeg", "MoonNormal.jpeg", null);
            SetupPlanet("Mars", litShader, "MarsAlbedoo.jpeg", "MarsNormal.jpeg", null);
            SetupPlanet("Jupiter", litShader, "JupiterAlbedo.jpeg", "JupiterNormal.jpeg", null);
            SetupSaturnAndRings(litShader);
            SetupPlanet("Uranus", litShader, "UranusAlbedo.png", "UranusNormal.jpeg", null);
            SetupPlanet("Neptune", litShader, "NeptuneColor.jpeg", null, null);
            SetupPlanet("Pluto", litShader, "PlutoAlbedo.jpeg", "PlutoNormal.jpeg", null);

            // 2. Fix Black Hole Materials
            FixBlackHoleMaterials(litShader, unlitShader);

            // 3. Fix Space Shuttle Materials
            FixShuttleMaterials(litShader);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(">>> All AstroSurvive Materials configured successfully!");
        }

        private static void SetupSun(Shader litShader)
        {
            Material mat = GetOrCreateMaterial($"{PLANET_MAT_DIR}/Sun.mat", litShader);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{PLANET_TEX_DIR}/SunAlbedo.jpeg");
            if (tex != null) mat.SetTexture("_BaseMap", tex);
            mat.SetColor("_BaseColor", new Color(1f, 0.9f, 0.7f));
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1.0f, 0.65f, 0.15f) * 2.8f);
            if (tex != null) mat.SetTexture("_EmissionMap", tex);
            EditorUtility.SetDirty(mat);
        }

        private static void SetupPlanet(string name, Shader litShader, string albedoName, string normalName, string emissiveName)
        {
            Material mat = GetOrCreateMaterial($"{PLANET_MAT_DIR}/{name}.mat", litShader);
            if (!string.IsNullOrEmpty(albedoName))
            {
                Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>($"{PLANET_TEX_DIR}/{albedoName}");
                if (albedo != null) mat.SetTexture("_BaseMap", albedo);
            }
            if (!string.IsNullOrEmpty(normalName))
            {
                EnsureTextureIsNormalMap($"{PLANET_TEX_DIR}/{normalName}");
                Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>($"{PLANET_TEX_DIR}/{normalName}");
                if (normal != null)
                {
                    mat.EnableKeyword("_NORMALMAP");
                    mat.SetTexture("_BumpMap", normal);
                    mat.SetFloat("_BumpScale", 1.0f);
                }
            }
            if (!string.IsNullOrEmpty(emissiveName))
            {
                Texture2D emissive = AssetDatabase.LoadAssetAtPath<Texture2D>($"{PLANET_TEX_DIR}/{emissiveName}");
                if (emissive != null)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetTexture("_EmissionMap", emissive);
                    mat.SetColor("_EmissionColor", new Color(0.9f, 0.5f, 0.2f) * 1.5f);
                }
            }
            mat.SetFloat("_Smoothness", 0.2f);
            EditorUtility.SetDirty(mat);
        }

        private static void SetupEarthAndClouds(Shader litShader)
        {
            // Earth Body
            SetupPlanet("Earth", litShader, "EartAlbedo.jpeg", "EarthNormal.jpeg", null);

            // Earth Clouds (Transparent outer layer)
            Material cloudMat = GetOrCreateMaterial($"{PLANET_MAT_DIR}/Earth_Clouds.mat", litShader);
            Texture2D cloudTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{PLANET_TEX_DIR}/cloud_combined_2048.png");
            if (cloudTex == null) cloudTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{PLANET_TEX_DIR}/earthcloudmap.jpg");
            if (cloudTex != null)
            {
                cloudMat.SetTexture("_BaseMap", cloudTex);
            }
            cloudMat.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0.65f));
            SetMaterialTransparent(cloudMat);
            EditorUtility.SetDirty(cloudMat);
        }

        private static void SetupSaturnAndRings(Shader litShader)
        {
            SetupPlanet("Saturn", litShader, "SaturnAlbedo.png", null, null);

            Material ringMat = GetOrCreateMaterial($"{PLANET_MAT_DIR}/Saturn_Rings.mat", litShader);
            Texture2D ringTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{PLANET_TEX_DIR}/SaturnAlbedo.png");
            if (ringTex != null) ringMat.SetTexture("_BaseMap", ringTex);
            ringMat.SetColor("_BaseColor", new Color(0.92f, 0.85f, 0.7f, 0.85f));
            ringMat.SetFloat("_Cull", 0); // Double-sided rendering for rings
            SetMaterialTransparent(ringMat);
            EditorUtility.SetDirty(ringMat);
        }

        private static void FixBlackHoleMaterials(Shader litShader, Shader unlitShader)
        {
            EnsureFolder(BLACKHOLE_MAT_DIR);

            // 1. Accretion Disk / Glowing Ring materials
            string[] lightMats = new string[] {
                "blackholelight_1_color",
                "blackholelight_2_color",
                "blackholelight_3_color",
                "blackholering_1_color",
                "blackholering_2_bump"
            };

            Color[] glowColors = new Color[] {
                new Color(1.0f, 0.45f, 0.1f) * 3.5f, // Brilliant warm accretion inner glow
                new Color(0.2f, 0.6f, 1.0f) * 3.0f,  // Energetic cyan/blue relativistic jet glow
                new Color(0.9f, 0.3f, 1.0f) * 2.8f,  // Violet gravitationally redshifted outer disk
                new Color(1.0f, 0.6f, 0.2f) * 2.5f,
                new Color(0.8f, 0.4f, 0.9f) * 2.0f
            };

            for (int i = 0; i < lightMats.Length; i++)
            {
                string matPath = $"{BLACKHOLE_MAT_DIR}/{lightMats[i]}.mat";
                Material mat = GetOrCreateMaterial(matPath, litShader);
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{BLACKHOLE_TEX_DIR}/{lightMats[i]}.png");
                if (tex != null)
                {
                    mat.SetTexture("_BaseMap", tex);
                    mat.SetTexture("_EmissionMap", tex);
                }

                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", glowColors[i % glowColors.Length]);
                mat.SetColor("_BaseColor", Color.white);
                mat.SetFloat("_Cull", 0); // Render both sides of swirling disks
                SetMaterialTransparent(mat);
                EditorUtility.SetDirty(mat);
            }

            // 2. Pure Event Horizon Core (The Singularity Void)
            Material voidMat = GetOrCreateMaterial($"{BLACKHOLE_MAT_DIR}/blackhole_singularity.mat", unlitShader);
            voidMat.SetColor("_BaseColor", Color.black);
            EditorUtility.SetDirty(voidMat);
        }

        private static void FixShuttleMaterials(Shader litShader)
        {
            EnsureFolder(SHUTTLE_MAT_DIR);

            // Rocket Hull Material
            Material hullMat = GetOrCreateMaterial($"{SHUTTLE_MAT_DIR}/spacecraft_rocket_BaseColor.mat", litShader);
            Texture2D baseCol = AssetDatabase.LoadAssetAtPath<Texture2D>($"{SHUTTLE_TEX_DIR}/spacecraft_rocket_BaseColor.png");
            Texture2D normalCol = AssetDatabase.LoadAssetAtPath<Texture2D>($"{SHUTTLE_TEX_DIR}/spacecraft_rocket_Normal.png");
            Texture2D metallicCol = AssetDatabase.LoadAssetAtPath<Texture2D>($"{SHUTTLE_TEX_DIR}/spacecraft_rocket_Metallic.png");
            Texture2D roughCol = AssetDatabase.LoadAssetAtPath<Texture2D>($"{SHUTTLE_TEX_DIR}/spacecraft_rocket_Roughness.png");

            if (baseCol != null) hullMat.SetTexture("_BaseMap", baseCol);
            if (normalCol != null)
            {
                EnsureTextureIsNormalMap($"{SHUTTLE_TEX_DIR}/spacecraft_rocket_Normal.png");
                hullMat.EnableKeyword("_NORMALMAP");
                hullMat.SetTexture("_BumpMap", normalCol);
            }
            if (metallicCol != null) hullMat.SetTexture("_MetallicGlossMap", metallicCol);
            hullMat.SetFloat("_Metallic", 0.75f);
            hullMat.SetFloat("_Smoothness", 0.65f);
            hullMat.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(hullMat);

            // Thruster Glow Material
            Material thrusterMat = GetOrCreateMaterial($"{SHUTTLE_MAT_DIR}/spacecraft_thruster.mat", litShader);
            Texture2D thrusterCol = AssetDatabase.LoadAssetAtPath<Texture2D>($"{SHUTTLE_TEX_DIR}/spacecraft_thruster_BaseColor.png");
            if (thrusterCol != null)
            {
                thrusterMat.SetTexture("_BaseMap", thrusterCol);
                thrusterMat.SetTexture("_EmissionMap", thrusterCol);
            }
            thrusterMat.EnableKeyword("_EMISSION");
            thrusterMat.SetColor("_EmissionColor", new Color(0.1f, 0.75f, 1.0f) * 4.0f); // Bright blue ion thruster flame
            EditorUtility.SetDirty(thrusterMat);
        }

        private static void SetMaterialTransparent(Material mat)
        {
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetFloat("_Blend", 0);   // Alpha
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            mat.SetOverrideTag("RenderType", "Transparent");
        }

        private static void EnsureTextureIsNormalMap(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.NormalMap)
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.SaveAndReimport();
            }
        }

        private static Material GetOrCreateMaterial(string path, Shader shader)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = shader;
            }
            return mat;
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
    }
}

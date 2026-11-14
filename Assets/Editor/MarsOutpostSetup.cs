using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AstroSurvive.Editor
{
    [InitializeOnLoad]
    public static class MarsOutpostSetup
    {
        static MarsOutpostSetup()
        {
            EditorApplication.delayCall += AutoSetupIfMarsSceneOpen;
        }

        private static void AutoSetupIfMarsSceneOpen()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.name.Equals("Mars", StringComparison.OrdinalIgnoreCase))
            {
                if (GameObject.Find("--- OutpostManager ---") == null || GameObject.Find("Mars_Survival_Canvas") == null)
                {
                    Debug.Log("[MarsOutpostSetup] Active scene is Mars. Running auto-setup for OutpostManager and Canvas...");
                    BuildOutpostManagerAndCanvas();
                }
            }
        }

        [MenuItem("AstroSurvive/Build Mars Outpost Manager & Canvas")]
        public static void BuildOutpostManagerAndCanvas()
        {
            Debug.Log(">>> [MarsOutpostSetup] Beginning setup for scene: Mars.unity...");

            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.name.Equals("Mars", StringComparison.OrdinalIgnoreCase))
            {
                string scenePath = "Assets/Scenes/Mars.unity";
                activeScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }

            // 1. Locate or create "--- OutpostManager ---"
            GameObject outpostManagerObj = GameObject.Find("--- OutpostManager ---");
            if (outpostManagerObj == null)
            {
                outpostManagerObj = new GameObject("--- OutpostManager ---");
                Undo.RegisterCreatedObjectUndo(outpostManagerObj, "Create OutpostManager");
            }

            // Ensure components on OutpostManager
            MarsOutpostController controller = outpostManagerObj.GetComponent<MarsOutpostController>();
            if (controller == null)
            {
                controller = outpostManagerObj.AddComponent<MarsOutpostController>();
            }

            NasaDataManager nasaManager = outpostManagerObj.GetComponent<NasaDataManager>();
            if (nasaManager == null)
            {
                nasaManager = outpostManagerObj.AddComponent<NasaDataManager>();
            }

            // 2. Locate scene objects
            GameObject sheild = GameObject.Find("Sheild") ?? GameObject.Find("Shield") ?? GameObject.Find("Copule");
            GameObject solarPanels = GameObject.Find("Solar Panels");
            GameObject plantsHouse = GameObject.Find("Plants House") ?? GameObject.Find("Plants");
            FirstPersonController player = UnityEngine.Object.FindFirstObjectByType<FirstPersonController>();
            Camera playerCam = null;
            if (player != null)
            {
                playerCam = player.playerCamera ?? player.GetComponentInChildren<Camera>();
            }
            if (playerCam == null) playerCam = Camera.main;

            controller.sheildObject = sheild;
            controller.solarPanelsObject = solarPanels;
            controller.plantsHouseObject = plantsHouse;
            controller.playerController = player;
            controller.playerCamera = playerCam;

            Debug.Log($"[MarsOutpostSetup] Objects found: Sheild={(sheild != null)}, SolarPanels={(solarPanels != null)}, PlantsHouse={(plantsHouse != null)}, Player={(player != null)}");

            // 3. Create or replace Mars_Survival_Canvas
            GameObject canvasObj = GameObject.Find("Mars_Survival_Canvas");
            if (canvasObj != null)
            {
                UnityEngine.Object.DestroyImmediate(canvasObj);
            }

            canvasObj = new GameObject("Mars_Survival_Canvas");
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Mars Survival Canvas");

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
            controller.hudCanvas = canvas;

            // Ensure EventSystem
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
            }

            Font standardFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (standardFont == null) standardFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Helper to create Text
            Text CreateText(string name, Transform parent, string content, int fontSize, FontStyle style, Color color, TextAnchor alignment)
            {
                GameObject tObj = new GameObject(name);
                tObj.transform.SetParent(parent, false);
                Text t = tObj.AddComponent<Text>();
                t.font = standardFont;
                t.fontSize = fontSize;
                t.fontStyle = style;
                t.color = color;
                t.alignment = alignment;
                t.text = content;
                t.raycastTarget = false;
                return t;
            }

            // 4. TOP-LEFT: Telemetry HUD Panel
            GameObject tlPanel = new GameObject("TopLeft_TelemetryHUD");
            tlPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform tlRect = tlPanel.AddComponent<RectTransform>();
            tlRect.anchorMin = new Vector2(0f, 1f);
            tlRect.anchorMax = new Vector2(0f, 1f);
            tlRect.pivot = new Vector2(0f, 1f);
            tlRect.anchoredPosition = new Vector2(25f, -25f);
            tlRect.sizeDelta = new Vector2(380f, 290f);

            Image tlBg = tlPanel.AddComponent<Image>();
            tlBg.color = new Color(0.06f, 0.08f, 0.12f, 0.88f);

            // Base Subtitle
            Text baseSub = CreateText("BaseHeader", tlPanel.transform, "ARES EXPEDITION // MARS BASE ALPHA", 13, FontStyle.Bold, new Color(0.6f, 0.72f, 0.85f), TextAnchor.UpperLeft);
            RectTransform baseSubR = baseSub.GetComponent<RectTransform>();
            baseSubR.anchorMin = new Vector2(0f, 1f);
            baseSubR.anchorMax = new Vector2(1f, 1f);
            baseSubR.pivot = new Vector2(0f, 1f);
            baseSubR.anchoredPosition = new Vector2(16f, -14f);
            baseSubR.sizeDelta = new Vector2(-32f, 22f);

            // Sol Text
            Text solTxt = CreateText("SolText", tlPanel.transform, "SOL: 1 / 5", 26, FontStyle.Bold, new Color(1f, 0.45f, 0.28f), TextAnchor.UpperLeft);
            RectTransform solR = solTxt.GetComponent<RectTransform>();
            solR.anchorMin = new Vector2(0f, 1f);
            solR.anchorMax = new Vector2(1f, 1f);
            solR.pivot = new Vector2(0f, 1f);
            solR.anchoredPosition = new Vector2(16f, -38f);
            solR.sizeDelta = new Vector2(-32f, 34f);
            controller.solText = solTxt;

            // Helper for Stat bar container
            (Text, Image) CreateStatRow(string name, Transform parent, float yPos, string label, Color statColor)
            {
                GameObject row = new GameObject(name);
                row.transform.SetParent(parent, false);
                RectTransform rowR = row.AddComponent<RectTransform>();
                rowR.anchorMin = new Vector2(0f, 1f);
                rowR.anchorMax = new Vector2(1f, 1f);
                rowR.pivot = new Vector2(0f, 1f);
                rowR.anchoredPosition = new Vector2(16f, yPos);
                rowR.sizeDelta = new Vector2(-32f, 44f);

                Text labelT = CreateText("Label", row.transform, label, 16, FontStyle.Bold, statColor, TextAnchor.MiddleLeft);
                RectTransform labelR = labelT.GetComponent<RectTransform>();
                labelR.anchorMin = new Vector2(0f, 0.5f);
                labelR.anchorMax = new Vector2(1f, 1f);
                labelR.pivot = new Vector2(0f, 0.5f);
                labelR.anchoredPosition = new Vector2(0f, 0f);
                labelR.sizeDelta = new Vector2(0f, 20f);

                // Bar BG
                GameObject barBG = new GameObject("BarBG");
                barBG.transform.SetParent(row.transform, false);
                RectTransform bgR = barBG.AddComponent<RectTransform>();
                bgR.anchorMin = new Vector2(0f, 0f);
                bgR.anchorMax = new Vector2(1f, 0.45f);
                bgR.anchoredPosition = Vector2.zero;
                bgR.sizeDelta = Vector2.zero;
                Image bgImg = barBG.AddComponent<Image>();
                bgImg.color = new Color(0.12f, 0.16f, 0.22f, 1f);

                // Bar Fill
                GameObject barFill = new GameObject("BarFill");
                barFill.transform.SetParent(barBG.transform, false);
                RectTransform fillR = barFill.AddComponent<RectTransform>();
                fillR.anchorMin = Vector2.zero;
                fillR.anchorMax = Vector2.one;
                fillR.anchoredPosition = Vector2.zero;
                fillR.sizeDelta = Vector2.zero;
                Image fillImg = barFill.AddComponent<Image>();
                fillImg.color = statColor;
                fillImg.type = Image.Type.Filled;
                fillImg.fillMethod = Image.FillMethod.Horizontal;
                fillImg.fillAmount = 0.8f;

                return (labelT, fillImg);
            }

            var (pwrTxt, pwrFill) = CreateStatRow("PowerRow", tlPanel.transform, -80f, "POWER: 80%  (+3%/s)", new Color(1f, 0.82f, 0.23f));
            controller.powerText = pwrTxt;
            controller.powerBar = pwrFill;

            var (o2Txt, o2Fill) = CreateStatRow("OxygenRow", tlPanel.transform, -132f, "OXYGEN: 80%", new Color(0.23f, 0.81f, 1f));
            controller.oxygenText = o2Txt;
            controller.oxygenBar = o2Fill;

            var (shdTxt, shdFill) = CreateStatRow("ShieldRow", tlPanel.transform, -184f, "SHIELD: 50%  (+5%/s)", new Color(0f, 0.95f, 1f));
            controller.shieldText = shdTxt;
            controller.shieldBar = shdFill;

            // Dome Status Text
            Text domeTxt = CreateText("DomeStatusText", tlPanel.transform, "DOME STATUS: ON (SHIELDING ACTIVE)", 15, FontStyle.Bold, new Color(0f, 0.95f, 1f), TextAnchor.UpperLeft);
            RectTransform domeR = domeTxt.GetComponent<RectTransform>();
            domeR.anchorMin = new Vector2(0f, 0f);
            domeR.anchorMax = new Vector2(1f, 0f);
            domeR.pivot = new Vector2(0f, 0f);
            domeR.anchoredPosition = new Vector2(16f, 14f);
            domeR.sizeDelta = new Vector2(-32f, 26f);
            controller.domeStatusText = domeTxt;

            // 5. TOP-RIGHT: Mission Controls Guide
            GameObject trPanel = new GameObject("TopRight_ControlsGuide");
            trPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform trRect = trPanel.AddComponent<RectTransform>();
            trRect.anchorMin = new Vector2(1f, 1f);
            trRect.anchorMax = new Vector2(1f, 1f);
            trRect.pivot = new Vector2(1f, 1f);
            trRect.anchoredPosition = new Vector2(-25f, -25f);
            trRect.sizeDelta = new Vector2(350f, 175f);

            Image trBg = trPanel.AddComponent<Image>();
            trBg.color = new Color(0.06f, 0.08f, 0.12f, 0.88f);

            Text ctrlGuide = CreateText("ControlsText", trPanel.transform, 
                "<b>MISSION CONTROLS:</b>\n" +
                "[1] Toggle Blue Shield Dome\n" +
                "[2] Advance Sol (NASA Storm)\n" +
                "[E] Repair Solar / Harvest O2\n" +
                "[R] Restart Mission", 
                15, FontStyle.Normal, new Color(0.9f, 0.94f, 1f), TextAnchor.UpperLeft);
            RectTransform ctrlR = ctrlGuide.GetComponent<RectTransform>();
            ctrlR.anchorMin = Vector2.zero;
            ctrlR.anchorMax = Vector2.one;
            ctrlR.anchoredPosition = Vector2.zero;
            ctrlR.sizeDelta = new Vector2(-32f, -28f);
            controller.controlsGuideText = ctrlGuide;

            // 6. CENTER: Reticle & Interactive Prompt
            GameObject centerHUD = new GameObject("Center_InteractionHUD");
            centerHUD.transform.SetParent(canvasObj.transform, false);
            RectTransform centerR = centerHUD.AddComponent<RectTransform>();
            centerR.anchorMin = new Vector2(0.5f, 0.5f);
            centerR.anchorMax = new Vector2(0.5f, 0.5f);
            centerR.anchoredPosition = Vector2.zero;
            centerR.sizeDelta = new Vector2(600f, 120f);

            // Crosshair dot
            GameObject dotObj = new GameObject("ReticleDot");
            dotObj.transform.SetParent(centerHUD.transform, false);
            RectTransform dotR = dotObj.AddComponent<RectTransform>();
            dotR.sizeDelta = new Vector2(8f, 8f);
            dotR.anchoredPosition = Vector2.zero;
            Image dotImg = dotObj.AddComponent<Image>();
            dotImg.color = new Color(1f, 1f, 1f, 0.85f);

            // Interaction Prompt Text
            Text promptTxt = CreateText("PromptText", centerHUD.transform, "", 19, FontStyle.Bold, new Color(1f, 0.82f, 0.23f), TextAnchor.MiddleCenter);
            RectTransform promptR = promptTxt.GetComponent<RectTransform>();
            promptR.anchorMin = new Vector2(0f, 0f);
            promptR.anchorMax = new Vector2(1f, 0.5f);
            promptR.anchoredPosition = new Vector2(0f, -25f);
            promptR.sizeDelta = new Vector2(0f, 40f);
            controller.crosshairPromptText = promptTxt;

            // 7. NOTIFICATION BANNER (Mid-screen alerts)
            GameObject notifObj = new GameObject("NotificationBanner");
            notifObj.transform.SetParent(canvasObj.transform, false);
            RectTransform notifR = notifObj.AddComponent<RectTransform>();
            notifR.anchorMin = new Vector2(0.5f, 0.5f);
            notifR.anchorMax = new Vector2(0.5f, 0.5f);
            notifR.anchoredPosition = new Vector2(0f, 160f);
            notifR.sizeDelta = new Vector2(1000f, 50f);

            Text notifTxt = CreateText("NotificationText", notifObj.transform, "", 22, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            RectTransform notifTxtR = notifTxt.GetComponent<RectTransform>();
            notifTxtR.anchorMin = Vector2.zero;
            notifTxtR.anchorMax = Vector2.one;
            notifTxtR.anchoredPosition = Vector2.zero;
            notifTxtR.sizeDelta = Vector2.zero;
            controller.feedbackNotificationText = notifTxt;

            // 8. BOTTOM: Highlighted NASA Science Note Banner
            GameObject botBanner = new GameObject("Bottom_NASABanner");
            botBanner.transform.SetParent(canvasObj.transform, false);
            RectTransform botR = botBanner.AddComponent<RectTransform>();
            botR.anchorMin = new Vector2(0.5f, 0f);
            botR.anchorMax = new Vector2(0.5f, 0f);
            botR.pivot = new Vector2(0.5f, 0f);
            botR.anchoredPosition = new Vector2(0f, 28f);
            botR.sizeDelta = new Vector2(1150f, 130f);

            Image botBg = botBanner.AddComponent<Image>();
            botBg.color = new Color(0.04f, 0.07f, 0.12f, 0.94f);
            controller.scienceBannerPanel = botBanner;

            // NASA Banner Header
            Text bannerTitle = CreateText("BannerTitle", botBanner.transform, "★ NASA DONKI SCIENCE LOG | SOL 1 — CME Impact (Moderate) ★", 17, FontStyle.Bold, new Color(1f, 0.82f, 0.23f), TextAnchor.MiddleCenter);
            RectTransform bTitleR = bannerTitle.GetComponent<RectTransform>();
            bTitleR.anchorMin = new Vector2(0f, 1f);
            bTitleR.anchorMax = new Vector2(1f, 1f);
            bTitleR.pivot = new Vector2(0.5f, 1f);
            bTitleR.anchoredPosition = new Vector2(0f, -12f);
            bTitleR.sizeDelta = new Vector2(-40f, 30f);
            controller.scienceBannerTitle = bannerTitle;

            // NASA Science Note Body
            Text bannerNote = CreateText("BannerNote", botBanner.transform, "\"Coronal Mass Ejections (CMEs) hurl billion-ton clouds of plasma into space, requiring immediate shielding.\"", 16, FontStyle.Normal, new Color(0.92f, 0.96f, 1f), TextAnchor.MiddleCenter);
            RectTransform bNoteR = bannerNote.GetComponent<RectTransform>();
            bNoteR.anchorMin = new Vector2(0f, 0f);
            bNoteR.anchorMax = new Vector2(1f, 1f);
            bNoteR.anchoredPosition = new Vector2(0f, -14f);
            bNoteR.sizeDelta = new Vector2(-60f, -44f);
            controller.scienceBannerNote = bannerNote;

            // 9. MISSION FAILED OVERLAY (Hidden by default)
            GameObject failObj = new GameObject("MissionFailedPanel");
            failObj.transform.SetParent(canvasObj.transform, false);
            RectTransform failR = failObj.AddComponent<RectTransform>();
            failR.anchorMin = Vector2.zero;
            failR.anchorMax = Vector2.one;
            failR.sizeDelta = Vector2.zero;
            Image failBg = failObj.AddComponent<Image>();
            failBg.color = new Color(0.18f, 0f, 0f, 0.92f);

            Text failTitle = CreateText("FailTitle", failObj.transform, "MISSION FAILED", 56, FontStyle.Bold, new Color(1f, 0.2f, 0.25f), TextAnchor.MiddleCenter);
            RectTransform failTitleR = failTitle.GetComponent<RectTransform>();
            failTitleR.anchorMin = new Vector2(0f, 0.5f);
            failTitleR.anchorMax = new Vector2(1f, 0.5f);
            failTitleR.anchoredPosition = new Vector2(0f, 60f);
            failTitleR.sizeDelta = new Vector2(0f, 80f);

            Text failReason = CreateText("FailReason", failObj.transform, "Base Power Depleted! All thermal and life support systems went dark.", 22, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter);
            RectTransform failReasonR = failReason.GetComponent<RectTransform>();
            failReasonR.anchorMin = new Vector2(0f, 0.5f);
            failReasonR.anchorMax = new Vector2(1f, 0.5f);
            failReasonR.anchoredPosition = new Vector2(0f, -10f);
            failReasonR.sizeDelta = new Vector2(1000f, 60f);
            controller.missionFailedReasonText = failReason;

            Text failPrompt = CreateText("FailPrompt", failObj.transform, "Press [R] to Restart Expedition", 20, FontStyle.Bold, new Color(1f, 0.75f, 0.75f), TextAnchor.MiddleCenter);
            RectTransform failPromptR = failPrompt.GetComponent<RectTransform>();
            failPromptR.anchorMin = new Vector2(0f, 0.5f);
            failPromptR.anchorMax = new Vector2(1f, 0.5f);
            failPromptR.anchoredPosition = new Vector2(0f, -70f);
            failPromptR.sizeDelta = new Vector2(0f, 40f);

            controller.missionFailedPanel = failObj;
            failObj.SetActive(false);

            // 10. MISSION ACCOMPLISHED OVERLAY (Hidden by default)
            GameObject winObj = new GameObject("MissionAccomplishedPanel");
            winObj.transform.SetParent(canvasObj.transform, false);
            RectTransform winR = winObj.AddComponent<RectTransform>();
            winR.anchorMin = Vector2.zero;
            winR.anchorMax = Vector2.one;
            winR.sizeDelta = Vector2.zero;
            Image winBg = winObj.AddComponent<Image>();
            winBg.color = new Color(0f, 0.16f, 0.06f, 0.92f);

            Text winTitle = CreateText("WinTitle", winObj.transform, "MISSION ACCOMPLISHED!", 56, FontStyle.Bold, new Color(0.25f, 1f, 0.45f), TextAnchor.MiddleCenter);
            RectTransform winTitleR = winTitle.GetComponent<RectTransform>();
            winTitleR.anchorMin = new Vector2(0f, 0.5f);
            winTitleR.anchorMax = new Vector2(1f, 0.5f);
            winTitleR.anchoredPosition = new Vector2(0f, 70f);
            winTitleR.sizeDelta = new Vector2(0f, 80f);

            Text winDetails = CreateText("WinDetails", winObj.transform, "Congratulations Commander!\nYou successfully defended the Mars Outpost and survived all 5 Sols through severe space weather and solar storms!", 22, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter);
            RectTransform winDetailsR = winDetails.GetComponent<RectTransform>();
            winDetailsR.anchorMin = new Vector2(0f, 0.5f);
            winDetailsR.anchorMax = new Vector2(1f, 0.5f);
            winDetailsR.anchoredPosition = new Vector2(0f, -10f);
            winDetailsR.sizeDelta = new Vector2(1100f, 80f);
            controller.missionAccomplishedText = winDetails;

            Text winPrompt = CreateText("WinPrompt", winObj.transform, "Press [R] to Play Again", 20, FontStyle.Bold, new Color(0.75f, 1f, 0.8f), TextAnchor.MiddleCenter);
            RectTransform winPromptR = winPrompt.GetComponent<RectTransform>();
            winPromptR.anchorMin = new Vector2(0f, 0.5f);
            winPromptR.anchorMax = new Vector2(1f, 0.5f);
            winPromptR.anchoredPosition = new Vector2(0f, -80f);
            winPromptR.sizeDelta = new Vector2(0f, 40f);

            controller.missionAccomplishedPanel = winObj;
            winObj.SetActive(false);

            // Save Scene
            EditorUtility.SetDirty(outpostManagerObj);
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(canvasObj);
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);

            Debug.Log("<color=green><b>>>> [MarsOutpostSetup] SUCCESS! Mars scene configured and saved with 5-Sol Survival loop and Canvas HUD.</b></color>");
        }
    }
}

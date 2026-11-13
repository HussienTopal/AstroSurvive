using UnityEngine;
using AstroSurvive.Player;
using AstroSurvive.Interaction;

namespace AstroSurvive.UI
{
    public class MarsHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerInteraction interaction;

        private static string activeNotification = "";
        private static float notificationTimer = 0f;

        private Texture2D whitePixel;
        private GUIStyle promptStyle;
        private GUIStyle headerStyle;
        private GUIStyle bodyStyle;
        private GUIStyle notificationStyle;
        private GUIStyle boxStyle;

        public static void ShowNotification(string message, float duration = 4.5f)
        {
            activeNotification = message;
            notificationTimer = duration;
        }

        private void Awake()
        {
            if (player == null) player = FindFirstObjectByType<PlayerController>();
            if (interaction == null) interaction = FindFirstObjectByType<PlayerInteraction>();

            whitePixel = new Texture2D(1, 1);
            whitePixel.SetPixel(0, 0, Color.white);
            whitePixel.Apply();
        }

        private void Update()
        {
            if (notificationTimer > 0f)
            {
                notificationTimer -= Time.deltaTime;
                if (notificationTimer <= 0f)
                {
                    activeNotification = "";
                }
            }
        }

        private void InitStyles()
        {
            if (promptStyle != null) return;

            promptStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 17,
                fontStyle = FontStyle.Bold
            };
            promptStyle.normal.textColor = new Color(1f, 0.85f, 0.4f);

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
            headerStyle.normal.textColor = new Color(0.9f, 0.5f, 0.3f);

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal
            };
            bodyStyle.normal.textColor = new Color(0.85f, 0.9f, 0.95f);

            notificationStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
            notificationStyle.normal.textColor = Color.white;

            boxStyle = new GUIStyle(GUI.skin.box);
        }

        private void OnGUI()
        {
            InitStyles();

            DrawReticle();
            DrawInteractionPrompt();
            DrawTelemetryPanel();
            DrawControlsHelp();
            DrawNotifications();
        }

        private void DrawReticle()
        {
            float screenCenterX = Screen.width * 0.5f;
            float screenCenterY = Screen.height * 0.5f;

            bool isAimingAtTarget = interaction != null && interaction.CurrentTarget != null;
            Color reticleColor = isAimingAtTarget ? new Color(1f, 0.75f, 0.2f, 0.95f) : new Color(1f, 1f, 1f, 0.6f);
            float dotSize = isAimingAtTarget ? 7f : 4f;

            DrawColoredRect(new Rect(screenCenterX - dotSize * 0.5f, screenCenterY - dotSize * 0.5f, dotSize, dotSize), reticleColor);

            if (isAimingAtTarget)
            {
                float ringSize = 14f;
                // Subtle brackets
                DrawColoredRect(new Rect(screenCenterX - ringSize, screenCenterY - ringSize, 2f, 6f), reticleColor);
                DrawColoredRect(new Rect(screenCenterX - ringSize, screenCenterY - ringSize, 6f, 2f), reticleColor);

                DrawColoredRect(new Rect(screenCenterX + ringSize - 2f, screenCenterY - ringSize, 2f, 6f), reticleColor);
                DrawColoredRect(new Rect(screenCenterX + ringSize - 6f, screenCenterY - ringSize, 6f, 2f), reticleColor);

                DrawColoredRect(new Rect(screenCenterX - ringSize, screenCenterY + ringSize - 6f, 2f, 6f), reticleColor);
                DrawColoredRect(new Rect(screenCenterX - ringSize, screenCenterY + ringSize - 2f, 6f, 2f), reticleColor);

                DrawColoredRect(new Rect(screenCenterX + ringSize - 2f, screenCenterY + ringSize - 6f, 2f, 6f), reticleColor);
                DrawColoredRect(new Rect(screenCenterX + ringSize - 6f, screenCenterY + ringSize - 2f, 6f, 2f), reticleColor);
            }
        }

        private void DrawInteractionPrompt()
        {
            if (interaction == null || interaction.CurrentTarget == null) return;

            string prompt = interaction.CurrentPrompt;
            if (string.IsNullOrEmpty(prompt)) return;

            float promptW = 380f;
            float promptH = 36f;
            float promptX = (Screen.width - promptW) * 0.5f;
            float promptY = Screen.height * 0.5f + 32f;

            DrawColoredRect(new Rect(promptX, promptY, promptW, promptH), new Color(0.08f, 0.08f, 0.12f, 0.85f));
            DrawOutlineRect(new Rect(promptX, promptY, promptW, promptH), new Color(0.95f, 0.55f, 0.2f, 0.8f), 1.5f);

            GUI.Label(new Rect(promptX, promptY, promptW, promptH), prompt, promptStyle);
        }

        private void DrawTelemetryPanel()
        {
            float panelW = 280f;
            float panelH = 180f;
            float panelX = 20f;
            float panelY = Screen.height - panelH - 25f;

            // Background
            DrawColoredRect(new Rect(panelX, panelY, panelW, panelH), new Color(0.06f, 0.07f, 0.1f, 0.85f));
            DrawOutlineRect(new Rect(panelX, panelY, panelW, panelH), new Color(0.85f, 0.42f, 0.22f, 0.7f), 1.5f);

            // Title
            GUI.Label(new Rect(panelX + 12f, panelY + 8f, 260f, 20f), "EVA SUIT TELEMETRY // MARS 0.38G", headerStyle);

            if (player != null)
            {
                // Oxygen bar
                float o2Pct = player.currentOxygen / player.maxOxygen;
                Color o2Color = o2Pct > 0.4f ? new Color(0.2f, 0.85f, 0.95f) : (o2Pct > 0.2f ? new Color(1f, 0.7f, 0.2f) : new Color(1f, 0.25f, 0.2f));

                GUI.Label(new Rect(panelX + 12f, panelY + 34f, 120f, 18f), $"O2 TANK: {player.currentOxygen:0.0}%", bodyStyle);
                DrawProgressBar(panelX + 12f, panelY + 54f, 256f, 12f, o2Pct, o2Color);

                // Stamina bar
                float stamPct = player.currentStamina / player.maxStamina;
                GUI.Label(new Rect(panelX + 12f, panelY + 72f, 140f, 18f), $"STAMINA: {player.currentStamina:0}%", bodyStyle);
                DrawProgressBar(panelX + 12f, panelY + 92f, 256f, 10f, stamPct, new Color(0.35f, 0.9f, 0.45f));

                // Movement state
                string stateStr = "[WALKING]";
                if (!player.IsGrounded) stateStr = "[LOW-G DRIFT]";
                else if (player.IsCrouching) stateStr = "[CROUCHED]";
                else if (player.IsRunning) stateStr = "[SPRINTING]";

                GUI.Label(new Rect(panelX + 12f, panelY + 112f, 260f, 18f), $"SUIT MODE: {stateStr}", bodyStyle);
            }

            GUI.Label(new Rect(panelX + 12f, panelY + 134f, 260f, 16f), "SURFACE: CHRYSE BASIN", bodyStyle);
            GUI.Label(new Rect(panelX + 12f, panelY + 152f, 260f, 16f), "ATM: 0.087 PSI | TEMP: -63°C", bodyStyle);
        }

        private void DrawControlsHelp()
        {
            float helpW = 230f;
            float helpH = 170f;
            float helpX = Screen.width - helpW - 20f;
            float helpY = 20f;

            DrawColoredRect(new Rect(helpX, helpY, helpW, helpH), new Color(0.06f, 0.07f, 0.1f, 0.85f));
            DrawOutlineRect(new Rect(helpX, helpY, helpW, helpH), new Color(0.4f, 0.7f, 0.9f, 0.6f), 1f);

            GUI.Label(new Rect(helpX + 10f, helpY + 8f, 210f, 20f), "MISSION CONTROLS", headerStyle);
            GUI.Label(new Rect(helpX + 10f, helpY + 30f, 210f, 18f), "[W, A, S, D] Move", bodyStyle);
            GUI.Label(new Rect(helpX + 10f, helpY + 50f, 210f, 18f), "[SHIFT] Run / Sprint", bodyStyle);
            GUI.Label(new Rect(helpX + 10f, helpY + 70f, 210f, 18f), "[CTRL / C] Crouch", bodyStyle);
            GUI.Label(new Rect(helpX + 10f, helpY + 90f, 210f, 18f), "[SPACE] Mars Jump", bodyStyle);
            GUI.Label(new Rect(helpX + 10f, helpY + 110f, 210f, 18f), "[E] Interact with Object", bodyStyle);
            GUI.Label(new Rect(helpX + 10f, helpY + 130f, 210f, 18f), "[ESC] Toggle Mouse Cursor", bodyStyle);
        }

        private void DrawNotifications()
        {
            if (string.IsNullOrEmpty(activeNotification)) return;

            float notifW = 540f;
            float notifH = 50f;
            float notifX = (Screen.width - notifW) * 0.5f;
            float notifY = Screen.height - 110f;

            DrawColoredRect(new Rect(notifX, notifY, notifW, notifH), new Color(0.1f, 0.12f, 0.18f, 0.95f));
            DrawOutlineRect(new Rect(notifX, notifY, notifW, notifH), new Color(0.2f, 0.85f, 1f, 0.85f), 1.5f);

            GUI.Label(new Rect(notifX + 10f, notifY + 5f, notifW - 20f, notifH - 10f), activeNotification, notificationStyle);
        }

        private void DrawProgressBar(float x, float y, float width, float height, float percentage, Color barColor)
        {
            // Background bar
            DrawColoredRect(new Rect(x, y, width, height), new Color(0.12f, 0.14f, 0.18f, 0.9f));
            // Filled bar
            float fillWidth = Mathf.Clamp01(percentage) * width;
            DrawColoredRect(new Rect(x, y, fillWidth, height), barColor);
            // Border
            DrawOutlineRect(new Rect(x, y, width, height), new Color(0.3f, 0.35f, 0.45f, 0.7f), 1f);
        }

        private void DrawColoredRect(Rect rect, Color color)
        {
            Color prev = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, whitePixel);
            GUI.color = prev;
        }

        private void DrawOutlineRect(Rect rect, Color color, float thickness)
        {
            DrawColoredRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            DrawColoredRect(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), color);
            DrawColoredRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            DrawColoredRect(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), color);
        }
    }
}

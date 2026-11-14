using UnityEngine;
using AstroSurvive.Player;
using AstroSurvive.Core;
using AstroSurvive.UI;

namespace AstroSurvive.Interaction
{
    [SelectionBase]
    public class TradeOffLever : MonoBehaviour, IInteractable
    {
        public enum LeverTradeOffType
        {
            ShieldPriority, // Divert Power to Shields (+20 Shield, -30 Power)
            OxygenPriority  // Conserve Power (-40 Shield, -10 Power, -20% Oxygen)
        }

        [Header("Trade-Off Type")]
        public LeverTradeOffType tradeOffType = LeverTradeOffType.ShieldPriority;
        public string leverTitle = "Divert Power to Shields";

        [Header("Stat Effects")]
        public float powerDelta = -30f;
        public float shieldDelta = 20f;
        public float oxygenDeltaPercent = 0f;

        [Header("Visual Feedback (Easily Swappable)")]
        [Tooltip("Transform of the lever handle to rotate when pulled")]
        public Transform leverHandle;
        public Vector3 neutralRotation = new Vector3(30f, 0f, 0f);
        public Vector3 pulledRotation = new Vector3(-30f, 0f, 0f);
        public Light indicatorLight;
        public Color activeColor = new Color(0.2f, 0.9f, 0.3f);
        public Color lockedColor = new Color(1f, 0.25f, 0.15f);

        // Static tracking across all trade-off levers
        private static bool s_LeversLockedThisSol = false;
        private bool isPulled = false;

        public static bool LeversLockedThisSol => s_LeversLockedThisSol;

        private void Start()
        {
            if (SolGameManager.Instance != null)
            {
                SolGameManager.Instance.OnSolChanged += HandleSolChanged;
            }

            UpdateVisuals();
        }

        private void OnDestroy()
        {
            if (SolGameManager.Instance != null)
            {
                SolGameManager.Instance.OnSolChanged -= HandleSolChanged;
            }
        }

        private void Update()
        {
            // Smooth handle rotation
            if (leverHandle != null)
            {
                Vector3 targetEuler = isPulled ? pulledRotation : neutralRotation;
                leverHandle.localRotation = Quaternion.Lerp(leverHandle.localRotation, Quaternion.Euler(targetEuler), Time.deltaTime * 8f);
            }
        }

        private void HandleSolChanged(int newSol)
        {
            // Re-enable levers for the new Sol
            s_LeversLockedThisSol = false;
            isPulled = false;
            UpdateVisuals();
        }

        public string GetInteractionPrompt()
        {
            if (s_LeversLockedThisSol)
            {
                return $"[Locked] {leverTitle} (Active for Sol {SolGameManager.Instance?.CurrentSol})";
            }

            string statsHint = $"Power {powerDelta:+0;-0}%, Shield {shieldDelta:+0;-0}%";
            if (Mathf.Abs(oxygenDeltaPercent) > 0.01f) statsHint += $", O2 {oxygenDeltaPercent:+0;-0}%";

            return $"[E] Pull Lever: {leverTitle} ({statsHint})";
        }

        public void Interact(PlayerController player)
        {
            if (s_LeversLockedThisSol)
            {
                MarsHUD.ShowNotification("Daily trade-off already locked! Advance to the next Sol to re-route systems.", 3.5f);
                return;
            }

            // Lock all levers until the next Sol
            s_LeversLockedThisSol = true;
            isPulled = true;
            UpdateVisuals();

            // Apply trade-off stats
            if (SolGameManager.Instance != null)
            {
                SolGameManager.Instance.ApplyTradeOff(powerDelta, shieldDelta, oxygenDeltaPercent);
            }

            // Retrieve NASA note and display on HUD for 5 seconds
            string educationalNote = "";
            if (NasaEventManager.Instance != null)
            {
                var evt = NasaEventManager.Instance.GetCurrentEvent();
                if (evt != null)
                {
                    educationalNote = evt.educational_note;
                }
            }

            if (string.IsNullOrEmpty(educationalNote))
            {
                educationalNote = "Coronal Mass Ejections (CMEs) hurl billion-ton clouds of plasma into space, requiring immediate shielding.";
            }

            MarsHUD.ShowNasaWarning(educationalNote, 5f);
        }

        private void UpdateVisuals()
        {
            if (indicatorLight != null)
            {
                indicatorLight.color = s_LeversLockedThisSol ? lockedColor : activeColor;
            }
        }

        public static void UnlockLevers()
        {
            s_LeversLockedThisSol = false;
        }
    }
}

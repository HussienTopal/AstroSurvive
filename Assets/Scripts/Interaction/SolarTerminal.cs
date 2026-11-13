using UnityEngine;
using AstroSurvive.Player;
using AstroSurvive.UI;

namespace AstroSurvive.Interaction
{
    public class SolarTerminal : InteractableBase
    {
        [Header("Solar Array System")]
        public Light outpostBeacon;
        public Transform solarPanels;
        public bool isOnline = true;

        public override string GetInteractionPrompt()
        {
            return isOnline ? "[E] Re-align Solar Arrays to Standby" : "[E] Engage Solar Array Sun-Tracking";
        }

        public override void Interact(PlayerController player)
        {
            isOnline = !isOnline;

            if (outpostBeacon != null)
            {
                outpostBeacon.enabled = isOnline;
            }

            if (solarPanels != null)
            {
                solarPanels.Rotate(Vector3.right, isOnline ? 25f : -25f, Space.Self);
            }

            MarsHUD.ShowNotification(isOnline ? "Solar Grid: ONLINE (Generating 14.2 kW)" : "Solar Grid: STANDBY");
            base.Interact(player);
        }
    }
}

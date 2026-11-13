using UnityEngine;
using AstroSurvive.Player;
using AstroSurvive.UI;

namespace AstroSurvive.Interaction
{
    public class OxygenStation : InteractableBase
    {
        [Header("Oxygen Dispenser")]
        public float refillAmount = 50f;
        public Light statusLight;
        public Color normalColor = new Color(0.2f, 0.8f, 1f);
        public Color emptyColor = new Color(1f, 0.3f, 0.1f);

        [SerializeField] private int remainingRefills = 5;

        private void Start()
        {
            UpdateLight();
        }

        public override string GetInteractionPrompt()
        {
            if (remainingRefills <= 0)
                return "Oxygen Station Depleted";

            return $"[E] Dispense O2 (+{refillAmount:0}%) [{remainingRefills} left]";
        }

        public override void Interact(PlayerController player)
        {
            if (remainingRefills <= 0)
            {
                MarsHUD.ShowNotification("Oxygen tank dispenser empty!");
                return;
            }

            if (player != null)
            {
                player.RefillOxygen(refillAmount);
                remainingRefills--;
                UpdateLight();
                MarsHUD.ShowNotification($"Oxygen replenished (+{refillAmount:0}%). Tanks status: {player.currentOxygen:0}%");
                base.Interact(player);
            }
        }

        private void UpdateLight()
        {
            if (statusLight != null)
            {
                statusLight.color = remainingRefills > 0 ? normalColor : emptyColor;
            }
        }
    }
}

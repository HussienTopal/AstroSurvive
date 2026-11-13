using UnityEngine;
using AstroSurvive.Player;
using AstroSurvive.UI;

namespace AstroSurvive.Interaction
{
    public class MarsSampleRock : InteractableBase
    {
        [Header("Sample Details")]
        public string sampleType = "Hematite / Olivine Crystal";
        public float scientificValue = 25f;

        public override string GetInteractionPrompt()
        {
            if (hasBeenUsed)
                return string.Empty;

            return $"[E] Extract Geological Core ({sampleType})";
        }

        public override void Interact(PlayerController player)
        {
            if (hasBeenUsed) return;

            hasBeenUsed = true;
            MarsHUD.ShowNotification($"Sample Acquired: {sampleType} (+{scientificValue} Science Units)");
            
            // Subtle collection effect: shrink and disable collider
            var collider = GetComponent<Collider>();
            if (collider != null) collider.enabled = false;

            transform.localScale *= 0.5f;

            base.Interact(player);
        }
    }
}

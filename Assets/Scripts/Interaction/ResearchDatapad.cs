using UnityEngine;
using AstroSurvive.Player;
using AstroSurvive.UI;

namespace AstroSurvive.Interaction
{
    public class ResearchDatapad : InteractableBase
    {
        [Header("Datapad Log")]
        public string logTitle = "Mission Log SOL 142";
        [TextArea(3, 8)]
        public string logContent = "Atmospheric sensors confirm trace methane in the Jezero crater basin. Maintain suit seals and report to habitat module before dust front arrives.";

        public override string GetInteractionPrompt()
        {
            return $"[E] Read Datapad: {logTitle}";
        }

        public override void Interact(PlayerController player)
        {
            MarsHUD.ShowNotification($"[{logTitle}]\n\"{logContent}\"", 7.5f);
            base.Interact(player);
        }
    }
}
